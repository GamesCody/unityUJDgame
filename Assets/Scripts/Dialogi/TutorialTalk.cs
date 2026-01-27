using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class TutorialTalk : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;          // dziecko
    public TextMeshProUGUI textBox;   // tekst w panelu

    [Header("Audio Manager")]
    public DialogAudioManager audioManager;  // Odniesienie do AudioManagera

    [Header("Dialog State")]
    public bool DialogStart = false;  // trigger to ustawia
    public int currentID = -1;        // trigger przypisuje ID
    public float textSpeed = 0.03f;
    [Tooltip("If >0, automatically advance to next line after this many seconds when a line finishes displaying.")]
    public float autoAdvanceDelay = 1.5f;

    private bool isTyping = false;
    private int index;
    private string[] activeLines;
    private Coroutine autoAdvanceRoutine = null;
    
    [Header("Audio dla linii")]
    [Tooltip("AudioSource dla każdej linii dialogu - musi być w tej samej kolejności co dialogi")]
    public AudioClip[] dialog0Audio = new AudioClip[] { };
    public AudioClip[] dialog1Audio = new AudioClip[] { };
    public AudioClip[] dialog2Audio = new AudioClip[] { };
    public AudioClip[] dialog3Audio = new AudioClip[] { };
    
    private AudioClip[] activeAudioClips;

    [Header("Wszystkie dialogi")]
    [TextArea(2, 8)]
    public string[] dialog0 = new string[]
    {
        "Witaj, wędrowcze. Cieszę się, że dotarłeś aż tutaj.",
        "Na początek: ruszaj się klawiszami WASD, a kamerą myszką.",
        "Pamiętaj, aby zbierać przedmioty — mogą uratować ci skórę.",
        "Gdy będziesz gotów, idź do karczmy i porozmawiaj z mieszkańcami."
    };
    [TextArea(2, 8)]
    public string[] dialog1 = new string[]
    {
        "O, nowa twarz! Witamy w naszej wiosce.",
        "Nazywam się Lidia. Jeśli potrzebujesz czegoś, szukaj mnie przy studni.",
        "Słyszałam, że w lesie pojawiły się dziwne światełka — uważaj po zmroku."
    };
    [TextArea(2, 8)]
    public string[] dialog2 = new string[]
    {
        "Słuchaj uważnie: ktoś ukradł mi narzędzia i poszedł w stronę bagien.",
        "Jeśli je odzyskasz, dam ci nagrodę — trochę ziół i domek na noc.",
        "Ostrożnie z bagiennymi bestiami — chodź cicho i miej latarnię pod ręką."
    };
    [TextArea(2, 8)]
    public string[] dialog3 = new string[]
    {
        "Cisza... czy ty to słyszysz? Ktoś — albo coś — obserwuje nas.",
        "Te ruiny kryją stare tajemnice. Nie wszystko, co błyszczy, jest skarbem.",
        "Jeżeli poczujesz zimny powiew, odwróć się i uciekaj."
    };

    void Start()
    {
        if (audioManager == null)
        {
            audioManager = GetComponent<DialogAudioManager>();
        }

        if (audioManager == null)
        {
            audioManager = transform.Find("SoundTalker")?.GetComponent<DialogAudioManager>();
        }

        if (audioManager == null)
        {
            Debug.LogWarning("[TutorialTalk] Nie znaleziono DialogAudioManager!");
        }

        panel.SetActive(false);
    }

    void Update()
    {
        if (!DialogStart)
            return;


        
        // Gdy dialog się zaczyna
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            PrepareDialog();
        }

        // Sterowanie klawiszem
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (audioManager != null)
            {
                audioManager.StopCurrentAudio();
            }

            if (isTyping)
            {
                // Skróć dialog - pokaż całą linię
                StopAllCoroutines();
                textBox.text = activeLines[index];
                isTyping = false;
                
                // Upewnij się, że dźwięk jest wyłączony po skróceniu linii
                if (audioManager != null)
                {
                    audioManager.StopCurrentAudio();
                }
                
                // start auto-advance if configured
                if (autoAdvanceDelay > 0f)
                {
                    if (autoAdvanceRoutine != null) StopCoroutine(autoAdvanceRoutine);
                    autoAdvanceRoutine = StartCoroutine(AutoAdvanceAfterDelay());
                }
            }
            else
            {
                // manual advance cancels any pending auto-advance
                if (autoAdvanceRoutine != null)
                {
                    StopCoroutine(autoAdvanceRoutine);
                    autoAdvanceRoutine = null;
                }
                NextLine();
            }
        }
    }

    void PrepareDialog()
    {
        activeLines = GetDialogByID(currentID);
        activeAudioClips = GetAudioByID(currentID);
        index = 0;

        if (activeLines == null || activeLines.Length == 0)
        {
            Debug.LogWarning("Brak dialogu dla ID: " + currentID + " (null or empty)");
            EndDialog();
            return;
        }

        textBox.text = "";
        StartCoroutine(TypeLine());
    }

    string[] GetDialogByID(int id)
    {
        switch (id)
        {
            case 0: return GetDialogOrDefault(dialog0, 0);
            case 1: return GetDialogOrDefault(dialog1, 1);
            case 2: return GetDialogOrDefault(dialog2, 2);
            case 3: return GetDialogOrDefault(dialog3, 3);
        }

        return null;
    }

    AudioClip[] GetAudioByID(int id)
    {
        switch (id)
        {
            case 0: return dialog0Audio ?? new AudioClip[] { };
            case 1: return dialog1Audio ?? new AudioClip[] { };
            case 2: return dialog2Audio ?? new AudioClip[] { };
            case 3: return dialog3Audio ?? new AudioClip[] { };
        }
        return new AudioClip[] { };
    }

    // Zwraca audio clip dla podanego indeksu; jeśli tablica jest krótsza,
    // zwraca ostatni dostępny clip jako fallback. Zwraca null jeśli brak clipów.
    AudioClip GetAudioClipForIndex(int idx)
    {
        if (activeAudioClips == null || activeAudioClips.Length == 0)
            return null;

        if (idx >= 0 && idx < activeAudioClips.Length)
            return activeAudioClips[idx];

        // Fallback: użyj ostatniego clipu jeśli indeks poza zakresem
        return activeAudioClips[activeAudioClips.Length - 1];
    }

    // If the inspector-provided dialog is null/empty, return a built-in default for that ID
    string[] GetDialogOrDefault(string[] dialog, int id)
    {
        if (dialog != null && dialog.Length > 0)
            return dialog;

        Debug.LogWarning($"[TutorialTalk] Dialog {id} is empty in Inspector — using default fallback.");

        switch (id)
        {
            case 0:
                return new string[] {
                    "Witaj, wędrowcze. Cieszę się, że dotarłeś aż tutaj.",
                    "Na początek: ruszaj się klawiszami WASD, a kamerą myszką.",
                    "Pamiętaj, aby zbierać przedmioty — mogą uratować ci skórę.",
                    "Gdy będziesz gotów, idź do karczmy i porozmawiaj z mieszkańcami."
                };
            case 1:
                return new string[] {
                    "O, nowa twarz! Witamy w naszej wiosce.",
                    "Nazywam się Lidia. Jeśli potrzebujesz czegoś, szukaj mnie przy studni.",
                    "Słyszałam, że w lesie pojawiły się dziwne światełka — uważaj po zmroku."
                };
            case 2:
                return new string[] {
                    "Słuchaj uważnie: ktoś ukradł mi narzędzia i poszedł w stronę bagien.",
                    "Jeśli je odzyskasz, dam ci nagrodę — trochę ziół i domek na noc.",
                    "Ostrożnie z bagiennymi bestiami — chodź cicho i miej latarnię pod ręką."
                };
            case 3:
                return new string[] {
                    "Cisza... czy ty to słyszysz? Ktoś — albo coś — obserwuje nas.",
                    "Te ruiny kryją stare tajemnice. Nie wszystko, co błyszczy, jest skarbem.",
                    "Jeżeli poczujesz zimny powiew, odwróć się i uciekaj."
                };
        }

        return null;
    }

   IEnumerator TypeLine()
{
    // zabezpieczenia
    if (activeLines == null || activeLines.Length == 0 || index < 0 || index >= activeLines.Length)
    {
        isTyping = false;
        yield break;
    }

    isTyping = true;

    // *** TU: start audio dla nowej linii (użyj fallbacku jeśli tablica krótsza) ***
    AudioClip clipForLine = GetAudioClipForIndex(index);
    if (audioManager != null && clipForLine != null)
    {
        float duration = activeLines[index].Length * textSpeed;
        audioManager.PlayAudioClip(clipForLine, duration);
    }

    textBox.text = "";
    string currentLine = activeLines[index];

    for (int i = 0; i < currentLine.Length; i++)
    {
        textBox.text += currentLine[i];
        yield return new WaitForSeconds(textSpeed);
    }

    // *** NIE zatrzymuj tu audio! ***
    // audioManager.StopCurrentAudio();  <-- To usuwamy

    isTyping = false;

    // auto-advance
    if (autoAdvanceDelay > 0f)
    {
        if (autoAdvanceRoutine != null)
            StopCoroutine(autoAdvanceRoutine);

        autoAdvanceRoutine = StartCoroutine(AutoAdvanceAfterDelay());
    }
}

    void NextLine()
    {
        if (index < activeLines.Length - 1)
        {
            index++;
            textBox.text = "";
            // cancel any pending auto-advance when advancing
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialog();
        }
    }

    void EndDialog()
    {
        textBox.text = "";
        panel.SetActive(false);

        if (audioManager != null)
        {
            audioManager.StopCurrentAudio();
        }

        DialogStart = false;
        currentID = -1;
        // cancel any pending auto-advance
        if (autoAdvanceRoutine != null)
        {
            StopCoroutine(autoAdvanceRoutine);
            autoAdvanceRoutine = null;
        }
    }

    System.Collections.IEnumerator AutoAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        // only advance if still active and not typing
        if (!isTyping && DialogStart && activeLines != null && index < activeLines.Length)
        {
            NextLine();
        }
        autoAdvanceRoutine = null;
    }
}
