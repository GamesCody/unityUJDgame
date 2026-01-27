using UnityEngine;
using System.Collections;

public class GP1 : MonoBehaviour
{
    [Header("Objects")]
    public GameObject gravityStunObject;
    public GameObject vimSpawnerObject;
    public GameObject blueScreenObject;
    public GameObject projectileSpawnerObject; // obiekt z ProjectileSpawner

    [Header("Times")]
    public float gravityDuration = 20f;    // czas działania grawitacji
    public float totalActiveTime = 20f;    // całkowity czas działania GP1
    public float blueScreenDelay = 1f;     // opóźnienie BlueScreen

    private GravityStun gravityStun;
    private VimPlatformSpawner vimSpawner;
    private ProjectileSpawner projectileSpawner;

    void Start()
    {
        if (gravityStunObject != null)
            gravityStun = gravityStunObject.GetComponent<GravityStun>();

        if (vimSpawnerObject != null)
            vimSpawner = vimSpawnerObject.GetComponent<VimPlatformSpawner>();

        if (projectileSpawnerObject != null)
            projectileSpawner = projectileSpawnerObject.GetComponent<ProjectileSpawner>();

        StartCoroutine(GP1Routine());
    }

    IEnumerator GP1Routine()
    {
        // 1️⃣ Włączamy VimSpawner od razu
        if (vimSpawner != null) vimSpawner.enabled = true;

        // 2️⃣ Włączamy grawitację z dodatkowym +3s
        if (gravityStun != null)
            gravityStun.GravityActive(true, gravityDuration + 3f);

        // 3️⃣ Czekamy przed BlueScreen
        yield return new WaitForSeconds(blueScreenDelay+1f);

        // 4️⃣ Włączamy BlueScreen (poprzez aktywację obiektu)
        if (blueScreenObject != null)
            blueScreenObject.SetActive(true);

        // 5️⃣ Włączamy ProjectileSpawner i ustawiamy maxConcurrentTurrets = 2
        if (projectileSpawner != null)
        {
            projectileSpawner.enabled = true;
        }

        // 6️⃣ Czekamy aż GP1 się skończy
        yield return new WaitForSeconds(totalActiveTime);

        // 7️⃣ Wyłączamy wszystko
        if (vimSpawner != null) vimSpawner.enabled = false;

        if (blueScreenObject != null)
            blueScreenObject.SetActive(false);

        if (projectileSpawner != null)
            projectileSpawner.enabled = false;

        if (gravityStun != null)
            gravityStun.GravityActive(false);
    }
}
