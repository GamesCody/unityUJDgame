using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using UnityEngine.InputSystem;
using System;

public class InkDialogController : MonoBehaviour
{
    private PlayerInteraction playerInteractionRef = null;

    // ===== EVENT SYSTEM =====
    public delegate void OnDialogLineChanged(string line, string speaker);
    public static event OnDialogLineChanged dialogLineChanged;

    public delegate void OnDialogStartedEvent();
    public static event OnDialogStartedEvent OnDialogStarted;

    public delegate void OnDialogEndedEvent();
    public static event OnDialogEndedEvent OnDialogEnded;

    // 🔥 GLOBALNA FLAGA
    public static bool IsAnyDialogActive = false;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI textBox;
    public Button[] choiceButtons = new Button[4];
    public TextMeshProUGUI[] choiceLabels = new TextMeshProUGUI[4];

    [Header("Speaker UI")]
    public TextMeshProUGUI speakerNameText;

    [Header("Settings")]
    public float textSpeed = 0.03f;
    public float autoAdvanceDelay = 1.5f;

    // =========================================================
    // 🔥 NOWE: SYSTEM AUDIO (Dodane bez usuwania reszty)
    // =========================================================
    [Header("Audio System")]
    public AudioSource audioSource; // Przeciągnij tu AudioSource z inspektora

    [System.Serializable]
    public class DialogueAudioEntry
    {
        public string id;       // Np. "windows" (używane w tagu #audio:windows)
        public AudioClip clip;  // Plik dźwiękowy
    }
    public List<DialogueAudioEntry> audioClips = new List<DialogueAudioEntry>();
    // =========================================================

    [Header("Ink")]
    public TextAsset inkJSONAsset;

    private Story story;
    private Dictionary<string, object> pendingVariables = new Dictionary<string, object>();
    
    [System.Serializable]
    public class InkVariableEntry
    {
        public string name;
        public string type;
        public string value;
    }

    [System.Serializable]
    public class InkVariableCollection
    {
        public List<InkVariableEntry> variables = new List<InkVariableEntry>();
    }

    private Dictionary<string, InkVariableEntry> savedVariables = new Dictionary<string, InkVariableEntry>();

    [Header("Dialog Identity")]
    public string dialogId = "defaultDialog";
    private string PlayerPrefsKey => $"InkVariables_{dialogId}_v1";

    [System.Serializable]
    public class DefaultInkVariable
    {
        public string name;
        public string value;
    }

    [System.Serializable]
    public class SpeakerDefinition
    {
        public string id;          // np. "blacksmith"
        public string displayName; // np. "Kowal"
    }

    [Header("Defaults")]
    public List<DefaultInkVariable> defaultVariables = new List<DefaultInkVariable>();

    [Header("Speakers")]
    public List<SpeakerDefinition> speakers = new List<SpeakerDefinition>();

    private Dictionary<string, string> speakerMap;
    private string currentSpeaker = "";

    [Header("Persistence")]
    public bool clearSavedOnStart = true;
    [Header("Game Start")]
    public bool clearOnGameStart = true;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine autoAdvanceRoutine;
    private string lastLine = "";

    private bool prevCursorVisible = false;
    private CursorLockMode prevCursorLockState = CursorLockMode.None;

    // GLOBALNA BLOKADA INTERAKCJI
    public bool IsDialogActive => panel != null && panel.activeSelf;

    // 🔥 Flaga statyczna - współdzielona przez wszystkie instancje skryptu
    private static bool _globalGameStarted = false;
    
    // 🔥 Flaga do śledzenia czy już robiliśmy reset dla tego NPC w tej sesji
    private static HashSet<string> _resetNPCsThisSession = new HashSet<string>();

    void Awake()
    {
        // 🔥 ZAWSZE odblokuj graczowi kontrolę gdy scena się załaduje
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = true;
            if (player.TryGetComponent<CharacterController>(out var controller)) controller.enabled = true;
            if (player.TryGetComponent<PlayerInteraction>(out var pi)) pi.enabled = true;
        }
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Czyścimy wszystko tylko raz na cały proces uruchomienia gry
        if (clearOnGameStart && !_globalGameStarted)
        {
            Debug.Log("[InkDialogController] Wykryto start gry. Czyszczenie wszystkich zapisanych stanów Ink...");
            
            // 1. Czyścimy zmienne globalne
            PlayerPrefs.DeleteKey("InkVariables_global_v1");

            // 2. Czyścimy konkretne ID tego obiektu
            PlayerPrefs.DeleteKey(PlayerPrefsKey);

            // 3. 🔥 CZYŚCIMY WSZYSTKIE MOŻLIWE KLUCZE NPC (bo dialogId może się zmienić dynamicznie)
            for (int i = 1; i <= 50; i++)
            {
                PlayerPrefs.DeleteKey($"InkVariables_npc_{i}_v1");
            }
            
            PlayerPrefs.Save();
            _globalGameStarted = true; // Zaznaczamy, że reset już się odbył
            _resetNPCsThisSession.Clear(); // Resetujemy tracking NPCów
            
            // Clear in-memory saved/pending variables
            savedVariables.Clear();
            pendingVariables.Clear();
        }
        else if (clearOnGameStart)
        {
            // Jeśli to nie jest pierwszy skrypt w tej sesji, czyścimy tylko klucz przypisany do TEGO konkretnego NPC
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        // Build speaker map
        BuildSpeakerMap();

        // Load saved NPC-specific variables
        LoadSavedVariables();
        
        // Also load global variables (don't clear them)
        LoadGlobalVariables();
        
        panel.SetActive(false);
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && panel.activeSelf)
        {
            OnAdvanceKey();
        }
    }

    void OnAdvanceKey()
    {
        if (story == null) return;

        if (isTyping)
        {
            StopTypingCoroutine();
            textBox.text = lastLine;
            isTyping = false;

            if (story.currentChoices != null && story.currentChoices.Count > 0)
            {
                ShowChoices();
                return;
            }

            if (story.canContinue)
                TryAutoAdvance();

            return;
        }

        if (story.currentChoices.Count > 0) return;

        if (story.canContinue)
        {
            ContinueStory();
            return;
        }

        EndDialog();
    }

    public void StartDialog()
    {
        IsAnyDialogActive = true; // 🔥 FLAGA NA POCZĄTKU

        if (inkJSONAsset == null || string.IsNullOrEmpty(inkJSONAsset.text))
        {
            Debug.LogWarning("[InkDialogController] Brak przypisanego pliku Ink lub jest pusty.");
            panel.SetActive(false);
            return;
        }

        story = new Story(inkJSONAsset.text);

        // Reload saved variables for the current dialogId (in case dialogId was changed just before StartDialog)
        LoadSavedVariables();
        LoadGlobalVariables();

        // Apply saved persistent variables to this new story
        ApplySavedVariablesToStory();

        // Apply inspector-defined default variables if they aren't already persisted
        if (defaultVariables != null)
        {
            foreach (var v in defaultVariables)
            {
                if (string.IsNullOrEmpty(v.name)) continue;
                if (savedVariables.ContainsKey(v.name)) continue;

                if (story.variablesState.GlobalVariableExistsWithName(v.name))
                {
                    story.variablesState[v.name] = ParseStringToObject(v.value);
                }
                else
                {
                    Debug.LogWarning($"[InkDialogController] Default variable '{v.name}' not declared in the Ink story for dialog '{dialogId}', skipping.");
                }
            }
        }

        // Zastosuj oczekujące zmienne (tylko te, które istnieją w story). Nie usuwamy tych, które nie istnieją —
        // mogą dotyczyć innego story/ID i pozostaną w pending.
        var keysToRemove = new List<string>();
        foreach (var kv in pendingVariables)
        {
            if (story.variablesState.GlobalVariableExistsWithName(kv.Key))
            {
                story.variablesState[kv.Key] = kv.Value;
                keysToRemove.Add(kv.Key);
            }
            else
            {
                Debug.LogWarning($"[InkDialogController] Pending variable '{kv.Key}' not declared in the Ink story for dialog '{dialogId}', keeping pending.");
            }
        }
        foreach (var k in keysToRemove)
            pendingVariables.Remove(k);

        panel.SetActive(true);
        DisablePlayerControls();
        
        // Emit OnDialogStarted event
        OnDialogStarted?.Invoke();
        
        // Wyłącz PlayerInteraction script i interactionUI object
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInteraction playerInt = player.GetComponent<PlayerInteraction>();
            if (playerInt != null)
            {
                playerInt.enabled = false;
                if (playerInt.interactionUI != null)
                    playerInt.interactionUI.SetActive(false);
            }
        }
        
        ContinueStory();
    }

    // Overload z zewnętrznym plikiem i zmiennymi
    public void StartDialog(TextAsset inkAsset, Dictionary<string, object> initialVariables = null)
    {
        IsAnyDialogActive = true; // 🔥 FLAGA NA POCZĄTKU

        inkJSONAsset = inkAsset;
        story = new Story(inkJSONAsset.text);

        // Reload saved variables for the current dialogId
        LoadSavedVariables();
        LoadGlobalVariables();

        // Apply saved persistent variables first
        ApplySavedVariablesToStory();

        if (initialVariables != null)
        {
            foreach (var kv in initialVariables)
            {
                if (story.variablesState.GlobalVariableExistsWithName(kv.Key))
                    story.variablesState[kv.Key] = kv.Value;
                else
                    Debug.LogWarning($"[InkDialogController] initialVariables contains '{kv.Key}' which is not declared in the Ink story for dialog '{dialogId}', skipping.");
            }
        }

        // Apply inspector-defined default variables if they aren't already persisted or provided via initialVariables
        if (defaultVariables != null)
        {
            foreach (var v in defaultVariables)
            {
                if (string.IsNullOrEmpty(v.name)) continue;
                if (savedVariables.ContainsKey(v.name)) continue;
                if (initialVariables != null && initialVariables.ContainsKey(v.name)) continue;
                story.variablesState[v.name] = ParseStringToObject(v.value);
            }
        }

        // Zastosuj oczekujące zmienne (tylko te, które istnieją w story)
        var keysToRemove2 = new List<string>();
        foreach (var kv in pendingVariables)
        {
            if (story.variablesState.GlobalVariableExistsWithName(kv.Key))
            {
                story.variablesState[kv.Key] = kv.Value;
                keysToRemove2.Add(kv.Key);
            }
            else
            {
                Debug.LogWarning($"[InkDialogController] Pending variable '{kv.Key}' not declared in the Ink story for dialog '{dialogId}', keeping pending.");
            }
        }
        foreach (var k in keysToRemove2)
            pendingVariables.Remove(k);

        panel.SetActive(true);
        DisablePlayerControls();
        
        // Emit OnDialogStarted event
        OnDialogStarted?.Invoke();
        
        // Wyłącz PlayerInteraction script i interactionUI object
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInteraction playerInt = player.GetComponent<PlayerInteraction>();
            if (playerInt != null)
            {
                playerInt.enabled = false;
                if (playerInt.interactionUI != null)
                    playerInt.interactionUI.SetActive(false);
            }
        }
        
        ContinueStory();
    }

    void ContinueStory()
    {
        CancelAutoAdvance();
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        if (story == null)
        {
            EndDialog();
            return;
        }

        if (story.canContinue)
        {
            lastLine = story.Continue()?.Trim() ?? "";
            ProcessTags();
            StopTypingCoroutine();
            typingCoroutine = StartCoroutine(TypeText(lastLine));

            // Save any variables changed by the story so they persist
            SaveStoryVariablesToPrefs();

            if (!story.canContinue && (story.currentChoices == null || story.currentChoices.Count == 0))
                ScheduleAutoEnd();
        }
        else if (story.currentChoices != null && story.currentChoices.Count > 0)
        {
            ShowChoices();
        }
        else
        {
            EndDialog();
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        textBox.text = "";

        foreach (char c in text)
        {
            textBox.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (story == null) yield break;

        if (story.canContinue)
            TryAutoAdvance();
        else if (story.currentChoices != null && story.currentChoices.Count > 0)
            ShowChoices();
    }

    void ShowChoices()
    {
        var choices = story.currentChoices;
        for (int i = 0; i < choiceButtons.Length; i++)
            choiceButtons[i].gameObject.SetActive(false);

        for (int i = 0; i < choices.Count && i < 4; i++)
        {
            choiceLabels[i].text = choices[i].text;
            choiceButtons[i].gameObject.SetActive(true);

            int choiceIndex = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoice(choiceIndex));
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void TryAutoAdvance()
    {
        if (autoAdvanceDelay > 0)
            autoAdvanceRoutine = StartCoroutine(AutoAdvance());
    }

    void LoadSavedVariables()
    {
        savedVariables.Clear();
        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            return;
        string json = PlayerPrefs.GetString(PlayerPrefsKey);
        if (string.IsNullOrEmpty(json)) return;
        var collection = JsonUtility.FromJson<InkVariableCollection>(json);
        if (collection == null || collection.variables == null) return;
        foreach (var e in collection.variables)
        {
            if (string.IsNullOrEmpty(e.name)) continue;
            savedVariables[e.name] = e;
        }
    }

    void LoadGlobalVariables()
    {
        // Load global variables that should persist across all dialogs
        string globalKey = "InkVariables_global_v1";
        if (!PlayerPrefs.HasKey(globalKey))
            return;
        string json = PlayerPrefs.GetString(globalKey);
        if (string.IsNullOrEmpty(json)) return;
        var collection = JsonUtility.FromJson<InkVariableCollection>(json);
        if (collection == null || collection.variables == null) return;
        foreach (var e in collection.variables)
        {
            if (string.IsNullOrEmpty(e.name)) continue;
            // Add to savedVariables so they get applied to story
            savedVariables[e.name] = e;
        }
    }

    void SaveSavedVariables()
    {
        var collection = new InkVariableCollection();
        foreach (var kv in savedVariables)
            collection.variables.Add(kv.Value);
        string json = JsonUtility.ToJson(collection);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    public void ClearSavedVariables()
    {
        savedVariables.Clear();
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }
    }

    void ApplySavedVariablesToStory()
    {
        if (story == null) return;
        
        // Apply all saved variables (including global ones loaded in LoadGlobalVariables)
        foreach (var kv in savedVariables)
        {
            var entry = kv.Value;
            if (string.IsNullOrEmpty(entry.name)) continue;
            object parsed = ParseSavedValue(entry.type, entry.value);

            try
            {
                // Only assign if the Ink story actually declares the variable
                if (story.variablesState.GlobalVariableExistsWithName(entry.name))
                {
                    story.variablesState[entry.name] = parsed;
                    Debug.Log($"[InkDialogController] Applied variable '{entry.name}' = {parsed}");
                }
                else
                {
                    Debug.LogWarning($"[InkDialogController] Saved variable '{entry.name}' not declared in the Ink story for dialog '{dialogId}', skipping.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[InkDialogController] Failed applying saved variable '{entry.name}': {ex.Message}");
            }
        }
    }

    object ParseSavedValue(string type, string value)
    {
        if (type == "bool")
        {
            return value == "1" || value.ToLower() == "true";
        }
        else if (type == "int")
        {
            if (int.TryParse(value, out int vi)) return vi;
            return 0;
        }
        else if (type == "float")
        {
            if (float.TryParse(value, out float vf)) return vf;
            return 0f;
        }
        else // string
        {
            return value ?? "";
        }
    }

    // Parse a plain string into bool/int/float/string for default values
    object ParseStringToObject(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var vLower = value.ToLower();
        if (vLower == "true" || vLower == "false")
            return vLower == "true";
        if (int.TryParse(value, out int vi)) return vi;
        if (float.TryParse(value, out float vf)) return vf;
        return value;
    }

    void SaveVariableToDict(string name, object value)
    {
        if (string.IsNullOrEmpty(name)) return;
        var entry = new InkVariableEntry();
        entry.name = name;
        if (value is bool b)
        {
            entry.type = "bool";
            entry.value = b ? "true" : "false";
        }
        else if (value is int i)
        {
            entry.type = "int";
            entry.value = i.ToString();
        }
        else if (value is float f)
        {
            entry.type = "float";
            entry.value = f.ToString("R");
        }
        else
        {
            entry.type = "string";
            entry.value = value?.ToString() ?? "";
        }
        savedVariables[name] = entry;
    }

    void SaveStoryVariablesToPrefs()
    {
        if (story == null) return;
        try
        {
            Debug.Log($"[InkDialogController] 🔥 SaveStoryVariablesToPrefs() - Zapisuję zmienne do klucza: {PlayerPrefsKey}");
            
            // story.variablesState enumerates variable names (strings), so iterate names
            foreach (string varName in story.variablesState)
            {
                object val = null;
                try { val = story.variablesState[varName]; } catch { val = null; }
                Debug.Log($"[InkDialogController] Saving: {varName} = {val}");
                SaveVariableToDict(varName, val);
            }
            SaveSavedVariables();
            Debug.Log($"[InkDialogController] ✅ Wszystkie zmienne zapisane do PlayerPrefs");
        }
        catch { }
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSecondsRealtime(autoAdvanceDelay);
        if (!isTyping) ContinueStory();
    }

    void ScheduleAutoEnd()
    {
        autoAdvanceRoutine = StartCoroutine(AutoEnd());
    }

    IEnumerator AutoEnd()
    {
        yield return new WaitForSecondsRealtime(autoAdvanceDelay);
        EndDialog();
    }

    void CancelAutoAdvance()
    {
        if (autoAdvanceRoutine != null)
        {
            StopCoroutine(autoAdvanceRoutine);
            autoAdvanceRoutine = null;
        }
    }

    void BuildSpeakerMap()
    {
        speakerMap = new Dictionary<string, string>();
        foreach (var s in speakers)
        {
            if (!string.IsNullOrEmpty(s.id))
                speakerMap[s.id] = s.displayName;
        }
    }

    void ProcessTags()
    {
        if (story == null || story.currentTags == null) return;

        foreach (string tag in story.currentTags)
        {
            if (tag.StartsWith("speaker:"))
            {
                string speakerId = tag.Substring("speaker:".Length).Trim();
                currentSpeaker = speakerId;
                SetSpeaker(speakerId);
            }
            // =========================================================
            // 🔥 NOWE: Obsługa tagu audio
            // =========================================================
            else if (tag.StartsWith("audio:"))
            {
                // Format: #audio:nazwa_dzwieku
                string audioId = tag.Substring("audio:".Length).Trim();
                PlayAudioClip(audioId);
            }
            // =========================================================
            // 🔥 Obsługa zakończenia questa Maćka
            else if (tag == "maciekQuestComplete")
            {
                OnMaciekQuestComplete();
            }
            // 🔥 Obsługa aktywacji boss fight'u
            else if (tag == "bossFightTriggered")
            {
                OnBossFightTriggered();
            }
        }

        // Emit event with current line and speaker
        dialogLineChanged?.Invoke(lastLine, currentSpeaker);
    }

    // =========================================================
    // 🔥 NOWE: Funkcja odtwarzająca dźwięk
    // =========================================================
    void PlayAudioClip(string audioId)
    {
        if (audioSource == null) return;

        // Szukamy klipu w liście zdefiniowanej w inspektorze
        DialogueAudioEntry entry = audioClips.Find(x => x.id == audioId);
        
        if (entry != null && entry.clip != null)
        {
            audioSource.Stop(); // Zatrzymaj poprzedni dźwięk
            audioSource.clip = entry.clip;
            audioSource.Play();
            Debug.Log($"[InkDialogController] Playing audio: {audioId}");
        }
        else
        {
            Debug.LogWarning($"[InkDialogController] Audio ID '{audioId}' not found in list!");
        }
    }
    // =========================================================

    void SetSpeaker(string speakerId)
    {
        if (speakerNameText == null) return;

        if (speakerMap != null && speakerMap.TryGetValue(speakerId, out string display))
            speakerNameText.text = display;
        else
            speakerNameText.text = speakerId; // fallback

        Debug.Log($"[InkDialogController] Speaker changed to: {speakerId}");
    }

    // 🔥 Callback dla zakończenia questa Maćka
    void OnMaciekQuestComplete()
    {
        Debug.Log("[InkDialogController] ⚠️ OnMaciekQuestComplete() CALLED!");
        
        // Ustaw zmienną w story (dla bieżącego dialogu)
        SetInkVariable(QuestVariables.MACIEK_QUEST_COMPLETE, true);
        Debug.Log($"[InkDialogController] Set local story variable: {QuestVariables.MACIEK_QUEST_COMPLETE} = true");

        // Natychmiastowy transfer do profesora: koniecQuestMacka → poprawkaZGrafow
        Debug.Log($"[InkDialogController] Starting transfer: {QuestVariables.MACIEK_NPC_ID}.{QuestVariables.MACIEK_QUEST_COMPLETE} → {QuestVariables.PROFESOR_NPC_ID}.{QuestVariables.PROFESOR_EXAM_UNLOCKED}");
        
        InkVariableTransfer.TransferVariableBool(
            QuestVariables.MACIEK_NPC_ID, 
            QuestVariables.MACIEK_QUEST_COMPLETE,
            QuestVariables.PROFESOR_NPC_ID, 
            QuestVariables.PROFESOR_EXAM_UNLOCKED);

        // 🔥 Ustaw bezpośrednio publiczne pole
        InkVariableTransfer.Q2 = true;
        Debug.Log("[InkDialogController] ✅ Ustawiono InkVariableTransfer.Q2 (poprawkaZGrafow) = true");

        Debug.Log("[InkDialogController] 🎉 Quest Maćka zakończony, profesor powiadomiony (poprawkaZGrafow=true)");
    }

    // 🔥 NOWY CALLBACK: Boss fight aktywowany
    void OnBossFightTriggered()
    {
        Debug.Log("[InkDialogController] 🔥 OnBossFightTriggered() CALLED!");
        
        // Ustaw zmienną w story (dla bieżącego dialogu)
        SetInkVariable(QuestVariables.PROFESOR_BOSS_FIGHT_TRIGGERED, true);
        Debug.Log($"[InkDialogController] Set local story variable: {QuestVariables.PROFESOR_BOSS_FIGHT_TRIGGERED} = true");

        // Zapisz do PlayerPrefs dla Profesora
        InkVariableTransfer.WriteVariableBool(
            QuestVariables.PROFESOR_NPC_ID,
            QuestVariables.PROFESOR_BOSS_FIGHT_TRIGGERED,
            true);

        // 🔥 Ustaw bezpośrednio Q3 property
        InkVariableTransfer.Q3 = true;
        Debug.Log("[InkDialogController] ✅ Ustawiono InkVariableTransfer.Q3 (walkaBoss1) = true");

        // Załaduj scenę boss fight'u
        Debug.Log("[InkDialogController] 🎮 Ładuję scenę BattleScene za 1 sekundę...");
        StartCoroutine(LoadBattleSceneCoroutine());

        Debug.Log("[InkDialogController] 🎉 Boss fight aktywowany!");
    }

    // 🔥 Coroutine do opóźnionego załadowania sceny
    IEnumerator LoadBattleSceneCoroutine()
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    void StopTypingCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    void OnChoice(int choiceIndex)
    {
        story.ChooseChoiceIndex(choiceIndex);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ContinueStory();
    }

    void EndDialog()
    {
        panel.SetActive(false);
        textBox.text = "";

        // =========================================================
        // 🔥 NOWE: Zatrzymaj dźwięk po zamknięciu okna
        if (audioSource != null) audioSource.Stop();
        // =========================================================

        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevCursorLockState;

        EnablePlayerControls();
        
        // Emit OnDialogEnded event
        OnDialogEnded?.Invoke();
        
        // Włącz PlayerInteraction script i interactionUI object z powrotem
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInteraction playerInt = player.GetComponent<PlayerInteraction>();
            if (playerInt != null)
            {
                playerInt.enabled = true;
                if (playerInt.interactionUI != null)
                    playerInt.interactionUI.SetActive(true);
            }
        }
        
        story = null;
        
        IsAnyDialogActive = false; // 🔥 FLAGA NA KOŃCU
    }

    // 🔥 NOWA PUBLICZNA METODA: Bezpieczne wymuszenie zamknięcia dialogu
    /// <summary>
    /// Publiczna metoda do wymuszenia zamknięcia dialogu PRZED zmianą sceny.
    /// Zapewnia, że flaga IsAnyDialogActive zostanie poprawnie ustawiona na false.
    /// </summary>
    public void ForceEndDialog()
    {
        Debug.Log("[InkDialogController] 🔥 ForceEndDialog() called - wymuszam zamknięcie dialogu");
        
        // Jeśli dialog jest aktywny - zamknij go poprawnie
        if (IsDialogActive)
        {
            StopTypingCoroutine();
            CancelAutoAdvance();
            EndDialog();
            Debug.Log("[InkDialogController] ✅ Dialog zamknięty poprawnie");
        }
        else
        {
            // Nawet jeśli panel nie jest widoczny - wymuś resetowanie flagi
            IsAnyDialogActive = false;
            Debug.Log("[InkDialogController] ✅ Flaga IsAnyDialogActive = false");
        }
    }

    void DisablePlayerControls()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
            if (player.TryGetComponent<CharacterController>(out var controller)) controller.enabled = false;
            if (player.TryGetComponent<PlayerInteraction>(out var pi))
            {
                playerInteractionRef = pi;
                playerInteractionRef.enabled = false;
            }
        }

        prevCursorVisible = Cursor.visible;
        prevCursorLockState = Cursor.lockState;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void EnablePlayerControls()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = true;
            if (player.TryGetComponent<CharacterController>(out var controller)) controller.enabled = true;
            if (playerInteractionRef != null)
            {
                playerInteractionRef.enabled = true;
                playerInteractionRef = null;
            }
        }
    }


    public void SetInkVariable(string varName, object value)
    {
        if (story != null)
        {
            if (story.variablesState.GlobalVariableExistsWithName(varName))
            {
                story.variablesState[varName] = value;
            }
            else
            {
                Debug.LogWarning($"[InkDialogController] Tried to set '{varName}' but it is not declared in the Ink story for dialog '{dialogId}', skipping assignment.");
                // Keep it as pending so if another story with that variable is started later it can be applied
                pendingVariables[varName] = value;
            }
        }
        else
        {
            pendingVariables[varName] = value;
        }

        // Also persist the change so it becomes remembered across sessions
        SaveVariableToDict(varName, value);
        SaveSavedVariables();
    }

    // 🔥 NOWA METODA: Bezpieczna zmiana ID i opcjonalny reset (tylko pierwszy raz na starcie)
    public void SetupNPC(string newId, bool forceReset)
    {
        // 🔥 WAŻNE: Przed zmianą ID — zapisz zmienne ze starego ID do PlayerPrefs
        if (!string.IsNullOrEmpty(dialogId) && dialogId != newId)
        {
            // Zapisz zmienne z PAMIĘCI (savedVariables) nie ze story (bo story może być null)
            SaveSavedVariables();
            Debug.Log($"[InkDialogController] Zapisano zmienne dla starego ID: {dialogId}");
        }
        
        dialogId = newId;
        
        // Załaduj zmienne dla nowego ID
        LoadSavedVariables();
        LoadGlobalVariables();
        
        Debug.Log($"[InkDialogController] Zmieniono ID na: {newId}, załadowano zmienne z PlayerPrefs");
        
        // Reset tylko raz na starcie gry dla każdego NPC - nie przy każdej interakcji!
        if (forceReset && !_resetNPCsThisSession.Contains(newId))
        {
            string keyToClear = $"InkVariables_{newId}_v1";
            PlayerPrefs.DeleteKey(keyToClear);
            savedVariables.Clear();
            pendingVariables.Clear();
            _resetNPCsThisSession.Add(newId); // Oznacz że już resetowaliśmy tego NPC
            Debug.Log($"[InkDialogController] Reset danych dla: {keyToClear} (pierwszy raz w sesji)");
        }
        else if (forceReset && _resetNPCsThisSession.Contains(newId))
        {
            Debug.Log($"[InkDialogController] Pominięto reset dla: {newId} (już resetowano w tej sesji)");
        }
    }

    public T GetInkVariable<T>(string varName)
    {
        // Jeśli aktualnie dialog jest otwarty — odczytuj bezpośrednio ze story
        if (story != null && story.variablesState.GlobalVariableExistsWithName(varName))
        {
            return (T)story.variablesState[varName];
        }
    
        // Jeśli dialog nie jest otwarty — bierz z zapisanych zmiennych (PlayerPrefs)
        if (savedVariables.TryGetValue(varName, out var entry))
        {
            object parsed = ParseSavedValue(entry.type, entry.value);
            return (T)parsed;
        }
    
        Debug.LogWarning($"[InkDialogController] Requested variable '{varName}' does not exist.");
        return default;
    }

    void OnDisable()
    {
        StopTypingCoroutine();
        CancelAutoAdvance();
        EnablePlayerControls();
    }
}