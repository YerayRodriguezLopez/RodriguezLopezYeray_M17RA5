using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<ItemData> inventory = new List<ItemData>();

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    private void Awake()
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

    public void AddItem(ItemData item)
    {
        inventory.Add(item);
        onInventoryChanged?.Invoke();

        Debug.Log($"Item añadido: {item.itemName}");
    }

    public void RemoveItem(ItemData item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
            onInventoryChanged?.Invoke();
        }
    }

    public bool HasItem(ItemData item)
    {
        return inventory.Contains(item);
    }

    public List<ItemData> GetInventory()
    {
        return new List<ItemData>(inventory);
    }

    public void ClearInventory()
    {
        inventory.Clear();
        onInventoryChanged?.Invoke();
    }
}