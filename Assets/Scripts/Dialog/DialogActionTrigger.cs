using UnityEngine;
using System.Collections.Generic;

public class DialogActionTrigger : MonoBehaviour
{
    [System.Serializable]
    public class DialogAction
    {
        public string triggerLine;      // linia dialogu, która uruchomia akcję
        public GameObject targetCharacter;
        public CharacterAction[] actions;
    }

    [System.Serializable]
    public class CharacterAction
    {
        public enum ActionType { LookAt, MoveTo, Idle, ChangeCamera, Toggle, EnablePickable, DisableComponent, EnableComponent }
        public ActionType actionType;
        public Transform target;        // dla LookAt i MoveTo
        public Camera cameraToActivate; // dla ChangeCamera
        public GameObject toggleObject; // dla Toggle - urządzenie do włączenia/wyłączenia
        public GameObject pickableObject; // dla EnablePickable - przedmiot do zbierania
        public GameObject targetGameObject; // dla DisableComponent/EnableComponent - obiekt docelowy
        public string componentName; // nazwa komponentu do wyłączenia/włączenia (np. "MaciekVariable", "Animator")
        public float duration = 1f;     // czas trwania akcji
    }

    [SerializeField] private List<DialogAction> dialogActions = new List<DialogAction>();

    void OnEnable()
    {
        InkDialogController.dialogLineChanged += OnDialogLineChanged;
    }

    void OnDisable()
    {
        InkDialogController.dialogLineChanged -= OnDialogLineChanged;
    }

    void OnDialogLineChanged(string line, string speaker)
    {
        // Sprawdź czy któraś akcja powinna się uruchomić
        foreach (var action in dialogActions)
        {
            // Porównaj z trimowaniem białych znaków
            if (line.Trim().Contains(action.triggerLine.Trim()))
            {
                ExecuteAction(action);
            }
        }
    }

    void ExecuteAction(DialogAction action)
    {
        if (action.targetCharacter == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Target character is null!");
            return;
        }

        foreach (var charAction in action.actions)
        {
            switch (charAction.actionType)
            {
                case CharacterAction.ActionType.LookAt:
                    ExecuteLookAt(action.targetCharacter, charAction.target, charAction.duration);
                    break;

                case CharacterAction.ActionType.MoveTo:
                    ExecuteMoveTo(action.targetCharacter, charAction.target, charAction.duration);
                    break;

                case CharacterAction.ActionType.Idle:
                    // Idle - nic nie robić
                    Debug.Log($"[DialogActionTrigger] {action.targetCharacter.name} idle action");
                    break;

                case CharacterAction.ActionType.ChangeCamera:
                    ExecuteChangeCamera(charAction.cameraToActivate);
                    break;

                case CharacterAction.ActionType.Toggle:
                    ExecuteToggle(charAction.toggleObject);
                    break;

                case CharacterAction.ActionType.EnablePickable:
                    ExecuteEnablePickable(charAction.pickableObject);
                    break;

                case CharacterAction.ActionType.DisableComponent:
                    ExecuteDisableComponent(charAction.targetGameObject, charAction.componentName);
                    break;

                case CharacterAction.ActionType.EnableComponent:
                    ExecuteEnableComponent(charAction.targetGameObject, charAction.componentName);
                    break;
            }
        }
    }

    void ExecuteLookAt(GameObject character, Transform target, float duration)
    {
        var lookAt = character.GetComponent<CharacterLookAt>();
        if (lookAt == null)
            lookAt = character.AddComponent<CharacterLookAt>();

        lookAt.LookAtTarget(target, duration);
        Debug.Log($"[DialogActionTrigger] {character.name} looking at {target.name}");
    }

    void ExecuteMoveTo(GameObject character, Transform target, float duration)
    {
        var moveTo = character.GetComponent<CharacterMoveTo>();
        if (moveTo == null)
            moveTo = character.AddComponent<CharacterMoveTo>();

        moveTo.MoveToTarget(target.position, duration);
        Debug.Log($"[DialogActionTrigger] {character.name} moving to {target.name}");
    }

    void ExecuteChangeCamera(Camera newCamera)
    {
        if (newCamera == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Camera to activate is null!");
            return;
        }

        // Wyłącz główną kamerę gracza
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
            Debug.Log($"[DialogActionTrigger] Main camera disabled: {mainCamera.name}");
        }

        // Włącz wybraną kamerę z inspektora
        newCamera.gameObject.SetActive(true);
        Debug.Log($"[DialogActionTrigger] Camera changed to: {newCamera.name}");
    }

    void ExecuteToggle(GameObject toggleObject)
    {
        if (toggleObject == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Toggle object is null!");
            return;
        }

        IToggleable toggleable = toggleObject.GetComponent<IToggleable>();
        if (toggleable != null)
        {
            toggleable.Toggle();
            Debug.Log($"[DialogActionTrigger] Toggled {toggleObject.name} - IsActive: {toggleable.IsActive()}");
        }
        else
        {
            Debug.LogWarning($"[DialogActionTrigger] {toggleObject.name} nie ma IToggleable component!");
        }
    }

    void ExecuteEnablePickable(GameObject pickableObject)
    {
        if (pickableObject == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Pickable object is null!");
            return;
        }

        Pickable pickable = pickableObject.GetComponent<Pickable>();
        if (pickable != null)
        {
            // 🔥 Włącz component zamiast GameObjectu
            pickable.enabled = true;
            pickableObject.SetActive(true); // Włącz też GameObject na wypadek
            Debug.Log($"[DialogActionTrigger] Enabled Pickable component: {pickableObject.name}");
        }
        else
        {
            Debug.LogWarning($"[DialogActionTrigger] {pickableObject.name} nie ma Pickable component!");
        }
    }

    // 🔥 NOWA FUNKCJA: Wyłącz specificzny skrypt/komponent na obiekcie
    void ExecuteDisableComponent(GameObject target, string componentName)
    {
        if (target == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Target GameObject is null!");
            return;
        }

        if (string.IsNullOrEmpty(componentName))
        {
            Debug.LogWarning("[DialogActionTrigger] Component name is empty!");
            return;
        }

        var component = target.GetComponent(componentName);
        if (component != null && component is Behaviour behaviour)
        {
            behaviour.enabled = false;
            Debug.Log($"[DialogActionTrigger] ✅ Wyłączono component '{componentName}' na {target.name}");
        }
        else
        {
            Debug.LogWarning($"[DialogActionTrigger] ❌ Component '{componentName}' nie znaleziony na {target.name}");
        }
    }

    // 🔥 NOWA FUNKCJA: Włącz specificzny skrypt/komponent na obiekcie
    void ExecuteEnableComponent(GameObject target, string componentName)
    {
        if (target == null)
        {
            Debug.LogWarning("[DialogActionTrigger] Target GameObject is null!");
            return;
        }

        if (string.IsNullOrEmpty(componentName))
        {
            Debug.LogWarning("[DialogActionTrigger] Component name is empty!");
            return;
        }

        var component = target.GetComponent(componentName);
        if (component != null && component is Behaviour behaviour)
        {
            behaviour.enabled = true;
            Debug.Log($"[DialogActionTrigger] ✅ Włączono component '{componentName}' na {target.name}");
        }
        else
        {
            Debug.LogWarning($"[DialogActionTrigger] ❌ Component '{componentName}' nie znaleziony na {target.name}");
        }
    }
}
