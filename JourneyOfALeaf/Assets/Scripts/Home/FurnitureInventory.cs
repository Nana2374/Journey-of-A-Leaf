using System.Collections.Generic;
using UnityEngine;

public class FurnitureInventory : MonoBehaviour
{
    public static FurnitureInventory Instance { get; private set; }

    [SerializeField] private ObjectsDatabaseSO database; // drag in Inspector

    private Dictionary<int, int> storedItems = new Dictionary<int, int>();

    public event System.Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private int GetMaxQuantity(int furnitureID)
    {
        var data = database.objectsData.Find(d => d.ID == furnitureID);
        return data != null ? data.MaxQuantity : 1;
    }

    // Returns total owned (in inventory + placed in world)
    public int GetTotalOwned(int furnitureID)
    {
        return storedItems.ContainsKey(furnitureID) ? storedItems[furnitureID] : 0;
    }

    public void AddItem(int furnitureID, int quantity = 1)
    {

        Debug.Log($"AddItem called: ID={furnitureID}, quantity={quantity}");
        int current = GetTotalOwned(furnitureID);
        int max = GetMaxQuantity(furnitureID);

        // Clamp to max — never exceed what the player should own
        int canAdd = Mathf.Min(quantity, max - current);
        if (canAdd <= 0)
        {
            Debug.Log($"Cannot add ID {furnitureID} — already at max ({max})");
            return;
        }

        if (!storedItems.ContainsKey(furnitureID))
            storedItems[furnitureID] = 0;

        storedItems[furnitureID] += canAdd;
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added {canAdd} of ID {furnitureID}. Total in inventory: {storedItems[furnitureID]}/{max}");
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