using UnityEngine;

/// <summary>
/// Skrypt do monitorowania zmiennych questa Maćka
/// Umieszcź go na obiekcie Maćka
/// </summary>
public class MaciekVariable : MonoBehaviour
{
    [Header("Zmienne Maćka (ID: 123)")]
    [SerializeField]
    [Tooltip("Quest zakończony - VAR koniecQuestMacka")]
    public bool koniecQuestMacka = false;

    private bool lastKoniecQuestMacka = false;
    private bool profesorAlreadyNotified = false; // 🔥 Flaga aby ustawić zmienną tylko raz
    private InkDialogController dialogController;

    void Start()
    {
        // Znajdź InkDialogController w scenie
        dialogController = FindObjectOfType<InkDialogController>();
        if (dialogController == null)
        {
            Debug.LogError("[MaciekVariable] Nie znaleziono InkDialogController w scenie!");
        }
    }

    void Update()
    {
        if (dialogController == null) return;

        // Odczytaj zmienną z Ink dialogu
        bool currentValue = InkVariableTransfer.Q1; // Q1 = koniecQuestMacka z Maćka

        // Jeśli wartość się zmieniła, zaktualizuj publiczny bool
        if (currentValue != lastKoniecQuestMacka)
        {
            koniecQuestMacka = currentValue;
            lastKoniecQuestMacka = currentValue;

            if (currentValue && !profesorAlreadyNotified) // 🔥 Ustaw tylko raz!
            {
                Debug.Log("[MaciekVariable] ✅ Quest Maćka zakończony! koniecQuestMacka = true");
                
                // 🔥 USTAW ZMIENNĄ DLA PROFESORA - ta zmienna jest w story Profesora, nie Maćka
                // Dlatego zapisujemy bezpośrednio do PlayerPrefs dla ID Profesora
                InkVariableTransfer.WriteVariableBool(
                    QuestVariables.PROFESOR_NPC_ID, 
                    QuestVariables.PROFESOR_EXAM_UNLOCKED, 
                    true);
                Debug.Log($"[MaciekVariable] ✅ Zapisano dla Profesora: {QuestVariables.PROFESOR_EXAM_UNLOCKED} = true");
                
                profesorAlreadyNotified = true; // 🔥 Oznacz że już powiadomiliśmy profesora
            }
            else if (!currentValue)
            {
                Debug.Log("[MaciekVariable] ❌ Quest Maćka resetowany. koniecQuestMacka = false");
                profesorAlreadyNotified = false; // 🔥 Reset flagi gdy quest się resetuje
            }
        }
    }

    /// <summary>
    /// Możesz też ręcznie ustawić ten bool (przydatne do testów)
    /// </summary>
    public void SetQuestComplete(bool value)
    {
        koniecQuestMacka = value;
        lastKoniecQuestMacka = value;
        InkVariableTransfer.WriteVariableBool(QuestVariables.MACIEK_NPC_ID, QuestVariables.MACIEK_QUEST_COMPLETE, value);
        Debug.Log($"[MaciekVariable] Ręcznie ustawiono koniecQuestMacka = {value}");
    }
}
