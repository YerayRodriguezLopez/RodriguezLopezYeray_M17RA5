using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform cameraFollowTarget;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    // Components
    private CharacterController controller;
    private PlayerInput playerInput;
    private Animator animator;

    // Input values
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isCrouching;
    private bool isAiming;

    // State
    private Vector3 velocity;
    private float verticalRotation;
    private bool isGrounded;
    private bool isDancing;
    private bool isDead;

    // Animation hashes
    private int moveXHash;
    private int moveZHash;
    private int isRunningHash;
    private int isCrouchingHash;
    private int isAimingHash;
    private int jumpHash;
    private int isDancingHash;
    private int dieHash;
    private int shootHash;

    private Vector2 rawLookInput;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        // Cache animator hashes
        moveXHash = Animator.StringToHash("MoveX");
        moveZHash = Animator.StringToHash("MoveZ");
        isRunningHash = Animator.StringToHash("IsRunning");
        isCrouchingHash = Animator.StringToHash("IsCrouching");
        isAimingHash = Animator.StringToHash("IsAiming");
        jumpHash = Animator.StringToHash("Jump");
        isDancingHash = Animator.StringToHash("IsDancing");
        dieHash = Animator.StringToHash("Die");
        shootHash = Animator.StringToHash("Shoot");

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isDead || isDancing) return;

        CheckGrounded();
        HandleMovement();
        HandleRotation();
        ApplyGravity();
    }

    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float currentSpeed = walkSpeed;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isRunning && !isAiming)
            currentSpeed = runSpeed;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 move = (forward * moveInput.y + right * moveInput.x) * currentSpeed;
        controller.Move(move * Time.deltaTime);

        // Update animator
        float animMoveX = moveInput.x;
        float animMoveZ = moveInput.y;

        if (isAiming)
        {
            // En modo apuntar, movimiento relativo a la cámara
            animMoveX = moveInput.x;
            animMoveZ = moveInput.y;
        }

        animator.SetFloat(moveXHash, animMoveX, 0.1f, Time.deltaTime);
        animator.SetFloat(moveZHash, animMoveZ, 0.1f, Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (isAiming)
        {
            // En modo apuntar (cámara de hombro):
            // El jugador rota en el eje Y con la cámara
            float horizontalRotation = rawLookInput.x * mouseSensitivity;
            transform.Rotate(Vector3.up * horizontalRotation);

            // Actualizar la cámara POV
            CameraManager.Instance?.UpdateShoulderCamera(rawLookInput);
        }
        else
        {
            // En tercera persona normal:
            // Rotar el jugador con input horizontal
            float horizontalRotation = lookInput.x * mouseSensitivity;
            transform.Rotate(Vector3.up * horizontalRotation);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // INPUT CALLBACKS (llamadas por el nuevo Input System)

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        rawLookInput = context.ReadValue<Vector2>();

        // Aplicar sensibilidad solo para rotación del jugador
        lookInput = rawLookInput * mouseSensitivity * 0.1f;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && !isCrouching && !isDancing)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(jumpHash);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
            isRunning = true;
        else if (context.canceled)
            isRunning = false;

        animator.SetBool(isRunningHash, isRunning);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed && !isDancing)
        {
            isCrouching = !isCrouching;
            animator.SetBool(isCrouchingHash, isCrouching);

            // Ajustar altura del collider
            if (isCrouching)
            {
                controller.height = 1f;
                controller.center = new Vector3(0, 0.5f, 0);
            }
            else
            {
                controller.height = 2f;
                controller.center = new Vector3(0, 1f, 0);
            }
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAiming = true;
            CameraManager.Instance?.SwitchToShoulder(); // CAMBIADO
        }
        else if (context.canceled)
        {
            isAiming = false;
            CameraManager.Instance?.SwitchToThirdPerson();
        }

        animator.SetBool(isAimingHash, isAiming);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed && isAiming && !isDancing)
        {
            Shoot();
        }
    }

    public void OnDance(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && !isDead)
        {
            StartDance();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Detectar objetos interactuables
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
            foreach (var hit in hits)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                }
            }
        }
    }

    private void Shoot()
    {
        animator.SetTrigger(shootHash);

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);

            // Calcular dirección de disparo basada en la cámara
            Vector3 shootDirection;

            if (CameraManager.Instance != null && Camera.main != null)
            {
                // Raycast desde el centro de la pantalla
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    // Apuntar al punto de impacto
                    shootDirection = (hit.point - arrowSpawnPoint.position).normalized;
                }
                else
                {
                    // Apuntar hacia adelante de la cámara
                    shootDirection = ray.direction;
                }
            }
            else
            {
                // Fallback: dirección forward del spawn point
                shootDirection = arrowSpawnPoint.forward;
            }

            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.Shoot(shootDirection);
            }
        }

        // Efecto de sonido
        AudioManager.Instance?.PlaySFX("BowShoot");
    }

    private void StartDance()
    {
        isDancing = true;
        animator.SetBool(isDancingHash, true);

        // Mover cámara al frente
        CameraManager.Instance?.SwitchToDanceCamera();

        // Detener después de la animación
        Invoke(nameof(StopDance), 5f); // Ajustar según duración de animación
    }

    private void StopDance()
    {
        isDancing = false;
        animator.SetBool(isDancingHash, false);
        CameraManager.Instance?.SwitchToThirdPerson();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger(dieHash);

        // Desactivar controles
        playerInput.enabled = false;

        // Mostrar Game Over
        UIManager.Instance?.ShowGameOver();
    }

    public void TakeDamage(float damage)
    {
        // Implementar sistema de vida aquí
        Die();
    }
}