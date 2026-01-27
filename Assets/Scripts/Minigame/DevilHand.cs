using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DevilHand : MonoBehaviour
{
    [Header("Prefab do spawnu")]
    public GameObject objectToSpawn;

    [Header("Gracz")]
    public Transform player;

    [Header("Czasy działania (w sekundach)")]
    public float spawnDelay = 2f;             // czas przed pierwszym spawnem
    public float enableColliderDelay = 0.5f;  // po ilu sekundach włączyć collider
    public float activeDuration = 1f;         // po ilu sekundach wyłączyć collider i obiekt
    public float respawnDelay = 2f;           // czas oczekiwania przed kolejnym spawnem

    [Header("Kontrola spawnu")]
    public bool Aktywacja = true;             // czy spawn jest aktywny

    private GameObject spawnedObject;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Collider[] spawnedColliders3D;
    private Collider2D[] spawnedColliders2D;
    private Coroutine spawnLoopCoroutine;

    void OnEnable()
    {
        if (objectToSpawn == null || player == null)
        {
            Debug.LogWarning("DevilHand: brak prefab lub gracza!");
            return;
        }

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

        // Niszcz wszystkie spawned obiekty
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnDelay);

        while (Aktywacja)
        {
            // spawn obiektu w miejscu gracza
            spawnedObject = Instantiate(objectToSpawn, player.position, Quaternion.identity);
            spawnedObject.SetActive(true);
            spawnedObjects.Add(spawnedObject);  // 🔹 Dodaj do listy

            // pobieramy wszystkie collidery 3D i 2D w obiekcie i dzieciach (uwzględniamy też nieaktywne)
            spawnedColliders3D = spawnedObject.GetComponentsInChildren<Collider>(true);
            spawnedColliders2D = spawnedObject.GetComponentsInChildren<Collider2D>(true);

            int found3D = spawnedColliders3D != null ? spawnedColliders3D.Length : 0;
            int found2D = spawnedColliders2D != null ? spawnedColliders2D.Length : 0;
            Debug.Log($"DevilHand: znaleziono colliderów 3D={found3D}, 2D={found2D}");

            // Wyłączamy wszystkie na start
            if (found3D > 0)
            {
                foreach (var col in spawnedColliders3D)
                    col.enabled = false;
            }

            if (found2D > 0)
            {
                foreach (var col in spawnedColliders2D)
                    col.enabled = false;
            }

            // czekamy enableColliderDelay, a potem włączamy wszystkie collidery
            yield return new WaitForSeconds(enableColliderDelay);

            if (found3D > 0)
            {
                foreach (var col in spawnedColliders3D)
                    col.enabled = true;
            }

            if (found2D > 0)
            {
                foreach (var col in spawnedColliders2D)
                    col.enabled = true;
            }

            // po activeDuration wyłączamy collider i obiekt
            yield return new WaitForSeconds(activeDuration);

            if (found3D > 0)
            {
                foreach (var col in spawnedColliders3D)
                    col.enabled = false;
            }

            if (found2D > 0)
            {
                foreach (var col in spawnedColliders2D)
                    col.enabled = false;
            }

            if (spawnedObject != null)

            // Usuwamy zniszczony obiekt z listy
            spawnedObjects.Remove(spawnedObject);
                Destroy(spawnedObject);

            // czekamy przed kolejnym spawnem
            yield return new WaitForSeconds(respawnDelay);
        }
    }
}
