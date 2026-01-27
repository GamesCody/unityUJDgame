using UnityEngine;

public class GridMover : MonoBehaviour
{
    public float gridStep = 1f;
    public float tolerance = 0.25f;

    public bool TryMove(Vector2 dir)
    {
        Vector2 target = (Vector2)transform.position + dir * gridStep;

        if (IsBlocked(target))
            return false;

        transform.position = target;
        return true;
    }

    bool IsBlocked(Vector2 target)
    {
        Collider2D hit = Physics2D.OverlapCircle(target, tolerance);
        // 🔥 Trigger colliders (goal, items) NIE blokują ruchu - tylko solid colliders
        if (hit != null && hit.gameObject != gameObject && !hit.isTrigger)
            return true;

        return false;
    }
}
