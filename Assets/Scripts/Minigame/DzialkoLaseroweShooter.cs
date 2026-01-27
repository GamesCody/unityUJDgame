using System.Collections;
using UnityEngine;

public class DzialkoLaseroweShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject laserTurret;
    public Transform player;
    public Transform lufa;

    public GameObject laser1;
    public GameObject laser2;
    public GameObject laser3;

    [Header("Timing")]
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 4f;
    public float timeBetweenLaser1 = 2f;
    public float timeBetweenLaser2 = 0.5f;
    public float finalLaserTime = 1f;

    [Header("Rotation")]
    public bool rotateOnlyZ = true;
    public float rotationZOffset = 0f;
    public float rotationSmooth = 0f;

    [Header("Pause Behavior")]
    public bool pauseRotationDuringLaser2 = true;
    public bool pauseRotationDuringLaser3 = true;

    [Header("Mode")]
    public bool singleUse = false;

    bool turretActive;
    bool rotationPaused;
    Coroutine rotateCoroutine;

    void Start()
    {
        // Znajdź lasery i turret w prefabie
        EnsureLocalReferences();

        // NIE WYŁĄCZAJ TURRET – ma być zawsze widoczny
        if (laser1) laser1.SetActive(false);
        if (laser2) laser2.SetActive(false);
        if (laser3) laser3.SetActive(false);

        StartCoroutine(TurretLoop());
    }

    IEnumerator TurretLoop()
    {
        turretActive = true;

        // Start rotacji
        if (rotateCoroutine == null)
            rotateCoroutine = StartCoroutine(RotateTurret());

        // Odczekaj losowy czas przed sekwencją
        yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

        // Wykonaj sekwencję laserów
        yield return StartCoroutine(LaserSequence());

        turretActive = false;

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }

        // Jeśli turret jest singleUse, zniszcz go po zakończeniu
        if (singleUse)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator RotateTurret()
    {
        while (turretActive)
        {
            if (!player || rotationPaused)
            {
                yield return null;
                continue;
            }

            Vector3 dir = player.position - laserTurret.transform.position;

            if (rotateOnlyZ)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationZOffset;
                Quaternion target = Quaternion.Euler(0, 0, angle);

                laserTurret.transform.rotation = rotationSmooth > 0f
                    ? Quaternion.Lerp(laserTurret.transform.rotation, target, Time.deltaTime * rotationSmooth)
                    : target;
            }

            yield return null;
        }
    }

    IEnumerator LaserSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (laser1) laser1.SetActive(true);
        yield return new WaitForSeconds(timeBetweenLaser1);
        if (laser1) laser1.SetActive(false);

        if (pauseRotationDuringLaser2) rotationPaused = true;
        if (laser2) laser2.SetActive(true);
        yield return new WaitForSeconds(timeBetweenLaser2);
        if (laser2) laser2.SetActive(false);
        rotationPaused = false;

        if (pauseRotationDuringLaser3) rotationPaused = true;
        if (laser3) laser3.SetActive(true);
        yield return new WaitForSeconds(finalLaserTime);
        if (laser3) laser3.SetActive(false);
        rotationPaused = false;
    }

    void EnsureLocalReferences()
    {
        if (!laserTurret)
            laserTurret = FindChild("LaserTurret", "Turret", "Laser");

        if (!laser1)
            laser1 = FindChild("Laser1");

        if (!laser2)
            laser2 = FindChild("Laser2");

        if (!laser3)
            laser3 = FindChild("Laser3");
    }

    GameObject FindChild(params string[] names)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            foreach (string n in names)
            {
                if (t.name.ToLower() == n.ToLower())
                    return t.gameObject;
            }
        }
        return null;
    }
}
