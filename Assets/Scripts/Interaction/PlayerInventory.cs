using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Prosty system inwentarza dla zbieranych przedmiotów
/// Jeden singleton dla całej gry
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private List<string> items = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log($"[PlayerInventory] Dodano: {itemName}. Razem przedmiotów: {items.Count}");
    }

    public void RemoveItem(string itemName)
    {
        if (items.Remove(itemName))
        {
            Debug.Log($"[PlayerInventory] Usunięto: {itemName}. Razem przedmiotów: {items.Count}");
        }
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (item == itemName)
                count++;
        }
        return count;
    }

    public List<string> GetAllItems()
    {
        return new List<string>(items);
    }

    public void ClearInventory()
    {
        items.Clear();
    }
}
