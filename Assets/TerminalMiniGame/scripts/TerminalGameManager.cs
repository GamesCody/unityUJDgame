using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager do minigry terminalowej
/// Śledzi czy wszystkie komendy są prawidłowe
/// Zmienia scenę na "game" gdy wszystkie odpowiedzi są poprawne
/// </summary>
public class TerminalGameManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Game";
    [SerializeField] private string breakableDoorID = ""; // ID drzwi które mają zostać włamane
    private CommandChecker[] allCommandCheckers;
    
    void Start()
    {
        // Pokaż kursor i odblokuj go dla minigry terminalowej
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Szukaj wszystkich CommandChecker w scenie
        allCommandCheckers = FindObjectsOfType<CommandChecker>();
        Debug.Log($"[TerminalGameManager] Znaleziono {allCommandCheckers.Length} CommandChecker komponentów");
    }

    void Update()
    {
        // Co klatkę sprawdzaj czy wszystkie są prawidłowe
        CheckAllAnswers();
    }

    private void CheckAllAnswers()
    {
        if (allCommandCheckers.Length == 0)
            return;

        // Sprawdź czy wszystkie CommandChecker mają prawidłową odpowiedź
        bool allCorrect = true;
        
        foreach (var checker in allCommandCheckers)
        {
            if (!checker.IsCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        // Jeśli wszystkie prawidłowe - zmień scenę
        if (allCorrect)
        {
            LoadGameScene();
        }
    }

    private void LoadGameScene()
    {
        Debug.Log($"[TerminalGameManager] Wszystkie odpowiedzi prawidłowe! Ładuję scenę: {nextSceneName}");
        
        // Jeśli mamy przypisane ID drzwi do włamania - zapisz stan w GameStateManager
        if (!string.IsNullOrEmpty(breakableDoorID) && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetBreakableDoorState(breakableDoorID, true);
            Debug.Log($"[TerminalGameManager] Zapisano stan włamanych drzwi: {breakableDoorID} = true");
        }
        
        // Ukryj i zablokuj kursor przed przejściem do głównej gry
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(nextSceneName);
    }
}
