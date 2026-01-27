using UnityEngine;
using System.Collections;

public class GP2 : MonoBehaviour
{
    [Header("Objects")]
    public GameObject objectToActivate;      // Obiekt do włączenia w fazie 1
    public GameObject pinguAIObject;         // Obiekt z PinguAI
    public GameObject pingu2Object;          // Obiekt z Pingu2
    public GameObject spawnerLiczbBinarnychObject; // Obiekt z komponentem Spawner

    [Header("References")]
    public GravityStun gravityStun;
    public BattlePlayer battlePlayer;

    private MonoBehaviour pinguAI;
    private MonoBehaviour pingu2;
    private MonoBehaviour spawnerLiczbBinarnych;

    [Header("Phase 1 Timing")]
    public float phase1Duration = 5f;

    [Header("Phase 2 Timing")]
    public float delayBeforeStart = 3f;
    public float stunDuration = 2f;

    [Header("Auto Disable")]
    public float autoDisableTime = 40f; // Po ilu sekundach wszystko wyłączyć

    void Start()
    {
        if (gravityStun == null)
            gravityStun = GetComponent<GravityStun>();
        
        if (battlePlayer == null)
            battlePlayer = GetComponent<BattlePlayer>();

        // Pobieramy komponenty z przypisanych obiektów
        if (pinguAIObject != null)
            pinguAI = pinguAIObject.GetComponent<PinguAI>();
        else
            Debug.LogWarning("GP2: Nie przypisano pinguAIObject!");

        if (pingu2Object != null)
            pingu2 = pingu2Object.GetComponent<Pingu2>();
        else
            Debug.LogWarning("GP2: Nie przypisano pingu2Object!");

        if (spawnerLiczbBinarnychObject != null)
            spawnerLiczbBinarnych = spawnerLiczbBinarnychObject.GetComponent<SpawnerLiczbBinarnych>();
        else
            Debug.LogWarning("GP2: Nie przypisano spawnerLiczbBinarnychObject!");

        StartCoroutine(BattlePhases());

        // Uruchamiamy timer do automatycznego wyłączenia
        StartCoroutine(AutoDisableAfterTime(autoDisableTime));
    }

    IEnumerator BattlePhases()
    {
        // 🔹 FAZA 1
        if (pingu2 != null)
            pingu2.enabled = false;

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (spawnerLiczbBinarnych != null)
            spawnerLiczbBinarnych.enabled = true;

        yield return new WaitForSeconds(phase1Duration);

        // 🔹 FAZA 2
        if (pinguAI != null)
            pinguAI.enabled = false;

        if (pingu2 != null)
            pingu2.enabled = true;

        yield return new WaitForSeconds(delayBeforeStart);

        if (gravityStun != null && battlePlayer != null)
        {
            battlePlayer.disableJump = true;

            if (stunDuration > 0)
                gravityStun.GravityActive(true, stunDuration);
            else
                gravityStun.GravityActive(true);
        }
    }

    private IEnumerator AutoDisableAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Wyłączamy wszystkie skrypty
        if (pinguAI != null)
            pinguAI.enabled = false;

        if (pingu2 != null)
            pingu2.enabled = false;

        if (spawnerLiczbBinarnych != null)
            spawnerLiczbBinarnych.enabled = false;

        // Wyłączamy aktywowany obiekt
        if (objectToActivate != null)
            objectToActivate.SetActive(false);

        Debug.Log("GP2: Wszystkie włączone skrypty zostały wyłączone po " + time + " sekundach.");
    }
}
