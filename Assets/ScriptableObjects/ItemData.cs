using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public GameObject itemPrefab;

    [Header("Item Properties")]
    public ItemType itemType;
    public bool isEquippable;
    public GameObject equipmentModel;

    [Header("Effects")]
    public float healthRestore;
    public float damageBoost;
    public float speedBoost;
}

public enum ItemType
{
    Weapon,
    Consumable,
    QuestItem,
    Collectible
}