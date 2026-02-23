using UnityEngine;

/// <summary>
/// Manages third-person, first-person (aim) and dance camera modes.
/// Attach to a dedicated CameraManager GameObject.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    // ─── References ─────────────────────────────────────────────────────────
    [Header("Target")]
    [SerializeField] private Transform playerRoot;          // player transform
    [SerializeField] private Transform cameraTarget;        // empty child at shoulder height

    [Header("Third-person")]
    [SerializeField] private float tpDistance    = 4f;
    [SerializeField] private float tpHeight      = 1.5f;
    [SerializeField] private float tpMinPitch    = -20f;
    [SerializeField] private float tpMaxPitch    =  60f;

    [Header("First-person / Aim")]
    [SerializeField] private float fpDistance    = 0.3f;   // slight offset forward of head
    [SerializeField] private float fpHeight      = 1.7f;   // eye height

    [Header("Dance camera")]
    [SerializeField] private float danceDistance = 3.5f;
    [SerializeField] private float danceHeight   = 1.2f;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivity   = 0.12f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float     collisionRadius = 0.2f;

    [Header("Smooth")]
    [SerializeField] private float positionSmooth = 10f;
    [SerializeField] private float rotationSmooth = 10f;

    // ─── State ───────────────────────────────────────────────────────────────
    private Camera  cam;
    private float   yaw;
    private float   pitch;
    private bool    isAiming;
    private bool    isDancing;

    // ════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        cam = GetComponentInChildren<Camera>();

        if (playerRoot == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerRoot = player.transform;
        }

        // Initialise yaw from player facing
        if (playerRoot != null)
            yaw = playerRoot.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void LateUpdate()
    {
        if (playerRoot == null || cam == null) return;

        UpdateCameraPosition();
    }

    // ════════════════════════════════════════════════════════════════════════
    #region Public API
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Called every frame by PlayerController with mouse/stick delta.</summary>
    public void HandleRotation(Vector2 lookDelta)
    {
        if (isDancing) return;      // camera locked frontally during dance

        yaw   += lookDelta.x * sensitivity;
        pitch -= lookDelta.y * sensitivity;
        pitch  = Mathf.Clamp(pitch, tpMinPitch, tpMaxPitch);
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void SetDancing(bool dancing)
    {
        isDancing = dancing;

        if (dancing)
        {
            // Snap camera in front of player
            yaw   = playerRoot.eulerAngles.y + 180f;
            pitch = 10f;
        }
    }

    public bool IsShoulderCameraActive() => isAiming;

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region Position update
    // ════════════════════════════════════════════════════════════════════════

    private void UpdateCameraPosition()
    {
        Vector3 targetPos;
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);

        if (isDancing)
        {
            // Front-facing fixed view
            Vector3 danceOffset = Quaternion.Euler(5f, yaw, 0f) * new Vector3(0, danceHeight, danceDistance);
            targetPos = playerRoot.position + danceOffset;
        }
        else if (isAiming)
        {
            // First-person / shoulder
            Vector3 eyePos = playerRoot.position + Vector3.up * fpHeight;
            Vector3 fpOffset = targetRot * new Vector3(0.3f, 0, fpDistance);
            targetPos = eyePos + fpOffset;
        }
        else
        {
            // Third-person
            Vector3 offset = targetRot * new Vector3(0, 0, -tpDistance);
            targetPos = playerRoot.position + Vector3.up * tpHeight + offset;
        }

        // ─── Collision check ────────────────────────────────────────────────
        if (!isAiming)
        {
            Vector3 origin    = playerRoot.position + Vector3.up * tpHeight;
            Vector3 direction = (targetPos - origin).normalized;
            float   maxDist   = Vector3.Distance(origin, targetPos);

            if (Physics.SphereCast(origin, collisionRadius, direction, out RaycastHit hit,
                                   maxDist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                targetPos = origin + direction * (hit.distance - collisionRadius);
            }
        }

        // ─── Smooth move ────────────────────────────────────────────────────
        cam.transform.position = Vector3.Lerp(
            cam.transform.position, targetPos, positionSmooth * Time.deltaTime);

        cam.transform.rotation = Quaternion.Slerp(
            cam.transform.rotation, targetRot, rotationSmooth * Time.deltaTime);

        // Rotate player yaw to match camera when moving (handled in PlayerController)
        // but we expose yaw so the player can read it
    }

    #endregion
}
