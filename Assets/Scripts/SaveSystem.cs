using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3    playerPosition;
    public Quaternion playerRotation;
    public List<string> inventoryItemNames = new List<string>();
    public List<string> collectedItemIds   = new List<string>();  // scene collectibles
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

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
    #region Save
    // ════════════════════════════════════════════════════════════════════════

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Player transform
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation;
        }

        // Inventory
        if (InventoryManager.Instance != null)
            data.inventoryItemNames = InventoryManager.Instance.GetItemNames();

        // Collected scene items (items whose GameObject has been disabled/destroyed)
        CollectableItem[] collectables = FindObjectsOfType<CollectableItem>(true);
        foreach (CollectableItem c in collectables)
        {
            if (c.IsCollected)
                data.collectedItemIds.Add(c.ItemId);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);

        UIManager.Instance?.ShowMessage("Partida guardada!");
        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region Load
    // ════════════════════════════════════════════════════════════════════════

    public bool HasSave() => File.Exists(SavePath);

    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveSystem] No save file found.");
            return;
        }

        string   json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Player transform
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.SetPositionAndRotation(data.playerPosition, data.playerRotation);
            cc.enabled = true;
        }

        // Inventory
        InventoryManager.Instance?.LoadFromNames(data.inventoryItemNames);

        // Restore collected state of scene collectibles
        CollectableItem[] collectables = FindObjectsOfType<CollectableItem>(true);
        foreach (CollectableItem c in collectables)
        {
            if (data.collectedItemIds.Contains(c.ItemId))
                c.SetCollectedState(true);
        }

        Debug.Log("[SaveSystem] Game loaded.");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    #endregion
}
