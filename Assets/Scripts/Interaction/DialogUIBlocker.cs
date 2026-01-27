using UnityEngine;

/// <summary>
/// Prosty skrypt który wyłącza interactionUI gdy dialog jest aktywny
/// Przypisać na GameObject z interactionUI
/// </summary>
public class DialogUIBlocker : MonoBehaviour
{
    private GameObject uiObject;

    void Start()
    {
        // Jeśli ten skrypt jest na interactionUI, użyj gameObject
        uiObject = gameObject;
    }

    void Update()
    {
        // Jeśli dialog aktywny = UI OFF
        if (InkDialogController.IsAnyDialogActive)
        {
            uiObject.SetActive(false);
        }
        // Jeśli dialog nieaktywny = UI może być widoczny (zależy od raycastu)
        // (PlayerInteraction kontroluje to)
    }
}
