using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public float damageCooldown = 0.5f;
    private float lastDamageTime = -1f;

    public TMP_Text HealthText;
    public string scena = "BattleScene";

    // Migotanie
    private SpriteRenderer spriteRenderer;
    public float blinkDuration = 0.5f;
    public float blinkInterval = 0.1f;

    // Kolory zdrowia
    private Color healthyColor = Color.green;      // 5 hp
    private Color goodColor = new Color(1f, 1f, 0f); // żółty - 4 hp
    private Color warnColor = new Color(1f, 0.65f, 0f); // pomarańczowy - 2-3 hp
    private Color criticalColor = Color.red;       // 1 hp

    void Start()
    {
        currentHealth = maxHealth;
        HealthText.text = currentHealth.ToString();

        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHealthColor();
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;
        currentHealth -= amount;

        Debug.Log($"Gracz otrzymał obrażenia! Aktualne życie: {currentHealth}");
        HealthText.text = currentHealth.ToString();

        // Zmień kolor w zależności od zdrowia
        UpdateHealthColor();

        // Uruchom migotanie
        StartCoroutine(Blink());

        if (currentHealth <= 0)
        {
            Debug.Log("Gracz nie żyje!");
            SceneManager.LoadScene(scena);
        }
    }

    void UpdateHealthColor()
    {
        if (currentHealth >= 5)
            spriteRenderer.color = healthyColor; // Zielony
        else if (currentHealth == 4)
            spriteRenderer.color = goodColor; // Żółty
        else if (currentHealth == 3 || currentHealth == 2)
            spriteRenderer.color = warnColor; // Pomarańczowy
        else if (currentHealth == 1)
            spriteRenderer.color = criticalColor; // Czerwony
    }

    IEnumerator Blink()
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval * 2;
        }

        spriteRenderer.enabled = true; // Upewnij się że sprite wróci
    }
}
