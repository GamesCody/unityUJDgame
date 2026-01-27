using UnityEngine;

/// <summary>
/// Skrypt do edytowania zmiennych dialogu Profesora (ID 1234) w inspektorze
/// Pozwala na zmianę wartości zmiennych bezpośrednio w Unity bez uruchamiania gry
/// </summary>
public class ProfesorVariablesInspector : MonoBehaviour
{
    [Header("Zmienne dialogu Profesora (ID: 1234)")]
    [SerializeField]
    [Tooltip("Egzamin dostępny - jeśli true, gracz może zdawać egzamin")]
    private bool poprawkaZGrafow = false;

    [SerializeField]
    [Tooltip("Boss fight aktywny - jeśli true, przechodzi do boss fight'u")]
    private bool walkaBoss1 = false;

    [SerializeField]
    [TextArea(2, 4)]
    private string debugInfo = "Kliknij 'Load From PlayerPrefs' aby odczytać aktualne wartości";

    private void OnValidate()
    {
        // OnValidate jest wywoływana gdy zmienisz wartość w inspektorze
        // Aktualizujemy PlayerPrefs na podstawie wartości z inspektora
        if (Application.isPlaying)
        {
            UpdatePlayerPrefs();
        }
    }

    public void UpdatePlayerPrefs()
    {
        InkVariableTransfer.Q2 = poprawkaZGrafow;
        InkVariableTransfer.Q3 = walkaBoss1;
        
        debugInfo = $"✅ Zmienne zaktualizowane:\n" +
                    $"- poprawkaZGrafow: {poprawkaZGrafow}\n" +
                    $"- walkaBoss1: {walkaBoss1}";
        
        Debug.Log($"[ProfesorVariablesInspector] Zmienne Profesora zaktualizowane!");
    }

    public void LoadFromPlayerPrefs()
    {
        poprawkaZGrafow = InkVariableTransfer.Q2;
        walkaBoss1 = InkVariableTransfer.Q3;
        
        debugInfo = $"✅ Zmienne załadowane z PlayerPrefs:\n" +
                    $"- poprawkaZGrafow: {poprawkaZGrafow}\n" +
                    $"- walkaBoss1: {walkaBoss1}";
        
        Debug.Log($"[ProfesorVariablesInspector] Zmienne załadowane!");
    }

    public void ResetAllVariables()
    {
        poprawkaZGrafow = false;
        walkaBoss1 = false;
        
        InkVariableTransfer.Q2 = false;
        InkVariableTransfer.Q3 = false;
        
        debugInfo = "✅ Wszystkie zmienne zresetowane!";
        
        Debug.Log($"[ProfesorVariablesInspector] Zmienne zresetowane!");
    }

    // ========================================
    // PROPERTIES DLA ŁATWEGO DOSTĘPU
    // ========================================

    public bool PoprawkaZGrafow
    {
        get { return poprawkaZGrafow; }
        set { poprawkaZGrafow = value; UpdatePlayerPrefs(); }
    }

    public bool WalkaBoss1
    {
        get { return walkaBoss1; }
        set { walkaBoss1 = value; UpdatePlayerPrefs(); }
    }
}
