using UnityEngine;

public class BookWallSpawner : MonoBehaviour
{
    public GameObject bookWallPrefab;
    public int amountToSpawn = 30;
    public float spawnInterval = 1.5f;
    public Vector2 spawnOffset;
    public int partsToDisable = 2;

    int spawnedCount = 0;
    bool[] lastCombination;

    void OnEnable()
    {
        spawnedCount = 0;
        InvokeRepeating(nameof(Spawn), 0f, spawnInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    void Spawn()
    {
        if (spawnedCount >= amountToSpawn)
        {
            CancelInvoke();
            return;
        }

        Vector3 spawnPos = transform.position + (Vector3)spawnOffset;
        GameObject wall = Instantiate(bookWallPrefab, spawnPos, Quaternion.identity);

        BookWall md = wall.GetComponent<BookWall>();
        int partCount = md.parts.Length;

        bool[] newCombo = GenerateUniqueCombination(partCount);

        md.ApplyCombination(newCombo);

        lastCombination = newCombo;
        spawnedCount++;
    }

    bool[] GenerateUniqueCombination(int count)
    {
        bool[] combo;

        do
        {
            combo = new bool[count];
            int disabled = 0;

            while (disabled < partsToDisable)
            {
                int i = Random.Range(1, count);
                if (!combo[i])
                {
                    combo[i] = true;
                    disabled++;
                }
            }

        } while (SameAsLast(combo));

        return combo;
    }

    bool SameAsLast(bool[] combo)
    {
        if (lastCombination == null) return false;

        for (int i = 0; i < combo.Length; i++)
        {
            if (combo[i] != lastCombination[i])
                return false;
        }
        return true;
    }
}
