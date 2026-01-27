using System.Collections;
using UnityEngine;
using TMPro;

public class GravityStun : MonoBehaviour
{
    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(0f, 0f); // usuń vertical knockback, tylko X jeśli chcesz
    public bool useKnockback = false;

    [Header("References")]
    public Rigidbody2D playerRB;
    public TMP_Text gravityText;

    [Header("Gravity")]
    public float gravityScaleOn = 3f;

    [HideInInspector]
    public bool gravityActive = false;

    private float originalGravity;
    private Coroutine currentCoroutine = null;

    void Start()
    {
        if (playerRB == null)
            playerRB = GetComponent<Rigidbody2D>();

        originalGravity = playerRB.gravityScale;

        if (gravityText != null)
            gravityText.gameObject.SetActive(false);
    }

    void Update()
    {
        // tylko zmieniamy gravityScale
        playerRB.gravityScale = gravityActive ? gravityScaleOn : originalGravity;

        if (gravityText != null)
            gravityText.gameObject.SetActive(gravityActive);
    }

    public void GravityActive(bool state, float reverseDelay = -1f)
    {
        if (gravityActive == state)
            return;

        gravityActive = state;

        // opcjonalny knockback X (nie blokuje skoku)
        if (state && useKnockback)
        {
            playerRB.AddForce(knockbackForce, ForceMode2D.Impulse);
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (reverseDelay > 0f)
        {
            currentCoroutine = StartCoroutine(ReverseAfterDelay(state, reverseDelay));
        }
    }

    private IEnumerator ReverseAfterDelay(bool originalState, float delay)
    {
        yield return new WaitForSeconds(delay);
        gravityActive = !originalState;
        currentCoroutine = null;
    }
}
