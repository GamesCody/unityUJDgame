using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Attach this to a GameObject with a Collider (2D or 3D) set as Trigger.
// When an object tagged "Player" enters the trigger, unlocks the door and returns to game.
public class WinTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player2D";
    [SerializeField] private string doorID = "BreakableDoor_1"; // 🔥 ID drzwi do odblokowywania
    [SerializeField] private string returnSceneName = "Game"; // 🔥 Scena do której wrócić

    void Reset()
    {
        // Try to mark existing collider as trigger so it's ready by default in the editor
        var col2D = GetComponent<Collider2D>();
        if (col2D != null)
            col2D.isTrigger = true;

    }

    void Start()
    {
        // 🔥 DEBUG: Sprawdzenie konfiguracji
        Collider2D col = GetComponent<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        Debug.Log($"[WinTrigger] Start: Collider2D={col != null}, isTrigger={col?.isTrigger}, Rigidbody2D={rb != null}");
        
        if (col == null)
            Debug.LogWarning("[WinTrigger] ❌ BRAK Collider2D!");
        if (rb == null)
            Debug.LogWarning("[WinTrigger] ❌ BRAK Rigidbody2D! Trigger events nie będą działać!");
    }
 void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("[ResetLevel] Reloading current scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[WinTrigger] Coś weszło: {other.gameObject.name}, tag: {other.tag}");
        
        if (other != null && other.CompareTag(playerTag))
        {
            Debug.Log("🎉 WYGRAŁEŚ! Odblokowywanie drzwi...");
            
            // 🔥 Oznacz drzwi jako włamane w GameStateManager
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetBreakableDoorState(doorID, true);
                Debug.Log($"[WinTrigger] Ustawiono stan drzwi: {doorID} = włamane");
            }
            
            // 🔥 Wróć do głównej sceny
            SceneManager.LoadScene(returnSceneName);
        }
    }

}
