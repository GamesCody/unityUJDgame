using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Skrypt dla świecy - pozwala zbierać świece za pomocą klawisza E
/// Liczę zebane świece i aktualizuję zmienną w Ink
/// </summary>
public class CandleCollector : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private InkDialogController dialogController;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private string candleName = "Świeca";
    
    private bool isNearPlayer = false;
    private bool isCollected = false;
    private Transform player;

    void Start()
    {
        // Znaleźć dialog controller jeśli nie ma przypisanego
        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();
        
        // Znaleźć gracza (wyszukaj tag Player)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
        
        // RESETUJ ZMIENNE SCENY NA STARCIE
        // Czyść zmienne z poprzedniej sesji
        PlayerPrefs.DeleteKey("candleCount");
        PlayerPrefs.DeleteKey("candlesResetOnScene");
        
        // Resetuj zmienne w InkDialogController
        if (dialogController != null)
        {
            dialogController.SetInkVariable("liczba_swiec", 0);
            Debug.Log("[CandleCollector] 🔄 Resetowano: liczba_swiec = 0");
        }
        
        PlayerPrefs.Save();
        Debug.Log("[CandleCollector] 🔄 Resetowano PlayerPrefs na starcie sceny");
    }

    void Update()
    {
        if (isCollected || player == null) return;

        // Sprawdzić odległość do gracza
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= interactionRange)
        {
            // Gracz jest blisko
            isNearPlayer = true;
            
            // Sprawdzić naciśnięcie E
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                CollectCandle();
            }
        }
        else
        {
            isNearPlayer = false;
        }
    }

    private void CollectCandle()
    {
        isCollected = true;
        Debug.Log($"[CandleCollector] Zebrano: {candleName}");
        
        // Pobierz aktualną liczbę świec z PlayerPrefs (przechowywanie licznika)
        int currentCandleCount = PlayerPrefs.GetInt("candleCount", 0);
        
        // Zwiększ licznik
        currentCandleCount++;
        Debug.Log($"[CandleCollector] Liczba świec: {currentCandleCount}/3");
        
        // Zapisz w PlayerPrefs
        PlayerPrefs.SetInt("candleCount", currentCandleCount);
        PlayerPrefs.Save();
        
        // Ustaw zmienną w Ink
        if (dialogController != null)
        {
            dialogController.SetInkVariable("liczba_swiec", currentCandleCount);
            Debug.Log($"[CandleCollector] Ustawiono zmienną Ink: liczba_swiec = {currentCandleCount}");
        }
        
        // Zniszcz świecę
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Wizualna wskazówka że gracz jest w zasięgu
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
