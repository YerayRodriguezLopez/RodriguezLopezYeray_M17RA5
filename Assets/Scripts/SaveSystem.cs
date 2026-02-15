using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public string[] inventoryItems;
    public bool[] collectablesCollected;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private string saveFilePath;

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

        saveFilePath = Application.persistentDataPath + "/savegame.json";
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Guardar posición del jugador
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.rotation;
        }

        // Guardar inventario
        var inventory = InventoryManager.Instance.GetInventory();
        data.inventoryItems = new string[inventory.Count];
        for (int i = 0; i < inventory.Count; i++)
        {
            data.inventoryItems[i] = inventory[i].itemName;
        }

        // Guardar coleccionables
        CollectableItem[] collectables = FindObjectsOfType<CollectableItem>();
        data.collectablesCollected = new bool[collectables.Length];
        for (int i = 0; i < collectables.Length; i++)
        {
            data.collectablesCollected[i] = false; // Marcar los que faltan
        }

        // Convertir a JSON y guardar
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Partida guardada en: " + saveFilePath);
        UIManager.Instance?.ShowMessage("Partida guardada");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No hay partida guardada");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Cargar posición del jugador
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = data.playerPosition;
            player.transform.rotation = data.playerRotation;
            cc.enabled = true;
        }

        // Cargar inventario
        InventoryManager.Instance.ClearInventory();
        // Aquí deberías cargar los items desde sus nombres

        Debug.Log("Partida cargada");
        UIManager.Instance?.ShowMessage("Partida cargada");
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }
}