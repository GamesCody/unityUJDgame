using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LiczbyBinarne : MonoBehaviour
{
    public TMP_Text tekst;
    public float interval = 0.5f;
    public float lifeTime = 5f; // Czas życia obiektu w sekundach

    private void Start()
    {
        if (tekst == null)
        {
            Debug.LogError("Nie przypisano komponentu Text!");
            return;
        }

        InvokeRepeating("ZmienLiczby", 0f, interval);

        // Automatyczne zniszczenie obiektu po określonym czasie
        Destroy(gameObject, lifeTime);
    }

    void ZmienLiczby()
    {
        string liczbaBinarna = "";
        for (int i = 0; i < 5; i++)
        {
            int bit = Random.Range(0, 2);
            liczbaBinarna += bit.ToString();
        }
        tekst.text = liczbaBinarna;
    }
}
