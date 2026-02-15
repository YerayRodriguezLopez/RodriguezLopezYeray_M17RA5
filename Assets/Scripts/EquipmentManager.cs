using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Equipment Sockets")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private Transform backSocket;

    private GameObject currentWeapon;
    private ItemData currentWeaponData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EquipItem(ItemData item)
    {
        if (!item.isEquippable) return;

        // Desequipar arma actual
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        // Equipar nueva arma
        if (item.equipmentModel != null && weaponSocket != null)
        {
            currentWeapon = Instantiate(item.equipmentModel, weaponSocket);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            currentWeaponData = item;

            Debug.Log($"Equipado: {item.itemName}");
        }
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
            currentWeaponData = null;
        }
    }

    public ItemData GetCurrentWeapon()
    {
        return currentWeaponData;
    }
}