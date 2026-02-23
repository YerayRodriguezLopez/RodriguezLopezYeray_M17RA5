using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private List<ItemData> items = new List<ItemData>();

    /// <summary>Fired whenever the inventory changes (add / remove).</summary>
    public event System.Action OnInventoryChanged;

    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    #region Public API
    // ════════════════════════════════════════════════════════════════════════

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        items.Add(item);
        OnInventoryChanged?.Invoke();

        // Auto-equip if equippable
        if (item.isEquippable)
            EquipmentManager.Instance?.EquipItem(item);

        UIManager.Instance?.ShowMessage($"Obtingut: {item.itemName}");
        Debug.Log($"[Inventory] Added: {item.itemName}  (total: {items.Count})");
    }

    public bool RemoveItem(ItemData item)
    {
        bool removed = items.Remove(item);
        if (removed)
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] Removed: {item.itemName}");
        }
        return removed;
    }

    public bool HasItem(ItemData item) => items.Contains(item);

    public int ItemCount => items.Count;

    /// <summary>Returns a copy of the current inventory list.</summary>
    public List<ItemData> GetAllItems() => new List<ItemData>(items);

    // ─── Save / Load support ─────────────────────────────────────────────────

    public List<string> GetItemNames()
    {
        List<string> names = new List<string>();
        foreach (ItemData item in items)
            names.Add(item.itemName);
        return names;
    }

    /// <summary>
    /// Restore inventory from a list of names.
    /// Requires all ItemData assets to live in a Resources/Items folder.
    /// </summary>
    public void LoadFromNames(List<string> names)
    {
        items.Clear();
        foreach (string name in names)
        {
            ItemData asset = Resources.Load<ItemData>($"Items/{name}");
            if (asset != null)
                items.Add(asset);
            else
                Debug.LogWarning($"[Inventory] Could not find ItemData for '{name}' in Resources/Items/");
        }
        OnInventoryChanged?.Invoke();
    }

    #endregion
}
