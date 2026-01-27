using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper skrypt do transferu zmiennych między różnymi NPC (dialogami)
/// Uprościnia transfer zmiennych z MaciekQ1 → ProfesorQ1 i podobne operacje
/// </summary>
public class InkVariableTransfer : MonoBehaviour
{
    [System.Serializable]
    private class InkVariableEntry
    {
        public string name;
        public string type;
        public string value;
    }

    [System.Serializable]
    private class InkVariableCollection
    {
        public List<InkVariableEntry> variables = new List<InkVariableEntry>();
    }

    // ========================================
    // PUBLICZNE ZMIENNE DLA INSPEKTORA
    // ========================================

    [Header("Zmienne Profesora (ID: 1234)")]
    [SerializeField]
    [Tooltip("Egzamin dostępny - VAR poprawkaZGrafow")]
    public bool poprawkaZGrafow = false;

    [SerializeField]
    [Tooltip("Boss fight aktywny - VAR walkaBoss1")]
    public bool walkaBoss1 = false;

    private bool lastPoprawkaZGrafow = false;
    private bool lastWalkaBoss1 = false;

    // 🔥 Flaga do śledzenia czy już resetowaliśmy zmienne w tej sesji gry
    private static bool _variablesResetThisSession = false;
    
    // 🔥 Licznik framów do pominięcia automatycznego triggera
    private static int _skipAutoTriggerFrameCount = 0;
    private const int SKIP_FRAMES = 3; // Pomiń pierwsze 3 framy po resecie
    
    // 🔥 Flaga aby załadować BattleScene tylko raz
    private static bool _battleSceneLoadedThisSession = false;

    void Awake()
    {
        // 🔥 RESET NA STARCIE GRY: Resetuj wszystkie flagi
        if (!_variablesResetThisSession)
        {
            _battleSceneLoadedThisSession = false; // Reset flagi tylko raz na starcie gry
        }
    }

    void Start()
    {
        // 🔥 RESET NA STARCIE SCENY: NIE resetuj flagi BattleScene tutaj!
        // Flaga _battleSceneLoadedThisSession resetuje się tylko w Awake() na starcie gry
        // Po powrocie z BattleScene flaga pozostaje true, więc scena się nie załaduje ponownie
        
        // 🔥 RESET na starcie gry: wymaż zmienne Profesora (walkaBoss1, poprawkaZGrafow)
        // Robimy to w Start(), nie w Awake(), aby być pewnym że wszystkie inne skrypty już się zainiicjalizowały
        if (!_variablesResetThisSession)
        {
            string profesorKey = $"InkVariables_{QuestVariables.PROFESOR_NPC_ID}_v1";
            string maciekKey = $"InkVariables_{QuestVariables.MACIEK_NPC_ID}_v1";
            
            PlayerPrefs.DeleteKey(profesorKey);
            PlayerPrefs.DeleteKey(maciekKey); // 🔥 Resetuj też zmienne Maćka
            PlayerPrefs.Save();
            
            _variablesResetThisSession = true;
            _skipAutoTriggerFrameCount = SKIP_FRAMES; // 🔥 Pomiń następne 3 framy
            Debug.Log($"[InkVariableTransfer] 🔥 Reset na starcie gry: wyczyszczono zmienne Profesora i Maćka");
        }
    }

    void Update()
    {
        // 🔥 Jeśli jesteśmy w okresie pomijającym trigger, zmniejsz licznik
        if (_skipAutoTriggerFrameCount > 0)
        {
            // 🔥 Jeśli inny skrypt ustawił zmienne mimo resetu, usuń je ponownie!
            string profesorKey = $"InkVariables_{QuestVariables.PROFESOR_NPC_ID}_v1";
            if (PlayerPrefs.HasKey(profesorKey))
            {
                PlayerPrefs.DeleteKey(profesorKey);
                PlayerPrefs.Save();
                Debug.LogWarning($"[InkVariableTransfer] ⚠️ Inny skrypt ustawił zmienne! Usuwam ponownie...");
            }
            
            _skipAutoTriggerFrameCount--;
            Debug.Log($"[InkVariableTransfer] ⏭️ Pomijam automatyczne triggery ({_skipAutoTriggerFrameCount} framów pozostało)");
            return;
        }

        // 🔥 SYNCHRONIZACJA Z GÓRY NA DÓŁ (Z Ink/PlayerPrefs do Inspektora)
        // Pobieramy wartości z Q2/Q3 i odświeżamy checkboxy w Inspektorze
        bool newWalkaBoss1 = Q3; // 🔥 Pobierz nową wartość
        bool newPoprawkaZGrafow = Q2;
        
        // 🔥 ZMIANA SCENY: Jeśli walkaBoss1 zmienił się na true, załaduj BattleScene tylko raz
        // SPRAWDŹ WARUNEK PRZED przypisaniem lastWalkaBoss1!
        if (newWalkaBoss1 && !lastWalkaBoss1 && !_battleSceneLoadedThisSession)
        {
            Debug.Log("[InkVariableTransfer] 🔥 Boss fight aktywowany! Ładuję scenę: BattleScene");
            _battleSceneLoadedThisSession = true; // 🔥 Oznacz że już ładujemy scenę
            
            // 🔥 FIX #2: Wymuszenie pełnego zamknięcia dialogu PRZED zmianą sceny
            InkDialogController dialogController = FindObjectOfType<InkDialogController>();
            if (dialogController != null)
            {
                Debug.Log("[InkVariableTransfer] 🔥 Wymuszam pełne zamknięcie dialogu za pomocą ForceEndDialog()...");
                dialogController.ForceEndDialog(); // ✅ Ta metoda resetuje flagi i odblokuje gracza
            }
            
            // Odblokuj graczowi kontrolę (backup na wypadek gdy InkDialogController nie istnieje)
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                if (player.TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = true;
                if (player.TryGetComponent<CharacterController>(out var controller)) controller.enabled = true;
                if (player.TryGetComponent<PlayerInteraction>(out var pi)) pi.enabled = true;
            }
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            // 🔥 Załaduj scenę z opóźnieniem aby uniknąć problemów TextMesh Pro
            StartCoroutine(LoadBattleSceneWithDelay());
        }

        // 🔥 Teraz przypisz wartości
        poprawkaZGrafow = newPoprawkaZGrafow;
        walkaBoss1 = newWalkaBoss1;

        // Zapamiętujemy stan, żeby OnValidate nie zapętlił zapisu
        lastPoprawkaZGrafow = newPoprawkaZGrafow;
        lastWalkaBoss1 = newWalkaBoss1;

        // 🔥 AUTOMATYCZNE USTAWIENIE: Gdy koniecQuestMacka == true, zawsze ustaw poprawkaZGrafow = true
        if (Q1 && !Q2)
        {
            Q2 = true;
            Debug.Log("[InkVariableTransfer] 🔥 Auto-trigger: koniecQuestMacka=true → poprawkaZGrafow=true");
        }
    }

    private void OnValidate()
    {
        // Synchronizuj zmienne z inspektora do PlayerPrefs TYLKO jeśli się zmieniły
        if (Application.isPlaying)
        {
            // SYNCHRONIZACJA Z DOŁU NA GÓRĘ (Z Inspektora do Ink)
            if (poprawkaZGrafow != lastPoprawkaZGrafow)
            {
                Q2 = poprawkaZGrafow;
                lastPoprawkaZGrafow = poprawkaZGrafow;
            }

            if (walkaBoss1 != lastWalkaBoss1)
            {
                Q3 = walkaBoss1;
                lastWalkaBoss1 = walkaBoss1;
            }
        }
    }

    // ========================================
    // QUEST STATUS PROPERTIES
    // ========================================

    /// <summary>
    /// Q1 - Status questa Maćka
    /// true = Maciek wykonał quest (koniecQuestMacka = true)
    /// false = Quest nie wykonany
    /// </summary>
    public static bool Q1
    {
        get { return ReadVariableBool(QuestVariables.MACIEK_NPC_ID, QuestVariables.MACIEK_QUEST_COMPLETE); }
    }

    /// <summary>
    /// Q2 - Status egzaminu u Profesora (poprawkaZGrafow)
    /// true = Egzamin dostępny
    /// false = Egzamin niedostępny
    /// Można odczytywać i ustawiać!
    /// </summary>
    public static bool Q2
    {
        get { return ReadVariableBool(QuestVariables.PROFESOR_NPC_ID, QuestVariables.PROFESOR_EXAM_UNLOCKED); }
        set { WriteVariableBool(QuestVariables.PROFESOR_NPC_ID, QuestVariables.PROFESOR_EXAM_UNLOCKED, value); }
    }

    /// <summary>
    /// Q3 - Status boss fight'u (walkaBoss1)
    /// true = Boss fight aktywowany
    /// false = Boss fight nieaktywny
    /// </summary>
    public static bool Q3
    {
        get { return ReadVariableBool(QuestVariables.PROFESOR_NPC_ID, QuestVariables.PROFESOR_BOSS_FIGHT_TRIGGERED); }
        set { WriteVariableBool(QuestVariables.PROFESOR_NPC_ID, QuestVariables.PROFESOR_BOSS_FIGHT_TRIGGERED, value); }
    }

    // ========================================
    // CZYTANIE ZMIENNEJ Z NPC
    // ========================================
    
    /// <summary>
    /// Odczytuje zmienną bool z PlayerPrefs dla danego NPC ID
    /// </summary>
    public static bool ReadVariableBool(string npcId, string varName)
    {
        string value = ReadVariableString(npcId, varName);
        if (string.IsNullOrEmpty(value)) return false;
        return value.ToLower() == "true" || value == "1";
    }

    /// <summary>
    /// Odczytuje zmienną int z PlayerPrefs dla danego NPC ID
    /// </summary>
    public static int ReadVariableInt(string npcId, string varName)
    {
        string value = ReadVariableString(npcId, varName);
        if (string.IsNullOrEmpty(value)) return 0;
        if (int.TryParse(value, out int result)) return result;
        return 0;
    }

    /// <summary>
    /// Odczytuje zmienną float z PlayerPrefs dla danego NPC ID
    /// </summary>
    public static float ReadVariableFloat(string npcId, string varName)
    {
        string value = ReadVariableString(npcId, varName);
        if (string.IsNullOrEmpty(value)) return 0f;
        if (float.TryParse(value, out float result)) return result;
        return 0f;
    }

    /// <summary>
    /// Odczytuje zmienną string z PlayerPrefs dla danego NPC ID
    /// </summary>
    public static string ReadVariableString(string npcId, string varName)
    {
        string key = $"InkVariables_{npcId}_v1";
        
        Debug.Log($"[InkVariableTransfer] 🔍 Szukam zmiennej '{varName}' w kluczu: {key}");
        
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"[InkVariableTransfer] ❌ Brak zmiennych dla NPC ID: {npcId}. Klucz nie istnieje: {key}");
            return null;
        }

        string json = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var collection = JsonUtility.FromJson<InkVariableCollection>(json);
            if (collection != null && collection.variables != null)
            {
                Debug.Log($"[InkVariableTransfer] Znaleziono {collection.variables.Count} zmiennych dla {npcId}");
                foreach (var entry in collection.variables)
                {
                    if (entry.name == varName)
                    {
                        Debug.Log($"[InkVariableTransfer] ✅ Odczytano {varName}={entry.value} z NPC {npcId}");
                        return entry.value;
                    }
                }
                Debug.LogWarning($"[InkVariableTransfer] ⚠️ Zmienna '{varName}' nie znaleziona. Dostępne zmienne: {string.Join(", ", collection.variables.ConvertAll(v => v.name))}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InkVariableTransfer] Błąd odczytywania zmiennych: {ex.Message}");
        }

        Debug.LogWarning($"[InkVariableTransfer] Zmienna '{varName}' nie znaleziona dla NPC ID: {npcId}");
        return null;
    }

    // ========================================
    // PISANIE ZMIENNEJ DO NPC
    // ========================================

    /// <summary>
    /// Zapisuje zmienną bool do PlayerPrefs dla danego NPC ID
    /// </summary>
    public static void WriteVariableBool(string npcId, string varName, bool value)
    {
        WriteVariableString(npcId, varName, value ? "true" : "false", "bool");
    }

    /// <summary>
    /// Zapisuje zmienną int do PlayerPrefs dla danego NPC ID
    /// </summary>
    public static void WriteVariableInt(string npcId, string varName, int value)
    {
        WriteVariableString(npcId, varName, value.ToString(), "int");
    }

    /// <summary>
    /// Zapisuje zmienną float do PlayerPrefs dla danego NPC ID
    /// </summary>
    public static void WriteVariableFloat(string npcId, string varName, float value)
    {
        WriteVariableString(npcId, varName, value.ToString("R"), "float");
    }

    /// <summary>
    /// Zapisuje zmienną string do PlayerPrefs dla danego NPC ID
    /// </summary>
    public static void WriteVariableString(string npcId, string varName, string value, string type = "string")
    {
        string key = $"InkVariables_{npcId}_v1";
        string json = PlayerPrefs.GetString(key, "");
        InkVariableCollection collection = null;

        // Wczytaj istniejące zmienne
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                collection = JsonUtility.FromJson<InkVariableCollection>(json);
            }
            catch { }
        }

        if (collection == null)
            collection = new InkVariableCollection();

        // Usuń starą wartość
        collection.variables.RemoveAll(v => v.name == varName);

        // Dodaj nową
        collection.variables.Add(new InkVariableEntry
        {
            name = varName,
            type = type,
            value = value
        });

        // Zapisz
        json = JsonUtility.ToJson(collection);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        Debug.Log($"[InkVariableTransfer] ✅ Zapisano {varName}={value} ({type}) dla NPC {npcId}");
    }

    // ========================================
    // TRANSFER ZMIENNEJ Z JEDNEGO NPC NA DRUGI
    // ========================================

    /// <summary>
    /// Transferuje zmienną bool z jednego NPC na drugi
    /// Przykład: TransferVariableBool("123", "koniecQuestMacka", "1234", "poprawkaZGrafow")
    /// </summary>
    public static void TransferVariableBool(string sourceNpcId, string sourceVarName, 
                                             string targetNpcId, string targetVarName)
    {
        bool value = ReadVariableBool(sourceNpcId, sourceVarName);
        WriteVariableBool(targetNpcId, targetVarName, value);
        Debug.Log($"[InkVariableTransfer] 🔄 Transfer: {sourceNpcId}.{sourceVarName} ({value}) → {targetNpcId}.{targetVarName}");
    }

    /// <summary>
    /// Transferuje zmienną int z jednego NPC na drugi
    /// </summary>
    public static void TransferVariableInt(string sourceNpcId, string sourceVarName,
                                            string targetNpcId, string targetVarName)
    {
        int value = ReadVariableInt(sourceNpcId, sourceVarName);
        WriteVariableInt(targetNpcId, targetVarName, value);
        Debug.Log($"[InkVariableTransfer] 🔄 Transfer: {sourceNpcId}.{sourceVarName} ({value}) → {targetNpcId}.{targetVarName}");
    }

    /// <summary>
    /// Transferuje zmienną float z jednego NPC na drugi
    /// </summary>
    public static void TransferVariableFloat(string sourceNpcId, string sourceVarName,
                                              string targetNpcId, string targetVarName)
    {
        float value = ReadVariableFloat(sourceNpcId, sourceVarName);
        WriteVariableFloat(targetNpcId, targetVarName, value);
        Debug.Log($"[InkVariableTransfer] 🔄 Transfer: {sourceNpcId}.{sourceVarName} ({value}) → {targetNpcId}.{targetVarName}");
    }

    /// <summary>
    /// Transferuje zmienną string z jednego NPC na drugi
    /// </summary>
    public static void TransferVariableString(string sourceNpcId, string sourceVarName,
                                               string targetNpcId, string targetVarName, string type = "string")
    {
        string value = ReadVariableString(sourceNpcId, sourceVarName);
        if (value != null)
        {
            WriteVariableString(targetNpcId, targetVarName, value, type);
            Debug.Log($"[InkVariableTransfer] 🔄 Transfer: {sourceNpcId}.{sourceVarName} ({value}) → {targetNpcId}.{targetVarName}");
        }
    }

    // ========================================
    // GLOBALNE ZMIENNE
    // ========================================

    /// <summary>
    /// Zapisuje zmienną do globalnego scope (dostępna dla wszystkich NPC)
    /// </summary>
    public static void WriteGlobalVariableBool(string varName, bool value)
    {
        WriteGlobalVariableString(varName, value ? "true" : "false", "bool");
    }

    /// <summary>
    /// Odczytuje globalną zmienną bool
    /// </summary>
    public static bool ReadGlobalVariableBool(string varName)
    {
        string value = ReadGlobalVariableString(varName);
        if (string.IsNullOrEmpty(value)) return false;
        return value.ToLower() == "true" || value == "1";
    }

    /// <summary>
    /// Zapisuje zmienną do globalnego scope
    /// </summary>
    public static void WriteGlobalVariableString(string varName, string value, string type = "string")
    {
        string key = "InkVariables_global_v1";
        string json = PlayerPrefs.GetString(key, "");
        InkVariableCollection collection = null;

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                collection = JsonUtility.FromJson<InkVariableCollection>(json);
            }
            catch { }
        }

        if (collection == null)
            collection = new InkVariableCollection();

        collection.variables.RemoveAll(v => v.name == varName);
        collection.variables.Add(new InkVariableEntry
        {
            name = varName,
            type = type,
            value = value
        });

        json = JsonUtility.ToJson(collection);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        Debug.Log($"[InkVariableTransfer] ✅ Zapisano globalnie: {varName}={value} ({type})");
    }

    /// <summary>
    /// Odczytuje globalną zmienną string
    /// </summary>
    public static string ReadGlobalVariableString(string varName)
    {
        string key = "InkVariables_global_v1";
        if (!PlayerPrefs.HasKey(key)) return null;

        string json = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var collection = JsonUtility.FromJson<InkVariableCollection>(json);
            if (collection != null && collection.variables != null)
            {
                foreach (var entry in collection.variables)
                {
                    if (entry.name == varName)
                    {
                        Debug.Log($"[InkVariableTransfer] ✅ Odczytano globalnie {varName}={entry.value}");
                        return entry.value;
                    }
                }
            }
        }
        catch { }

        return null;
    }

    // ========================================
    // DEBUG - WYŚWIETL WSZYSTKIE ZMIENNE
    // ========================================

    /// <summary>
    /// Wyświetla wszystkie zmienne zapisane dla danego NPC ID
    /// </summary>
    public static void DebugPrintNpcVariables(string npcId)
    {
        string key = $"InkVariables_{npcId}_v1";
        
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log($"[InkVariableTransfer] Brak zmiennych dla NPC {npcId}");
            return;
        }

        string json = PlayerPrefs.GetString(key);
        Debug.Log($"[InkVariableTransfer] === Zmienne dla NPC {npcId} ===\n{json}");

        try
        {
            var collection = JsonUtility.FromJson<InkVariableCollection>(json);
            if (collection != null && collection.variables != null)
            {
                foreach (var entry in collection.variables)
                {
                    Debug.Log($"  - {entry.name} ({entry.type}): {entry.value}");
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Wyświetla wszystkie zmienne globalne
    /// </summary>
    public static void DebugPrintGlobalVariables()
    {
        string key = "InkVariables_global_v1";
        
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log($"[InkVariableTransfer] Brak zmiennych globalnych");
            return;
        }

        string json = PlayerPrefs.GetString(key);
        Debug.Log($"[InkVariableTransfer] === Zmienne globalne ===\n{json}");

        try
        {
            var collection = JsonUtility.FromJson<InkVariableCollection>(json);
            if (collection != null && collection.variables != null)
            {
                foreach (var entry in collection.variables)
                {
                    Debug.Log($"  - {entry.name} ({entry.type}): {entry.value}");
                }
            }
        }
        catch { }
    }

    // 🔥 COROUTINE do opóźnionego załadowania BattleScene
    /// <summary>
    /// Oczekuje na 0.5 sekundy aby uniknąć problemów TextMesh Pro podczas scene transition,
    /// potem załaduje BattleScene.
    /// </summary>
    private IEnumerator LoadBattleSceneWithDelay()
    {
        Debug.Log("[InkVariableTransfer] ⏳ Czekam 0.5 sekundy przed załadowaniem BattleScene...");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[InkVariableTransfer] 🎮 Ładuję BattleScene!");
        SceneManager.LoadScene("BattleScene");
    }
}
