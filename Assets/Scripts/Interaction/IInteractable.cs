using UnityEngine;

public interface IInteractable
{
    void Interact();
    string GetDescription();
    bool CanInteract(); // 🔥 NOWE
}
