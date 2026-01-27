using UnityEngine;

public class Pingu2 : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 4f;
    public float diveSpeed = 10f;
    public float riseSpeed = 6f;

    public float heightAbovePlayer = 5f;
    public float waitAfterHit = 2f;
    public float timeBetweenDives = 3f;

    Rigidbody2D rb;
    Animator anim;

    bool diving;
    bool rising;
    bool waiting;

    float diveTimer;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        diveTimer = timeBetweenDives;
    }

    void Update()
    {
        if (waiting || diving || rising) return;

        // Podążanie nad graczem (tylko X + wysokość)
        diveTimer -= Time.deltaTime;
        Vector2 target = new Vector2(player.position.x, player.position.y + heightAbovePlayer);
        Vector2 newPos = Vector2.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        if (diveTimer <= 0)
            StartDive();
    }

    void StartDive()
    {
        diving = true;
        anim.SetBool("Jump", true);
        rb.linearVelocity = Vector2.down * diveSpeed;
    }



    void FixedUpdate()
    {
        if (diving)
        {
            // Gdy osiągnie poziom Y gracza → stop nurkowania
            if (transform.position.y <= player.position.y)
            {
                diving = false;
                waiting = true;
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("Jump", false);
                Invoke(nameof(StartRise), waitAfterHit);
            }
        }

        if (rising)
        {
            float targetY = player.position.y + heightAbovePlayer;

            if (transform.position.y >= targetY)
            {
                rb.linearVelocity = Vector2.zero;
                rising = false;
                diveTimer = timeBetweenDives;
            }
        }
    }

    void StartRise()
    {
        waiting = false;
        rising = true;
        rb.linearVelocity = Vector2.up * riseSpeed;
    }
}
