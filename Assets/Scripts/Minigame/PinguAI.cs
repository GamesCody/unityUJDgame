using UnityEngine;

public class PinguAI : MonoBehaviour
{
    public float moveSpeed = 4f;

    [Header("Timers")]
    public float randomFlyTime = 3f;
    public float chaseTime = 2f;

    [Header("Random Area")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Target")]
    public Transform player;

    Rigidbody2D rb;

    Vector2 randomTarget;
    float stateTimer;

    enum State
    {
        RandomFly,
        Chase
    }

    State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SwitchToRandom();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        if (currentState == State.RandomFly)
        {
            FlyTo(randomTarget);

            if (Vector2.Distance(transform.position, randomTarget) < 0.5f)
                PickNewRandomTarget();

            if (stateTimer <= 0)
                SwitchToChase();
        }
        else if (currentState == State.Chase)
        {
            FlyTo(player.position);

            if (stateTimer <= 0)
                SwitchToRandom();
        }
    }

    void FlyTo(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    void PickNewRandomTarget()
    {
        randomTarget = new Vector2(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y)
        );
    }

    void SwitchToRandom()
    {
        currentState = State.RandomFly;
        stateTimer = randomFlyTime;
        PickNewRandomTarget();
    }

    void SwitchToChase()
    {
        currentState = State.Chase;
        stateTimer = chaseTime;
    }
}
