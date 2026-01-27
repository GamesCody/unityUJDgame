using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DzialkoLaserowe : MonoBehaviour
{
    public GameObject turretPrefab;
    public Transform[] spawnPoints;
    public Transform player;

    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 4f;

    public int maxConcurrentTurrets = 3;
    public bool spawnedTurretsSingleUse = true;

    List<GameObject> activeTurrets = new List<GameObject>();
    Coroutine spawnLoopCoroutine;

    void OnEnable()
    {
        if (spawnLoopCoroutine == null)
            spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));

            activeTurrets.RemoveAll(t => t == null);

            if (activeTurrets.Count >= maxConcurrentTurrets)
                continue;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            SpawnTurret(point);
        }
    }

void SpawnTurret(Transform p)
{
    var turret = Instantiate(turretPrefab, p.position, p.rotation);

    // FORCE ACTIVATE ROOT BEFORE Start()
    turret.SetActive(true);

    var shooter = turret.GetComponent<DzialkoLaseroweShooter>();
    if (shooter)
    {
        shooter.player = player;
        shooter.singleUse = spawnedTurretsSingleUse;
    }

    activeTurrets.Add(turret);
}

}
