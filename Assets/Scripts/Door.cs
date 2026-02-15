using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private ItemData requiredKey;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 targetPosition;

    private void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;

        closedPosition = doorTransform.localPosition;
        targetPosition = closedPosition;
    }

    private void Update()
    {
        if (isMoving)
        {
            doorTransform.localPosition = Vector3.Lerp(
                doorTransform.localPosition,
                targetPosition,
                Time.deltaTime * openSpeed
            );

            if (Vector3.Distance(doorTransform.localPosition, targetPosition) < 0.01f)
            {
                doorTransform.localPosition = targetPosition;
                isMoving = false;
            }
        }
    }

    public void Interact(PlayerController player)
    {
        if (isMoving) return;

        if (requiresKey && !isOpen)
        {
            if (InventoryManager.Instance.HasItem(requiredKey))
            {
                OpenDoor();
            }
            else
            {
                // Puerta bloqueada
                AudioManager.Instance?.PlaySFX(lockedSound);
                UIManager.Instance?.ShowMessage("Necesitas una llave para abrir esta puerta");
            }
        }
        else
        {
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        isMoving = true;
        targetPosition = openPosition;
        AudioManager.Instance?.PlaySFX(openSound);
    }

    private void CloseDoor()
    {
        isOpen = false;
        isMoving = true;
        targetPosition = closedPosition;
        AudioManager.Instance?.PlaySFX(closeSound);
    }
}