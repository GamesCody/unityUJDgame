using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{
    InputAction move;
    GridMover mover;

    void Awake()
    {
        mover = GetComponent<GridMover>();

        var map = new InputActionMap("game");
        move = map.AddAction("move", binding: "<Gamepad>/leftStick");

        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        map.Enable();
    }

    void Update()
    {
        Vector2 dir = move.ReadValue<Vector2>();
        if (dir.magnitude < 0.5f)
            return;

        dir = GetDominantDirection(dir);

        TryMove(dir);
    }

    Vector2 GetDominantDirection(Vector2 d)
    {
        return Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? new Vector2(Mathf.Sign(d.x), 0)
            : new Vector2(0, Mathf.Sign(d.y));
    }

    void TryMove(Vector2 dir)
    {
        // sprawdź czy jest obiekt do popchnięcia (Box lub ElectricBox)
        Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position + dir, 0.25f);

        if (hit != null)
        {
            Pushable p = hit.GetComponent<Pushable>();
            if (p != null && p.TryPush(dir))
            {
                mover.TryMove(dir);
                return;
            }
        }

        // nic nie blokuje → idź
        mover.TryMove(dir);
    }
}
