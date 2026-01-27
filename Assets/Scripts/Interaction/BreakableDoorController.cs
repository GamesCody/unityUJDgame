using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drzwi, które wymagają włamywania poprzez grę w minigrę
/// 1. E Włam się -> załaduj scenę minigry
/// 2. Po wygraniu minigry -> drzwi się otworzą automatycznie
/// </summary>
public class BreakableDoorController : MonoBehaviour, IInteractable, IDoorState
{
    [Header("Konfiguracja drzwi")]
    public string doorID = "BreakableDoor_1";
    public Animator animator;
    public bool isOpen = false;

    [Header("Minigra")]
    [SerializeField] private string minigameSceneName = "Linux";
    
    private bool isBreaking = false;
    private bool hasBeenBroken = false;

    public string GetDescription()
    {
        if (hasBeenBroken)
        {
            return isOpen ? "Zamknij drzwi (E)" : "Otwórz drzwi (E)";
        }
        else
        {
            return "E Włam się";
        }
    }

    public bool CanInteract()
    {
        return !isBreaking; // Można interaktować, chyba że jest w trakcie łamania
    }

    public void SetDoorState(bool open, Quaternion rotation)
    {
        Debug.Log($"[BreakableDoorController] SetDoorState called for {doorID}: open={open}");
        isOpen = open;
        transform.rotation = rotation;
        
        if (animator != null)
        {
            animator.SetBool("IsOpen", isOpen);
            
            if (isOpen)
            {
                animator.Play("Open");
            }
            else
            {
                animator.Play("Closed");
            }
        }
        else
        {
            Debug.LogWarning($"[BreakableDoorController] Animator is null on {doorID}");
        }
    }

    public void Interact()
    {
        // Guard: jeśli już w trakcie łamania - ignoruj
        if (isBreaking) return;
        isBreaking = true;

        if (!hasBeenBroken)
        {
            // ETAP 1: Włamywanie - załaduj minigrę
            Debug.Log($"[BreakableDoorController] Włamywanie drzwi: {doorID}");
            
            // Zapisz pozycję gracza przed zmianą sceny
            if (PlayerPositionManager.Instance != null)
            {
                PlayerPositionManager.Instance.SavePlayerPositionNow();
                Debug.Log("[BreakableDoorController] Zapisano pozycję gracza");
            }
            
            // Załaduj scenę minigry
            SceneManager.LoadScene(minigameSceneName);
        }
        else
        {
            // ETAP 2: Drzwi już włamane - otwórz/zamknij normalnie
            isOpen = !isOpen;

            if (animator != null)
                animator.SetBool("IsOpen", isOpen);

            Debug.Log($"[BreakableDoorController] Drzwi {doorID} -> state changed: {isOpen}");

            // Zapis stanu
            if (PlayerPositionManager.Instance != null)
                PlayerPositionManager.Instance.SaveDoorState(doorID, isOpen, transform);
            
            isBreaking = false;
        }
    }

    private void Start()
    {
        // Sprawdź czy drzwi były już włamane - szukaj w GameStateManager
        if (GameStateManager.Instance != null)
        {
            hasBeenBroken = GameStateManager.Instance.GetBreakableDoorState(doorID);
        }
        
        if (hasBeenBroken)
        {
            // Drzwi były włamane, otwórz je automatycznie
            isOpen = true;
            Debug.Log($"[BreakableDoorController] Drzwi {doorID} zostały włamane - otwieranie automatycznie");
        }

        // Przywracanie stanu drzwi z PlayerPositionManager
        if (PlayerPositionManager.Instance != null)
            PlayerPositionManager.Instance.RestoreDoorState(doorID, this);

        if (animator != null)
        {
            animator.SetBool("IsOpen", isOpen);
            if (isOpen)
            {
                animator.Play("Open");
            }
            else
            {
                animator.Play("Closed");
            }
        }
    }
}
