using UnityEngine;
using System.Collections;

public class CharacterMoveTo : MonoBehaviour
{
    private Vector3 moveTarget;
    private float moveDuration;
    private Coroutine moveCoroutine;
    private float moveSpeed = 5f;

    [SerializeField] private float stoppingDistance = 0.5f;

    private Animator animator;

    void Start()
    {
        // Szukaj Animator na tym obiekcie lub dzieciach
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void MoveToTarget(Vector3 target, float duration)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveTarget = target;
        moveDuration = duration;
        moveCoroutine = StartCoroutine(MoveCoroutine());
    }

    IEnumerator MoveCoroutine()
    {
        // 🔥 Włącz animację chodzenia
        if (animator != null)
            animator.SetBool("isWalking", true);

        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < moveDuration)
        {
            // Oblicz dystans do celu
            float distanceToTarget = Vector3.Distance(transform.position, moveTarget);

            if (distanceToTarget < stoppingDistance)
            {
                // Dotarliśmy na miejsce
                break;
            }

            // Lerp pozycji
            float progress = elapsed / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, moveTarget, progress);
            transform.position = newPos;

            // Obróć postać w kierunku ruchu
            Vector3 direction = moveTarget - transform.position;
            if (direction.magnitude > 0.01f)
            {
                direction.y = 0;
                transform.rotation = Quaternion.LookRotation(direction);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ustaw dokładną pozycję na końcu
        transform.position = moveTarget;

        // 🔥 Wyłącz animację chodzenia
        if (animator != null)
            animator.SetBool("isWalking", false);

        moveCoroutine = null;
    }

    void OnDisable()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        // 🔥 Na wypadek wyłączenia - zawsze wyłącz animację
        if (animator != null)
            animator.SetBool("isWalking", false);
    }
}
