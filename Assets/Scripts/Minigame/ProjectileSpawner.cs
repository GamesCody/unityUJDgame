using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Ustawienia pocisków")]
    public GameObject projectilePrefab;
    public float spawnInterval = 1.5f;
    public float spawnRadius = 1.5f;        // wokół spawnera
    public float minSpeed = 3f;
    public float maxSpeed = 7f;
    
    [Header("Celowanie")]
    public float aimInaccuracy = 15f;        // rozrzut (w stopniach)
    private float timer = 0f;

    [Header("Losowy cooldown")]
    public float minActiveTime = 2f;
    public float maxActiveTime = 5f;
    public float minCooldownTime = 2f;
    public float maxCooldownTime = 4f;

    private bool isActive = true;
    private float stateTimer = 0f;
    private float currentStateDuration = 0f;

    void Start()
    {
        SetCooldownState();
    }

    void Update()
    {
        stateTimer += Time.deltaTime;
        if (isActive)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnProjectile();
                timer = 0f;
            }
            if (stateTimer >= currentStateDuration)
            {
                SetCooldownState();
            }
        }
        else
        {
            if (stateTimer >= currentStateDuration)
            {
                SetActiveState();
            }
        }
    }

    void SetActiveState()
    {
        isActive = true;
        stateTimer = 0f;
        currentStateDuration = Random.Range(minActiveTime, maxActiveTime);
    }

    void SetCooldownState()
    {
        isActive = false;
        stateTimer = 0f;
        currentStateDuration = Random.Range(minCooldownTime, maxCooldownTime);
    }

    void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        // znajdź gracza
        GameObject player = GameObject.FindWithTag("Player2D");
        if (player == null)
        {
            Debug.LogWarning("[ProjectileSpawner] Nie znaleziono gracza!");
            return;
        }

        // losowa pozycja w promieniu wokół spawnera
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 spawnPos = (Vector2)transform.position + randomOffset;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // kierunek w stronę gracza
        Vector2 dirToPlayer = ((Vector2)player.transform.position - spawnPos).normalized;

        // dodaj lekki losowy rozrzut
        float angleOffset = Random.Range(-aimInaccuracy, aimInaccuracy);
        dirToPlayer = Quaternion.Euler(0, 0, angleOffset) * dirToPlayer;

        // ustaw kierunek i losową prędkość
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetDirection(dirToPlayer);
            p.speed = Random.Range(minSpeed, maxSpeed);
        }

        // opcjonalnie usuń po 5 sekundach
        Destroy(proj, 5f);
    }
}
