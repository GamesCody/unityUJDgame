using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ButtonSceneChanger : MonoBehaviour
{
    [Header("Source")]
    public GameObject playerObject;    // Obiekt gracza z PlayerHealth

    [Header("UI")]
    public TMP_Text healthText;        // Tekst do wpisania wartości

    [Header("Scene")]
    public string sceneName = "Game";

    private PlayerHealth playerHealth;
    private int healthValue;

    void Start()
    {
        // 🔹 Natychmiast odblokuj kursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 🔹 Pobieramy komponent PlayerHealth
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth != null){CollectHealth();Debug.Log("ButtonSceneChanger: Znaleziono PlayerHealth!");}
                
                
            else
                Debug.LogWarning("ButtonSceneChanger: Nie znaleziono komponentu PlayerHealth!");
        }
        else
        {
            Debug.LogWarning("ButtonSceneChanger: Player object nie przypisany!");
        }
    }

    public void LoadSceneByName(string _)
    {
        // Upewnij się, że kursor jest odblokowany
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("ButtonSceneChanger: Kursor odblokowany przed zmianą sceny");
        
        // Zapisz healthValue w GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetPlayerHealth(healthValue);
            Debug.Log("ButtonSceneChanger: Ustawiono healthValue w GameStateManager: " + healthValue);
        }
        
        SceneManager.LoadScene(sceneName);
    }

    void CollectHealth()
    {
        Debug.Log("ButtonSceneChanger: CollectHealth() wywołana!");
        Debug.Log("playerObject: " + (playerObject != null ? "OK" : "NULL"));
        Debug.Log("healthText: " + (healthText != null ? "OK" : "NULL"));
        Debug.Log("playerHealth: " + (playerHealth != null ? "OK" : "NULL"));

        if (playerObject == null || healthText == null || playerHealth == null)
        {
            Debug.LogWarning("ButtonSceneChanger: Brakuje referencji!");
            return;
        }

        // 🔹 Pobieramy wartość zdrowia bezpośrednio
        healthValue = playerHealth.currentHealth;

        // Wstaw do tekstu
        healthText.text = healthValue.ToString();
        Debug.Log("ButtonSceneChanger: Ustawiono health text na: " + healthValue);

        // Wyłącz obiekt gracza
        playerObject.SetActive(false);
        Debug.Log("ButtonSceneChanger: Wyłączono player object");
    }
}
