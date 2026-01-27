using UnityEngine;

/// <summary>
/// Skrypt dla obiektów do zbierania (pickable items)
/// Dodaj ten skrypt na GameObject przedmiotu i ustaw jaką nazwę powinien mieć
/// </summary>
public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private string itemName = "Przedmiot";
    [SerializeField] private bool destroyOnPickup = true;

    public string GetItemName()
    {
        return itemName;
    }

    public string GetPickDescription()
    {
        return $"E zabierz ({itemName})";
    }

    public void Pick()
    {
        Debug.Log($"[Pickable] Zebrałeś: {itemName}");
        
        // Dodaj do inwentarza jeśli ma
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemName);
        }

        // Usuń przedmiot ze świata
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
