using UnityEngine;
using System.Collections.Generic;

public class SpawnerLiczbBinarnych : MonoBehaviour
{
    public GameObject[] prefabs;          // Lista obiektów do respawnu
    public Transform[] spawnPoints;       // Lista punktów spawnu
    public float minRespawnTime = 1f;     // Minimalny czas między spawnami
    public float maxRespawnTime = 3f;     // Maksymalny czas między spawnami
    public int maxObjects = 10;           // Maksymalna liczba jednoczesnych obiektów

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private Coroutine spawnLoopCoroutine;

    private void OnEnable()
    {
        // Kopiujemy wszystkie punkty spawn do listy dostępnych
        availableSpawnPoints.Clear();
        availableSpawnPoints.AddRange(spawnPoints);

        if (spawnLoopCoroutine == null)
            spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Usuwamy z listy zniszczone obiekty i zwalniamy ich punkt spawn
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedObjects[i] == null)
                {
                    spawnedObjects.RemoveAt(i);
                }
            }

            // Spawn tylko jeśli jest wolny punkt i nie przekroczyliśmy maxObjects
            if (spawnedObjects.Count < maxObjects && availableSpawnPoints.Count > 0)
            {
                SpawnObject();
            }

            float waitTime = Random.Range(minRespawnTime, maxRespawnTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnObject()
    {
        // Losowy prefab
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

        // Losowy dostępny punkt spawn
        int index = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[index];

        // Tworzymy obiekt
        GameObject obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        spawnedObjects.Add(obj);

        // Usuwamy punkt z listy dostępnych
        availableSpawnPoints.RemoveAt(index);

        // Po żywotności obiektu (lifeTime) zwracamy punkt spawn do dostępnych
        LiczbyBinarne lb = obj.GetComponent<LiczbyBinarne>();
        if (lb != null)
        {
            float life = lb.lifeTime;
            StartCoroutine(ReturnSpawnPointAfterTime(spawnPoint, life));
        }
    }

    private System.Collections.IEnumerator ReturnSpawnPointAfterTime(Transform spawnPoint, float time)
    {
        yield return new WaitForSeconds(time);
        availableSpawnPoints.Add(spawnPoint);
    }
}