using UnityEngine;
using System.Collections;

public class CharacterLookAt : MonoBehaviour
{
    private Transform lookTarget;
    private float lookDuration;
    private Coroutine lookCoroutine;

    public void LookAtTarget(Transform target, float duration)
    {
        if (lookCoroutine != null)
            StopCoroutine(lookCoroutine);

        lookTarget = target;
        lookDuration = duration;
        lookCoroutine = StartCoroutine(LookCoroutine());
    }

    IEnumerator LookCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < lookDuration && lookTarget != null)
        {
            // Oblicz kierunek do celu
            Vector3 direction = lookTarget.position - transform.position;
            direction.y = 0; // Nie patrzmy w górę/dół, tylko w poziomie

            if (direction.magnitude > 0.01f)
            {
                // Obrót w kierunku celu
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 3f // Szybkość obrotu
                );
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        lookCoroutine = null;
    }

    void OnDisable()
    {
        if (lookCoroutine != null)
            StopCoroutine(lookCoroutine);
    }
}
