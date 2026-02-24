using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementBehaviour))]
[RequireComponent(typeof(PlayerAnimatorBehaviour))]
[RequireComponent(typeof(SaveLoadBehaviour))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [Header("Weapon")]
    [SerializeField] private GameObject weaponGear; // Child object to enable when unlocked, and hidden while aiming
    [SerializeField] private GameObject weaponHand; // Child object to show when aiming


    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;



    private InputSystem_Actions _input;
    private PlayerMovementBehaviour movementBehaviour;
    private PlayerAnimatorBehaviour animatorBehaviour;
    private SaveLoadBehaviour _saveLoadBehaviour;
    private CameraController _camera;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isAiming;
    private bool _hasWeapon = true;

    private Camera _cam;
    public bool HasWeapon => _hasWeapon;
    private bool _camFound = false;

    private void Awake()
    {
        _input = new InputSystem_Actions();
        _input.Player.SetCallbacks(this);

        movementBehaviour = GetComponent<PlayerMovementBehaviour>();
        animatorBehaviour = GetComponent<PlayerAnimatorBehaviour>();
        _saveLoadBehaviour = GetComponent<SaveLoadBehaviour>();
        _camera = FindFirstObjectByType<CameraController>();

        if (!_camera) Debug.Log("_camera is null");
        else _camFound = true;

        _cam = Camera.main;

        //if (_saveLoadBehaviour)
        //{
        //    _hasWeapon = _saveLoadBehaviour.LoadPlayerData();
        //}
    }

    private void Start()
    {
        // Make sure weapon is hidden at start
        //if ((weaponGear && weaponHand) && !_hasWeapon)
        //{
        //    weaponGear.SetActive(_hasWeapon);
        //    weaponHand.SetActive(_hasWeapon);
        //}
        //else
        //{
            weaponGear.SetActive(_hasWeapon);
            weaponHand.SetActive(!_hasWeapon);
        //}
    }

    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        _camera?.Rotate(_lookInput);

        animatorBehaviour.SetGrounded(movementBehaviour.IsGrounded);

        if (_camFound)
        {
            float mouseX = _camera!.MouseXInput;
            animatorBehaviour?.UpdateAnimator(_moveInput, mouseX, _isAiming);
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = CalculateMovementDirection();
        float speed = movementBehaviour.GetCurrentSpeed();
        movementBehaviour.Move(direction, speed);
    }

    /// <summary>
    /// Uses W/S for the Y axis and AD for the X axis movement.
    /// Projects it on the horizontal plane, and then processes the direction. 
    /// </summary>
    /// <returns>The direction combined from the inputs, and 0 if too small or the camera isn't found (null)</returns>
    private Vector3 CalculateMovementDirection()
    {
        Vector3 direction;

        if (_moveInput.sqrMagnitude < 0.01f || !_camFound) direction = Vector3.zero;

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;

        Vector3 forward = new Vector3(camForward.x, 0f, camForward.z).normalized;
        Vector3 right = new Vector3(camRight.x, 0f, camRight.z).normalized;

        // Fall back to using the transform's forward if the camera is vertical
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = transform.forward;
        }

        direction = forward * _moveInput.y + right * _moveInput.x;
        return direction;
    }

    public void UnlockWeapon()
    {
        if (!_hasWeapon)
        {
            _hasWeapon = true;

            if (weaponGear)
            {
                weaponGear.SetActive(true);
            }
            else
            {
                Debug.LogError("PlayerController: No weapon object assigned!");
            }
        }
    }
    // Input callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
        //Debug.Log($"Look input: {_lookInput}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) movementBehaviour.Jump();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed) movementBehaviour.SetCrouch(!movementBehaviour.IsCrouching);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        movementBehaviour.SetSprint(context.ReadValueAsButton());
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        Debug.Log($"HasWeapon = {_hasWeapon}");
        if (_hasWeapon)
        {
            _isAiming = context.ReadValueAsButton();
            ToggleCameraAim();
            //_camera?.SetAiming(_isAiming);
            Debug.Log($"Aiming: {_isAiming}");
            weaponHand.SetActive(_isAiming);
            weaponGear.SetActive(!_isAiming);
        }
    }

    private void ToggleCameraAim()
    {
        _camera?.SetAiming(_isAiming);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed && _isAiming && _hasWeapon)
        {
            animatorBehaviour?.TriggerShoot();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryInteract();
        }
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        Application.Quit();
    }

    private void TryInteract()
    {
        if (_cam)
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            // Debug.DrawRay(ray.origin, ray.direction, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.OnInteract(this);
            }
        }
    }

    public void DisableControls()
    {
        _input.Player.Disable();
    }
}