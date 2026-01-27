using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f;

    private CharacterController controller;
    private Camera playerCamera;
    private float rotationX = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    // Animator for player walk animations.
    // Ustaw w Inspectorze Animator, który zawiera stany:
    // "Chodzenie_start" (jednorazowa animacja startu),
    // "chodzenie" (pętla),
    // "chodzenie_end" (jednorazowa animacja końca).
    // W kodzie ustawiamy bool `isWalking` oraz trigger `StopWalking`.
    public Animator animator;
    private bool wasWalking = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Jeśli Animator nie został przypisany w Inspectorze, spróbuj go wyszukać
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Diagnostyka animatora: sprawdź czy istnieje Controller i wymagane parametry
        if (animator == null)
        {
            Debug.LogWarning("[PlayerMovement] Animator nie przypisany ani nie znaleziony w dzieciach. Przypisz Animator w Inspectorze.");
        }
        else
        {
            if (animator.runtimeAnimatorController == null)
                Debug.LogWarning("[PlayerMovement] Animator nie ma przypisanego Runtime Controller (Animator Controller).");

            if (!HasAnimatorParameter("isWalking", AnimatorControllerParameterType.Bool))
                Debug.LogWarning("[PlayerMovement] Animator nie zawiera parametru Bool 'isWalking'. Dodaj go w Animatorze.");

            if (!HasAnimatorParameter("StopWalking", AnimatorControllerParameterType.Trigger))
                Debug.LogWarning("[PlayerMovement] Animator nie zawiera parametru Trigger 'StopWalking'. Dodaj go w Animatorze.");

            if (!animator.gameObject.activeInHierarchy)
                Debug.LogWarning("[PlayerMovement] Animator znajduje się na nieaktywnym obiekcie. Upewnij się, że jest aktywny.");
        }

        // Jeśli mamy zapisane dane pozycji i rotacji, przywróć je
        if (PlayerPositionManager.Instance != null && PlayerPositionManager.Instance.hasSavedTransform)
        {
            Debug.Log("[PlayerMovement] Przywracam zapisane pozycje i rotacje gracza.");
            ApplySavedTransform();
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void ApplySavedTransform()
    {
        var ppm = PlayerPositionManager.Instance;
        if (ppm == null || !ppm.hasSavedTransform) return;

        if (controller != null) controller.enabled = false;

        // Przywracamy pozycję i rotację gracza
        transform.position = ppm.savedPosition;
        transform.rotation = ppm.savedRotation;

        // Przywracamy rotację kamery
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = ppm.savedCameraLocalRotation;

            // Synchronizujemy rotationX (żeby kamera nie "skakała")
            float x = ppm.savedCameraLocalRotation.eulerAngles.x;
            if (x > 180f) x -= 360f;
            rotationX = x;
        }

        if (controller != null) controller.enabled = true;

        // Czyścimy zapis, by nie nadpisywać ponownie pozycją
        ppm.hasSavedTransform = false;

        Debug.Log("[PlayerMovement] Zastosowano zapisane pozycje i rotacje gracza.");
    }

    // Pomocnicza metoda do sprawdzenia czy Animator zawiera dany parametr (by uniknąć cichych błędów)
    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == paramName && p.type == paramType) return true;
        }
        return false;
    }

    // Flagi do uniknięcia spamowania logów
    private bool animatorLoggedMissing = false;

    void Update()
    {
        // Ruch klawiaturą
        if (Keyboard.current != null)
        {
            float moveX = 0f;
            float moveZ = 0f;
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            moveInput = new Vector2(moveX, moveZ).normalized;
        }

        // Obrót myszką
        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue() * sensitivity * Time.deltaTime;
        }

        // Poruszanie
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // --- Animator: ustawianie stanów chodzenia (z diagnostyką) ---
        bool isWalking = moveInput.sqrMagnitude > 0.001f;
        if (animator != null)
        {
            // Bezpieczne ustawianie parametrów tylko jeśli istnieją
            if (HasAnimatorParameter("isWalking", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("isWalking", isWalking);
            }
            else
            {
                // Pokazuj warning raz — jeżeli brakuje parametru, użytkownik sam musi go dodać
                Debug.LogWarning("[PlayerMovement] Brakuje parametru 'isWalking' (Bool) w Animatorze. Dodaj go w zakładce Parameters.");
            }

            // Trigger StopWalking tylko gdy przechodzimy z ruchu do stania
            if (!isWalking && wasWalking)
            {
                if (HasAnimatorParameter("StopWalking", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("StopWalking");
                    Debug.Log("[PlayerMovement] Wyzwolono StopWalking trigger.");
                }
                else
                {
                    Debug.LogWarning("[PlayerMovement] Brakuje parametru 'StopWalking' (Trigger) w Animatorze. Dodaj go w zakładce Parameters.");
                }
            }
        }
        else if (!animatorLoggedMissing)
        {
            Debug.LogWarning("[PlayerMovement] Animator nie jest przypisany — animacje nie będą odtwarzane.");
            animatorLoggedMissing = true;
        }
        wasWalking = isWalking;

        controller.SimpleMove(move * speed);

        // Obracanie kamery (góra/dół)
        rotationX -= lookInput.y;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Obracanie gracza (lewo/prawo)
        transform.Rotate(Vector3.up * lookInput.x);
    }
}
