using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable, IDoorState
{
    public string doorID = "Door_1";
    public Animator animator;

    public bool isOpen = false;

    public string GetDescription()
    {
        return isOpen ? "Zamknij drzwi (E)" : "Otwórz drzwi (E)";
    }

    public bool CanInteract()
    {
        return true; // Drzwi można zawsze otwierać/zamykać
    }
    public void SetDoorState(bool open, Quaternion rotation)
    {
        Debug.Log($"[DoorController] SetDoorState called for {doorID}: open={open}");
        isOpen = open;
        transform.rotation = rotation;
        if (animator != null)
        {
            animator.SetBool("IsOpen", isOpen);
        }
        else
        {
            Debug.LogWarning($"[DoorController] Animator is null on {doorID} in SetDoorState");
        }
        // Ustaw animator na odpowiedni klip/stanu
        if (isOpen)
        {
            animator.Play("Closed"); // Ustaw na stan zamkniętych drzwi, nawet jeśli są otwarte
        }
        else
        {
            animator.Play("DoorIdle"); // Ustaw na stan zamkniętych drzwi
        }
    }
    public void Interact()
    {
        isOpen = !isOpen;

        if (animator != null)
            animator.SetBool("IsOpen", isOpen);

        Debug.Log($"[DoorController] Interact() -> state changed: {isOpen}");

        // Zapis stanu
        if (PlayerPositionManager.Instance != null)
            PlayerPositionManager.Instance.SaveDoorState(doorID, isOpen, transform);
    }

    private void Start()
    {
        // Przywracanie stanu
        if (PlayerPositionManager.Instance != null)
            PlayerPositionManager.Instance.RestoreDoorState(doorID, this);

        if (animator != null)
            animator.SetBool("IsOpen", isOpen);
    }


}
