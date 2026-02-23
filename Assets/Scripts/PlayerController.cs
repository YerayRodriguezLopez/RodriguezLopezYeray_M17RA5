using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    // ─── Stats ──────────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float walkSpeed    = 3f;
    [SerializeField] private float runSpeed     = 6f;
    [SerializeField] private float crouchSpeed  = 1.5f;
    [SerializeField] private float jumpForce    = 5f;
    [SerializeField] private float gravity      = -20f;

    [Header("Shoot")]
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackRange  = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip footstepSound;

    // ─── Components ─────────────────────────────────────────────────────────
    private CharacterController cc;
    private Animator            anim;
    private PlayerInputActions  inputActions;

    // ─── State ──────────────────────────────────────────────────────────────
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector3 velocity;           // vertical velocity (gravity + jump)
    private bool    isGrounded;
    private bool    wasGrounded;

    private bool    isCrouching;
    private bool    isRunning;
    private bool    isAiming;
    private bool    isDancing;
    private bool    isDead;
    private bool    isPaused;

    // Jump phases
    private enum JumpPhase { None, Preparing, Rising, Falling, Landing }
    private JumpPhase jumpPhase = JumpPhase.None;
    private float     jumpApexY;

    // Footstep timer
    private float footstepTimer;
    private const float FootstepInterval = 0.4f;

    // ─── Animator hashes ────────────────────────────────────────────────────
    private static readonly int HashSpeed      = Animator.StringToHash("Speed");
    private static readonly int HashCrouch     = Animator.StringToHash("IsCrouching");
    private static readonly int HashAim        = Animator.StringToHash("IsAiming");
    private static readonly int HashJumpPrep   = Animator.StringToHash("JumpPrepare");
    private static readonly int HashJumpRise   = Animator.StringToHash("JumpRise");
    private static readonly int HashJumpFall   = Animator.StringToHash("JumpFall");
    private static readonly int HashLand       = Animator.StringToHash("Land");
    private static readonly int HashDance      = Animator.StringToHash("IsDancing");
    private static readonly int HashDeath      = Animator.StringToHash("Die");
    private static readonly int HashAttack     = Animator.StringToHash("Attack");
    private static readonly int HashShoot      = Animator.StringToHash("Shoot");

    // ═══════════════════════════════════════════════════════════════════════
    #region Unity lifecycle
    // ═══════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        cc   = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        inputActions = new PlayerInputActions();
        inputActions.Player.AddCallbacks(this);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.UI.Disable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void Update()
    {
        if (isDead) return;

        CheckGround();
        HandleGravityAndJumpPhases();
        HandleMovement();
        HandleCamera();
        HandleFootsteps();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Input callbacks  (PlayerInputActions.IPlayerActions)
    // ═══════════════════════════════════════════════════════════════════════

    public void OnMovement(InputAction.CallbackContext ctx)
    {
        if (isDead || isDancing) { moveInput = Vector2.zero; return; }
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isDead || isDancing || isCrouching) return;
        if (!isGrounded || jumpPhase != JumpPhase.None) return;

        StartJump();
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isDead || isDancing) return;

        isCrouching = !isCrouching;
        anim?.SetBool(HashCrouch, isCrouching);

        // Adjust capsule height
        cc.height  = isCrouching ? 1f : 2f;
        cc.center  = isCrouching ? new Vector3(0, 0.5f, 0) : new Vector3(0, 1f, 0);
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (isDead || isDancing) return;
        isRunning = ctx.ReadValueAsButton();
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (isDead || isDancing) return;

        isAiming = ctx.ReadValueAsButton();
        anim?.SetBool(HashAim, isAiming);

        CameraManager.Instance?.SetAiming(isAiming);
        Cursor.lockState = isAiming ? CursorLockMode.Locked : CursorLockMode.Locked;
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isDead || isDancing || !isAiming) return;

        ShootArrow();
        anim?.SetTrigger(HashShoot);
    }

    public void OnDance(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isDead) return;

        isDancing  = !isDancing;
        moveInput  = Vector2.zero;
        anim?.SetBool(HashDance, isDancing);

        CameraManager.Instance?.SetDancing(isDancing);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isDead || isDancing) return;

        // Sphere cast in front of player
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1.2f + Vector3.up * 0.8f, 1f);

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
                break;
            }
        }
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TogglePause();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Movement & Physics
    // ═══════════════════════════════════════════════════════════════════════

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded  = cc.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;   // small negative to keep grounded

        // Land trigger
        if (!wasGrounded && isGrounded && jumpPhase == JumpPhase.Falling)
        {
            jumpPhase = JumpPhase.Landing;
            anim?.SetTrigger(HashLand);
            AudioManager.Instance?.PlaySFX(landSound);
            Invoke(nameof(ClearLandPhase), 0.4f);
        }
    }

    private void ClearLandPhase() => jumpPhase = JumpPhase.None;

    private void HandleGravityAndJumpPhases()
    {
        // Gravity
        velocity.y += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

        // Rising -> Falling transition at apex
        if (jumpPhase == JumpPhase.Rising && velocity.y < 0)
        {
            jumpPhase = JumpPhase.Falling;
            anim?.SetBool(HashJumpFall, true);
            anim?.SetBool(HashJumpRise, false);
        }
    }

    private void HandleMovement()
    {
        if (isDancing) return;

        // Choose speed
        float speed = isCrouching ? crouchSpeed :
                      isRunning   ? runSpeed    : walkSpeed;

        // World-space direction relative to camera yaw
        Vector3 camForward = Camera.main != null
            ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized
            : transform.forward;
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized * -1f;

        Vector3 move = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        if (move.sqrMagnitude > 0.01f)
        {
            // Rotate player toward movement direction (except while aiming)
            if (!isAiming)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
            }

            cc.Move(move * speed * Time.deltaTime);
        }

        // Animator blend speed (0 = idle, 0.5 = walk, 1 = run)
        float animSpeed = moveInput.sqrMagnitude < 0.01f ? 0f :
                          isRunning ? 1f : 0.5f;
        if (isCrouching && animSpeed > 0) animSpeed = 0.25f;

        anim?.SetFloat(HashSpeed, animSpeed, 0.1f, Time.deltaTime);
    }

    private void HandleCamera()
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.HandleRotation(lookInput);
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || moveInput.sqrMagnitude < 0.01f) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            AudioManager.Instance?.PlaySFX(footstepSound);
            footstepTimer = isRunning ? FootstepInterval * 0.6f : FootstepInterval;
        }
    }

    private void StartJump()
    {
        jumpPhase  = JumpPhase.Preparing;
        anim?.SetTrigger(HashJumpPrep);
        AudioManager.Instance?.PlaySFX(jumpSound);
        Invoke(nameof(LaunchJump), 0.15f);  // small pre-jump delay for animation
    }

    private void LaunchJump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        jumpPhase  = JumpPhase.Rising;
        anim?.SetBool(HashJumpRise, true);
        anim?.SetBool(HashJumpFall, false);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Combat
    // ═══════════════════════════════════════════════════════════════════════

    public void Attack()
    {
        if (isDead || isDancing) return;
        anim?.SetTrigger(HashAttack);

        // Damage enemies in melee range
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1f + Vector3.up * 1f, attackRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                IDamageable dmg = hit.GetComponent<IDamageable>();
                dmg?.TakeDamage(attackDamage);
            }
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        // Direction: camera forward
        Vector3 dir = Camera.main != null
            ? Camera.main.transform.forward
            : transform.forward;

        GameObject arrowGO = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Arrow arrow = arrowGO.GetComponent<Arrow>();
        arrow?.Shoot(dir);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        anim?.SetTrigger(HashDeath);
        AudioManager.Instance?.PlaySFX(deathSound);
        cc.enabled = false;

        Invoke(nameof(ShowGameOver), 2f);
    }

    private void ShowGameOver()
    {
        UIManager.Instance?.ShowGameOver();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Pause / ESC menu
    // ═══════════════════════════════════════════════════════════════════════

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Switch to UI map so ESC closes the menu and player actions stop
            inputActions.Player.Disable();
            inputActions.UI.Enable();

            Time.timeScale       = 0f;
            Cursor.lockState     = CursorLockMode.None;
            Cursor.visible       = true;
            UIManager.Instance?.ShowPauseMenu(true);
        }
        else
        {
            inputActions.UI.Disable();
            inputActions.Player.Enable();

            Time.timeScale       = 1f;
            Cursor.lockState     = CursorLockMode.Locked;
            Cursor.visible       = false;
            UIManager.Instance?.ShowPauseMenu(false);
        }
    }

    /// <summary>Called by the UI map's Pause action (ESC while paused).</summary>
    public void OnUIPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) TogglePause();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Public helpers
    // ═══════════════════════════════════════════════════════════════════════

    public bool IsDead    => isDead;
    public bool IsPaused  => isPaused;
    public bool IsAiming  => isAiming;

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f + Vector3.up, attackRange);
    }
#endif
}
