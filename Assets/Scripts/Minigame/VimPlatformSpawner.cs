using UnityEngine;
using System.Collections;

public class VimPlatformSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Vector2 spawnset;              // dokładna pozycja (X,Y)
    public GameObject VinPrefab;
    public int amounToSpawn = 10;
    public float spawnnInterval = 1.5f;

    private int spawned = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (spawned < amounToSpawn)
        {
            SpawnPlatform();
            spawned++;

            yield return new WaitForSeconds(spawnnInterval);
        }
    }

    void SpawnPlatform()
    {
        Vector3 spawnPos = new Vector3(spawnset.x, spawnset.y, 0f);
        Instantiate(VinPrefab, spawnPos, Quaternion.identity);
    }
}
