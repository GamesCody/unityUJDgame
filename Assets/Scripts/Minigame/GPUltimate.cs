using UnityEngine;
using System.Collections;

public class GPUltimate : MonoBehaviour
{
    [Header("Phase Objects")]
    public GameObject GP3Object;
    public GameObject GP1Object;
    public GameObject GP2Object;

    [Header("Additional Objects")]
    public GameObject bookWallSpawnerObject;
    public GameObject devilHandObject;
    public GameObject dzialkoLaseroweObject;
    public GameObject objectToActivateAtEnd;  // Obiekt do uaktywnienia na koniec

    [Header("Objects to Activate with DevilHand")]
    public GameObject devilHandActivateObject1;
    public GameObject devilHandActivateObject2;

    [Header("Phase Durations (seconds)")]
    public float GP3Duration = 61f;
    public float GP1Duration = 21f;
    public float GP2Duration = 40f;

    [Header("Spawner Settings")]
    public int amountToSpawn = 10;

    [Header("Laser Settings")]
    public int maxConcurrentTurrets = 12;

    private GP3 gp3Script;
    private GP1 gp1Script;
    private GP2 gp2Script;
    private BookWallSpawner bookWallSpawner;
    private DevilHand devilHand;
    private DzialkoLaserowe dzialkoLaserowe;

    void Start()
    {
        // Pobieramy komponenty GP
        if (GP3Object != null) gp3Script = GP3Object.GetComponent<GP3>();
        if (GP1Object != null) gp1Script = GP1Object.GetComponent<GP1>();
        if (GP2Object != null) gp2Script = GP2Object.GetComponent<GP2>();

        // Pobieramy dodatkowe komponenty
        if (bookWallSpawnerObject != null) bookWallSpawner = bookWallSpawnerObject.GetComponent<BookWallSpawner>();
        if (devilHandObject != null) devilHand = devilHandObject.GetComponent<DevilHand>();
        if (dzialkoLaseroweObject != null) dzialkoLaserowe = dzialkoLaseroweObject.GetComponent<DzialkoLaserowe>();

        StartCoroutine(UltimateSequence());
    }

    private IEnumerator UltimateSequence()
    {
        // 🔹 Sekwencja przed GP3
        // 1️⃣ Uruchom BookWallSpawner
        if (bookWallSpawner != null)
        {
            bookWallSpawner.enabled = true;
            bookWallSpawner.amountToSpawn = amountToSpawn;
            Debug.Log("GPUltimate: Włączono BookWallSpawner z amountToSpawn = " + amountToSpawn);
            yield return new WaitForSeconds(20f); // działa 20 sekund
            bookWallSpawner.enabled = false;
            Debug.Log("GPUltimate: Wyłączono BookWallSpawner");
        }

        // 2️⃣ Po 5 sekund włącz DevilHand
        yield return new WaitForSeconds(5f);
        if (devilHand != null)
        {
            // Włącz oba obiekty wraz z DevilHand
            if (devilHandActivateObject1 != null)
                devilHandActivateObject1.SetActive(true);
            if (devilHandActivateObject2 != null)
                devilHandActivateObject2.SetActive(true);

            devilHand.enabled = true;
            Debug.Log("GPUltimate: Włączono DevilHand + 2 obiekty");
            yield return new WaitForSeconds(20f); // działa 20 sekund
            devilHand.enabled = false;

            // Wyłącz oba obiekty gdy DevilHand się skończy
            if (devilHandActivateObject1 != null)
                devilHandActivateObject1.SetActive(false);
            if (devilHandActivateObject2 != null)
                devilHandActivateObject2.SetActive(false);

            Debug.Log("GPUltimate: Wyłączono DevilHand + 2 obiekty");
        }

        // 3️⃣ Po 5 sekund włącz DzialkoLaserowe
        yield return new WaitForSeconds(5f);
        if (dzialkoLaserowe != null)
        {
            dzialkoLaserowe.enabled = true;
            dzialkoLaserowe.maxConcurrentTurrets = maxConcurrentTurrets;
            Debug.Log("GPUltimate: Włączono DzialkoLaserowe z maxConcurrentTurrets = " + maxConcurrentTurrets);
            yield return new WaitForSeconds(20f);
            dzialkoLaserowe.enabled = false;
            Debug.Log("GPUltimate: Wyłączono DzialkoLaserowe");
        }

        // 🔹 FAZA 1: GP3
        yield return new WaitForSeconds(5f); // krótka przerwa przed GP3
        if (gp3Script != null)
        {
            gp3Script.enabled = true;
            Debug.Log("GPUltimate: Włączono GP3");
            yield return new WaitForSeconds(GP3Duration);
            gp3Script.enabled = false;
            Debug.Log("GPUltimate: Wyłączono GP3");
        }

        // 🔹 FAZA 2: GP1
        if (gp1Script != null)
        {
            gp1Script.enabled = true;
            Debug.Log("GPUltimate: Włączono GP1");
            yield return new WaitForSeconds(GP1Duration);
            gp1Script.enabled = false;
            Debug.Log("GPUltimate: Wyłączono GP1");
        }

        // 🔹 FAZA 3: GP2
        if (gp2Script != null)
        {
            gp2Script.enabled = true;
            Debug.Log("GPUltimate: Włączono GP2");
            yield return new WaitForSeconds(GP2Duration);
            gp2Script.enabled = false;
            Debug.Log("GPUltimate: Wyłączono GP2");
        }

        Debug.Log("GPUltimate: Wszystkie fazy zakończone");

        // Uaktywnij obiekt na koniec (po 7 sekundach)
        if (objectToActivateAtEnd != null)
        {
            Debug.Log("GPUltimate: Oczekiwanie 7s przed uaktywnieniem obiektu...");
            yield return new WaitForSeconds(7f);
            objectToActivateAtEnd.SetActive(true);
            Debug.Log("GPUltimate: Uaktywniono " + objectToActivateAtEnd.name);
        }
    }
}
