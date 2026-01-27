using UnityEngine;

/// <summary>
/// Skrypt dla laptopa Macka - obsługuje zbieranie i pojawienie się drugiego obiektu
/// Działa razem z Pickable componentem
/// </summary>
public class laptopMacka : MonoBehaviour, IPickable
{
    [SerializeField] private string itemName = "Laptop Macka";
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Pojawienie się drugiego obiektu")]
    [SerializeField] private GameObject spawnObjectOnPickup; 
    private bool isProcessing = false;

    public string GetItemName()
    {
        return itemName;
    }

    public string GetPickDescription()
    {
        return $"E umieść ({itemName})";
    }

    public void Pick()
    {
        // Guard: jeśli już w trakcie przetwarzania - ignoruj kolejne kliknięcia
        if (isProcessing) return;
        isProcessing = true;

        // Wyłącz collider tak, żeby obiekt nie dał się ponownie kliknąć
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        var col2 = GetComponent<Collider2D>();
        if (col2 != null) col2.enabled = false;

        Debug.Log($"[laptopMacka] Zebrałeś: {itemName}");
        
        // Zapisz stan w GameStateManager
        StatefulObject stateful = GetComponent<StatefulObject>();
        if (stateful != null && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(stateful.ID, false);
            Debug.Log($"[laptopMacka] Zapisano stan: {stateful.ID} = false");
        }
        
        // Pojawi się drugi obiekt w tym miejscu
        if (spawnObjectOnPickup != null)
        {
            spawnObjectOnPickup.SetActive(true);
            
            // Zapisz stan drugiego obiektu w GameStateManager
            StatefulObject spawnStateful = spawnObjectOnPickup.GetComponent<StatefulObject>();
            if (spawnStateful != null && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(spawnStateful.ID, true);
                Debug.Log($"[laptopMacka] Zapisano stan drugiego obiektu: {spawnStateful.ID} = true");
            }
        } 
        
        // Dodaj do inwentarza
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemName);
        }

        // Usuń ten przedmiot ze świata (po zabezpieczeniu)
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
