using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class PlayerInteraction : MonoBehaviour
{
public Camera mainCam;
public float interactionDistance = 2f;
public GameObject interactionUI;
public TextMeshProUGUI interactionText;

private InkDialogController dialogController;

void Start()
{
    dialogController = FindObjectOfType<InkDialogController>();
}

void OnEnable()
{
    InkDialogController.OnDialogStarted += HideInteractionUI;
    InkDialogController.OnDialogEnded += ShowInteractionUI;
}

void OnDisable()
{
    InkDialogController.OnDialogStarted -= HideInteractionUI;
    InkDialogController.OnDialogEnded -= ShowInteractionUI;
}

void HideInteractionUI()
{
    interactionUI.SetActive(false);
}

void ShowInteractionUI()
{
    interactionUI.SetActive(false); // pokaże się dopiero po raycaście
}

// Unity Message | 0 references
private void Update()
{
    // 🔥 PIERWSZA LINIA - GLOBALNA BLOKADA
    if (InkDialogController.IsAnyDialogActive)
    {
        interactionUI.SetActive(false); // 🔥 GAŚ UI CO KLATKĘ
        return;                         // 🔥 PRZERWIJ UPDATE
    }

    InteractionRay();
}

// 1 reference
void InteractionRay()
{
    Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);
    RaycastHit hit;

    bool hitSomething = false;

    if (Physics.Raycast(ray, out hit, interactionDistance))
    {
        // Sprawdź czy to Interactable (NPC, drzwi itd)
        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable != null)
        {
            hitSomething = true;
            interactionText.text = interactable.GetDescription();

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                interactable.Interact();
            }
        }
        
        // Sprawdź czy to Pickable (przedmioty do zbierania)
        IPickable pickable = hit.collider.GetComponent<IPickable>();
        if (pickable != null)
        {
            // 🔥 Sprawdź czy Pickable component jest enabled
            Pickable pickableComponent = hit.collider.GetComponent<Pickable>();
            if (pickableComponent != null && pickableComponent.enabled)
            {
                hitSomething = true;
                interactionText.text = pickable.GetPickDescription();

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    pickable.Pick();
                }
            }
        }
    }

    interactionUI.SetActive(hitSomething);
}
}