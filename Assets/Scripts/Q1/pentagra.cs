using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class pentagra : MonoBehaviour
{
    [Header("Obiekt do aktywacji")]
    [SerializeField] private GameObject objectToActivate; // Przypisz obiekt w inspektorze
    
    [Header("Pojawienie się drugiego obiektu")]
    [SerializeField] private GameObject spawnObjectOnActivate;
    
    [Header("Trigger - kamera i blokada ruchu")]
    [SerializeField] private GameObject newCamera; // Nowa kamera do aktywacji
    
    [Header("Ustawienia")]
    [SerializeField] private bool activateOnlyOnce = true; // Aktywuj tylko raz
    [SerializeField] private string nextSceneName = "BattleScene2"; // Scena do załadowania
    [SerializeField] private float delayBeforeSceneChange = 4f; // Opóźnienie w sekundach
    
    private bool hasActivated = false;
    private PlayerMovement playerMovement;
    private Camera mainCamera;

    void Start()
    {
        // 📷 Upewnij się że nowa kamera jest wyłączona na starcie
        if (newCamera != null)
        {
            newCamera.SetActive(false);
            Debug.Log("[pentagra] Nowa kamera wyłączona na starcie");
        }
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// Trigger collider - gdy gracz dotknie
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        // Sprawdź czy to gracz
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[pentagra] Gracz dotknął pentagram!");
            
            // Sprawdź czy już aktywowaliśmy
            if (activateOnlyOnce && hasActivated)
            {
                Debug.Log("[pentagra] Już aktywowano, ignoruję");
                return;
            }
            
            // 🔒 Zablokuj poruszanie gracza
            playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                Debug.Log("[pentagra] Zablokowano PlayerMovement");
            }
            
            // 📷 Wyłącz główną kamerę
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(false);
                Debug.Log("[pentagra] Wyłączono główną kamerę");
            }
            
            // 📷 Włącz nową kamerę
            if (newCamera != null)
            {
                newCamera.SetActive(true);
                Debug.Log("[pentagra] Włączono nową kamerę");
            }
            
            // Aktywuj główny obiekt
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
                
                // Zapisz stan w GameStateManager
                StatefulObject stateful = objectToActivate.GetComponent<StatefulObject>();
                if (stateful != null && GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetState(stateful.ID, true);
                    Debug.Log($"[pentagra] Zapisano stan: {stateful.ID} = true");
                }
                
                Debug.Log($"[pentagra] ✅ Uaktywniono: {objectToActivate.name}");
            }
            
            // Pojawi się drugi obiekt
            if (spawnObjectOnActivate != null)
            {
                spawnObjectOnActivate.SetActive(true);
                
                // Zapisz stan drugiego obiektu w GameStateManager
                StatefulObject spawnStateful = spawnObjectOnActivate.GetComponent<StatefulObject>();
                if (spawnStateful != null && GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetState(spawnStateful.ID, true);
                    Debug.Log($"[pentagra] Zapisano stan drugiego obiektu: {spawnStateful.ID} = true");
                }
                
                Debug.Log($"[pentagra] ✅ Uaktywniono drugi obiekt: {spawnObjectOnActivate.name}");
            }
            
            hasActivated = true;
            
            // ⏳ PO 4 SEKUNDACH ZMIEŃ SCENĘ
            StartCoroutine(LoadSceneWithDelay());
        }
    }
    
    /// <summary>
    /// Czeka 4 sekundy i załadowuje nową scenę
    /// </summary>
    private IEnumerator LoadSceneWithDelay()
    {
        Debug.Log($"[pentagra] ⏳ Czekam {delayBeforeSceneChange} sekund przed zmianą sceny...");
        yield return new WaitForSeconds(delayBeforeSceneChange);
        Debug.Log($"[pentagra] 🎮 Ładuję scenę: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
    
    /// <summary>
    /// Gdy gracz opuści trigger - odblokuj ruch, przywróć kamerę
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[pentagra] Gracz opuścił trigger!");
            
            // 🔓 Odblokuj poruszanie gracza
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                Debug.Log("[pentagra] Odblokowali PlayerMovement");
            }
            
            // 📷 Włącz główną kamerę
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(true);
                Debug.Log("[pentagra] Włączono główną kamerę");
            }
            
            // 📷 Wyłącz nową kamerę
            if (newCamera != null)
            {
                newCamera.SetActive(false);
                Debug.Log("[pentagra] Wyłączono nową kamerę");
            }
        }
    }
}
