using UnityEngine;
using UnityEngine.InputSystem;

public class BattlePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float flySpeed = 5f;
    public bool disableJump = false;   // ← Wyłącz skok

    public bool GravityStun = false;   // ← Twój tryb grawitacji
    private bool isGrounded;

    private Rigidbody2D rb;
    private GravityStun gravityStunComponent;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityStunComponent = GetComponent<GravityStun>();
    }

    void Update()
    {
        float x = 0;
        float y = 0;

        // Lewo / Prawo
        if (Keyboard.current.aKey.isPressed) x = -1;
        if (Keyboard.current.dKey.isPressed) x = 1;

        // Determine gravity mode from GravityStun component when available
        bool gravityMode = gravityStunComponent != null ? gravityStunComponent.gravityActive : GravityStun;

        // Tryb NORMALNY (bez grawitacji)
        if (!gravityMode)
        {
            if (Keyboard.current.wKey.isPressed) y = 1;
            if (Keyboard.current.sKey.isPressed) y = -1;

            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(x * moveSpeed, y * moveSpeed);
        }
        else
        {
            // Tryb GRAWITACJI
            float targetGravity = gravityStunComponent != null ? gravityStunComponent.gravityScaleOn : 3f;
            rb.gravityScale = targetGravity;

            rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

            // W = skok
            if (Keyboard.current.wKey.wasPressedThisFrame)
                Jump();
        }
    }

    void Jump()
    {
        if (!isGrounded || disableJump) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
