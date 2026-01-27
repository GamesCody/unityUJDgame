using UnityEngine;
using System.Collections;

public class KobyTrap : MonoBehaviour
{
    public GameObject myObject;   
    public GameObject myObject2;          // obiekt do włączania
    private GravityStun gravityStun;      // GravityStun na tym obiekcie

    [Header("Czasy")]
    public float initialDelay = 4f;       // czekaj na start
    public float gravityDuration = 2f;    // czas gravity
    public float waitBeforeObject = 1f;   // czekaj przed włączeniem obiektu
    public float objectActiveDuration = 5f; // czas włączenia obiektu

    void Start()
    {
        gravityStun = GetComponent<GravityStun>();

        if (gravityStun == null)
            Debug.LogWarning("KobyTrap: Nie znaleziono GravityStun!");

        if (myObject == null)
            Debug.LogWarning("KobyTrap: myObject nie został przypisany!");

        StartCoroutine(TrapCycle());
    }

    IEnumerator TrapCycle()
    {
        // początkowe czekanie
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
                        if (myObject != null)
                myObject2.SetActive(false);
            // 1️⃣ włączamy gravity
            if (gravityStun != null)
                gravityStun.GravityActive(true, gravityDuration); // włączy na 2 sekundy, potem false

            // czekamy aż gravity się wyłączy
            yield return new WaitForSeconds(gravityDuration);

                            if (myObject != null)
                myObject2.SetActive(true);
            // 2️⃣ dodatkowa pauza przed włączeniem obiektu
            yield return new WaitForSeconds(waitBeforeObject);

            // 3️⃣ włączamy obiekt
            if (myObject != null)
                myObject.SetActive(true);

            // 4️⃣ czekamy ile ma być włączony
            yield return new WaitForSeconds(objectActiveDuration);

            // 5️⃣ wyłączamy obiekt
            if (myObject != null)
                myObject.SetActive(false);

            // opcjonalnie, jeśli chcesz powtarzać w kółko bez dodatkowego delay, pętla zacznie się od nowa
        }
    }
}
