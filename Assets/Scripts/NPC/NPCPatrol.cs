using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public string npcID = "NPC_1"; // Ustaw unikalny identyfikator w Inspectorze
    public Transform[] patrolPoints;
    public float waitTime = 2f;

    private NavMeshAgent agent;
    private int currentPointIndex = 0;
    private float waitTimer;
    private bool playerInRange = false;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Przywróć pozycję i rotację NPC po zmianie sceny
        if (PlayerPositionManager.Instance != null)
            PlayerPositionManager.Instance.RestoreNPCTransform(npcID, transform);

        // Poczekaj 1 frame, zanim zaczniesz patrolować
        StartCoroutine(StartPatrolNextFrame());
    }

    IEnumerator StartPatrolNextFrame()
    {
        yield return null; // Poczekaj jedną klatkę
        GoToNextPoint();
    }

    void Update()
    {
        if (playerInRange && player != null)
        {
            agent.isStopped = true;

            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            if (agent.remainingDistance < 0.2f && !agent.pathPending)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    GoToNextPoint();
                    waitTimer = 0f;
                }
            }
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            GoToNextPoint();
        }
    }

    public void SaveState()
    {
        if (PlayerPositionManager.Instance != null)
            PlayerPositionManager.Instance.SaveNPCTransform(npcID, transform);
    }
}
