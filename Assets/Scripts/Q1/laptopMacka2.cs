using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Skrypt dla laptopa Macka 2 - pojawia się gdy laptop 1 zostaje zebrany
/// Dwuetapowy: najpierw "E hakuj laptop" -> zmiana sceny -> potem "E weź Laptop" -> zniszczenie
/// </summary>
public class laptopMacka2 : MonoBehaviour, IPickable
{
    [SerializeField] private string itemName = "Laptop Macka 2";
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private string sceneToLoad = "Game";
    [SerializeField] private InkDialogController dialogController; // 🔥 DODAJ REFERENCJĘ
    
    private bool isHacked = false;
    private StatefulObject stateful;
    private bool isProcessing = false;

    void Start()
    {
        stateful = GetComponent<StatefulObject>();
        
        // 🔥 ZNALEŹĆ DIALOG CONTROLLER JEŚLI NIE MA PRZYPISANEGO
        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();
        
        // Jeśli laptopMacka2 nie ma wpisu w GameStateManager, dodaj go
        if (stateful != null && GameStateManager.Instance != null)
        {
            if (!GameStateManager.Instance.objectStates.Exists(s => s.ID == stateful.ID))
            {
                GameStateManager.Instance.SetState(stateful.ID, true);
                Debug.Log($"[laptopMacka2] Dodano do GameStateManager: {stateful.ID} = true");
            }
        }
        
        // Sprawdź czy był już haczony (zapisane w PlayerPrefs)
        if (PlayerPrefs.GetInt("laptopMacka2_hacked", 0) == 1)
        {
            isHacked = true;
            Debug.Log("[laptopMacka2] Przywrócono stan: laptop był już haczony");
        }
    }

    public string GetItemName()
    {
        return itemName;
    }

    public string GetPickDescription()
    {
        return isHacked ? $"E weź ({itemName})" : "E hakuj laptop";
    }

    public void Pick()
    {
        // Guard: jeśli już przetwarzamy interakcję, ignoruj dodatkowe kliknięcia
        if (isProcessing) return;
        isProcessing = true;

        // Wyłącz collider (2D/3D) żeby nie dało się kliknąć ponownie
        var c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        var c2 = GetComponent<Collider2D>();
        if (c2 != null) c2.enabled = false;

        if (!isHacked)
        {
            // ETAP 1: Hakuj laptop i załaduj scenę
            Debug.Log($"[laptopMacka2] Hakujesz: {itemName}");
            isHacked = true;
            
            // Zapisz pozycję gracza przed zmianą sceny
            if (PlayerPositionManager.Instance != null)
            {
                PlayerPositionManager.Instance.SavePlayerPositionNow();
                Debug.Log("[laptopMacka2] Zapisano pozycję gracza przed zmianą sceny");
            }
            
            // Zapisz stan haczenia
            PlayerPrefs.SetInt("laptopMacka2_hacked", 1);
            PlayerPrefs.Save();
            
            // Załaduj scenę normalnie
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            // ETAP 2: Weź laptop i zniszcz go
            Debug.Log($"[laptopMacka2] Zebrałeś: {itemName}");
            
            // Zapisz stan w GameStateManager - laptop został zebrany
            if (stateful != null && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(stateful.ID, false);
                Debug.Log($"[laptopMacka2] Zapisano stan: {stateful.ID} = false");
            }
            
            // 🔥 USTAW ZMIENNĄ W INK - laptop został zwrócony Mackiemu (PRAWIDŁOWY SPOSÓB JAK W QUESTTRIGGER)
            if (dialogController != null)
            {
                dialogController.SetInkVariable("laptopReturned", true);
                Debug.Log("[laptopMacka2] Ustawiono zmienną Ink: laptopReturned = true");
            }
            else
            {
                Debug.LogWarning("[laptopMacka2] Brak przypisanego InkDialogController!");
            }
            
            // Dodaj do inwentarza
            PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddItem(itemName);
            }

            // Wyczyść stan haczenia z PlayerPrefs
            PlayerPrefs.DeleteKey("laptopMacka2_hacked");
            PlayerPrefs.Save();

            // Usuń ten przedmiot ze świata
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
