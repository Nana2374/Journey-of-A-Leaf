using System.Collections.Generic;
using UnityEngine;

public class FurnitureInventory : MonoBehaviour
{
    public static FurnitureInventory Instance { get; private set; }

    // Stored furniture: ID and how many
    private Dictionary<int, int> storedItems = new Dictionary<int, int>();

    public event System.Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(int furnitureID, int quantity = 1)
    {
        if (!storedItems.ContainsKey(furnitureID))
            storedItems[furnitureID] = 0;
        storedItems[furnitureID] += quantity;
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added {quantity} of ID {furnitureID} to inventory. Total: {storedItems[furnitureID]}");
    }

    public bool RemoveItem(int furnitureID, int quantity = 1)
    {
        if (!storedItems.ContainsKey(furnitureID) || storedItems[furnitureID] < quantity)
            return false;
        storedItems[furnitureID] -= quantity;
        if (storedItems[furnitureID] <= 0)
            storedItems.Remove(furnitureID);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(int furnitureID)
    {
        return storedItems.ContainsKey(furnitureID) ? storedItems[furnitureID] : 0;
    }

    public bool HasItem(int furnitureID)
    {
        return storedItems.ContainsKey(furnitureID) && storedItems[furnitureID] > 0;
    }

    public Dictionary<int, int> GetAllItems() => storedItems;
}