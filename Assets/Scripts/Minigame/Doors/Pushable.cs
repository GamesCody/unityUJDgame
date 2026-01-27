using UnityEngine;

public class Pushable : MonoBehaviour
{
    GridMover mover;

    void Awake()
    {
        mover = GetComponent<GridMover>();
    }

    public bool TryPush(Vector2 dir)
    {
        return mover.TryMove(dir);
    }
}
