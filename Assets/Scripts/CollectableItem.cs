using UnityEngine;

/// <summary>
/// Place on any pickup in the scene.
/// On interact / trigger enter the item is added to the inventory
/// and the GameObject is hidden (so it can be restored on load).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectableItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;

    [Header("Visuals / FX")]
    [SerializeField] private float   bobAmplitude  = 0.15f;
    [SerializeField] private float   bobFrequency  = 1.5f;
    [SerializeField] private float   rotateSpeed   = 60f;
    [SerializeField] private ParticleSystem collectEffect;

    // ─── Unique id used by the save system ──────────────────────────────────
    [Header("Save")]
    [SerializeField] private string itemId = "";   // set a unique string in Inspector

    private Vector3 startPos;
    private bool    collected;

    public bool   IsCollected => collected;
    public string ItemId      => string.IsNullOrEmpty(itemId) ? name : itemId;

    // ════════════════════════════════════════════════════════════════════════
    private void Start()
    {
        startPos = transform.position;

        // Make sure collider is trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (collected) return;

        // Bob and spin
        float newY = startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position    = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    // ─── Pick up by walking into it ──────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null) Collect();
    }

    // ─── Pick up by interaction (E key) ─────────────────────────────────────
    public void Interact(PlayerController player)
    {
        if (!collected) Collect();
    }

    // ════════════════════════════════════════════════════════════════════════
    private void Collect()
    {
        collected = true;

        InventoryManager.Instance?.AddItem(itemData);

        // Play effect before hiding
        if (collectEffect != null)
        {
            collectEffect.transform.parent = null;   // detach so it survives
            collectEffect.Play();
            Destroy(collectEffect.gameObject, collectEffect.main.duration + 1f);
        }

        // Check win condition
        if (itemData != null && itemData.itemType == ItemType.QuestItem)
            CheckWinCondition();

        gameObject.SetActive(false);
    }

    /// <summary>Called by SaveSystem to restore collected state without side effects.</summary>
    public void SetCollectedState(bool state)
    {
        collected = state;
        gameObject.SetActive(!state);
    }

    private void CheckWinCondition()
    {
        // Example: if player has all quest items, open door / trigger win
        // Extend this logic per your game's design.
        GameManager.Instance?.NotifyDoorOpened();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generate an id from the object name if empty
        if (string.IsNullOrEmpty(itemId))
            itemId = name;
    }
#endif
}
