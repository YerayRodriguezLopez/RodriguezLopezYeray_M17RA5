using UnityEngine;

public class CollectableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Rotar el objeto
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Efecto de flotación
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void Interact(PlayerController player)
    {
        Collect(player);
    }

    private void Collect(PlayerController player)
    {
        // Agregar al inventario
        InventoryManager.Instance?.AddItem(itemData);

        // Efecto de partículas
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Sonido
        AudioManager.Instance?.PlaySFX("ItemCollect");

        // Si es equipable, equipar automáticamente
        if (itemData.isEquippable && itemData.equipmentModel != null)
        {
            EquipmentManager.Instance?.EquipItem(itemData);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Collect(player);
            }
        }
    }
}