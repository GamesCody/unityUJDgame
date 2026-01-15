using UnityEngine;
using UnityEngine.InputSystem;

public class playerHandler : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float speed = 4.0f;
    public float rotationSpeed = 720f;

    private Animator animator;
    private Transform mainCameraTransform;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Pobieramy transform kamery, aby wiedzieć, gdzie jest "przód" dla gracza
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        
        float h = 0;
        float v = 0;

        if (Keyboard.current.wKey.isPressed) v = 1;
        if (Keyboard.current.sKey.isPressed) v = -1;
        if (Keyboard.current.aKey.isPressed) h = -1;
        if (Keyboard.current.dKey.isPressed) h = 1;

        // Obliczanie kierunku WZGLĘDEM KAMERY
        // Bierzemy wektory "przód" i "prawo" kamery
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        // Zerujemy oś Y
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Tworzymy docelowy kierunek ruchu
        Vector3 moveDirection = (forward * v + right * h).normalized;

        //  Obsługa animacji i poruszania
        if (moveDirection.magnitude >= 0.1f)
        {
            animator.SetBool("isWalking", true);

            // Płynny obrót w stronę, w którą idziemy
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Ruch w stronę, w którą patrzy kamera/klawisze
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
