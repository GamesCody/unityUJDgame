using UnityEngine;
using System.Collections;

public class GP3 : MonoBehaviour
{
    [Header("Objects")]
    public GameObject bookWallSpawnerObject;  // Obiekt z BookWallSpawner
    public GameObject dzialkoLaseroweObject;  // Obiekt z DzialkoLaserowe

    private MonoBehaviour bookWallSpawner;
    private DzialkoLaserowe dzialkoLaserowe;

    [Header("Laser Settings")]
    public float increaseInterval = 2f;  // Co ile sekund zwiększamy działka
    public float totalDuration = 20f;    // Po ilu sekundach wyłączyć wszystko

    private Coroutine increaseCoroutine;

    void Start()
    {
        // 🔹 Pobieramy komponenty
        if (bookWallSpawnerObject != null)
        {
            bookWallSpawner = bookWallSpawnerObject.GetComponent<BookWallSpawner>();
            if (bookWallSpawner != null)
                bookWallSpawner.enabled = true;
            else
                Debug.LogWarning("GP3: Nie znaleziono komponentu BookWallSpawner!");
        }

        if (dzialkoLaseroweObject != null)
        {
            dzialkoLaserowe = dzialkoLaseroweObject.GetComponent<DzialkoLaserowe>();
            if (dzialkoLaserowe != null)
            {
                dzialkoLaserowe.enabled = true;
                dzialkoLaserowe.maxConcurrentTurrets = 1;

                // Uruchamiamy coroutine zwiększania działek
                //Too OP ---------// increaseCoroutine = StartCoroutine(IncreaseTurretsLoop());
            }
            else
            {
                Debug.LogWarning("GP3: Nie znaleziono komponentu DzialkoLaserowe!");
            }
        }

        // Startujemy timer wyłączenia wszystkich skryptów
        StartCoroutine(DisableAllAfterTime(totalDuration));
    }

    private IEnumerator IncreaseTurretsLoop()
    {
        while (dzialkoLaserowe != null)
        {
            yield return new WaitForSeconds(increaseInterval);

            // Zwiększamy maxConcurrentTurrets o jego wartość + 1
            dzialkoLaserowe.maxConcurrentTurrets += dzialkoLaserowe.maxConcurrentTurrets + 1;

            Debug.Log("GP3: maxConcurrentTurrets = " + dzialkoLaserowe.maxConcurrentTurrets);
        }
    }

    private IEnumerator DisableAllAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Wyłączamy BookWallSpawner
        if (bookWallSpawner != null)
            bookWallSpawner.enabled = false;

        // Wyłączamy DzialkoLaserowe
        if (dzialkoLaserowe != null)
            dzialkoLaserowe.enabled = false;

        // Zatrzymujemy coroutine zwiększania działek
        if (increaseCoroutine != null)
            StopCoroutine(increaseCoroutine);

        Debug.Log("GP3: Wszystkie skrypty wyłączone po " + time + " sekundach.");
    }
}
