using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovementBehaviour))]
public class PlayerAnimatorBehaviour : MonoBehaviour
{
    private const float _half = 0.5f;
    
    private Animator _animator;
    private PlayerMovementBehaviour movementBehaviour;
    
    [SerializeField] private float runCycleLegOffset = 0.2f;
    private bool _isGrounded = true;
    
    // Hash IDs for performance
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Turn = Animator.StringToHash("Turn");
    private static readonly int Crouch = Animator.StringToHash("Crouch");
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");
    private static readonly int IsAiming = Animator.StringToHash("IsAiming");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Dance = Animator.StringToHash("Dance");

    private void Start()
    {
        _animator = GetComponent<Animator>();
        movementBehaviour = GetComponent<PlayerMovementBehaviour>();
    }

    public void UpdateAnimator(Vector2 moveInput, float mouseX, bool isAiming)
    {
        // Forward: -1 (back), 0 (idle), 0.5 (walk), 1 (run)
        float forward = 0f;
        if (Mathf.Abs(moveInput.y) > 0.01f)
        {
            if (moveInput.y < 0)
                forward = -0.5f;
            else
                forward = movementBehaviour.IsSprinting ? 1f : 0.5f;
        }
        
        // Horizontal: -1 (left), 0 (none), 1 (right)
        float horizontal = 0f;
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            horizontal = Mathf.Sign(moveInput.x);
        }
        
        float turn = mouseX;

        float jumpValue = movementBehaviour.IsGrounded
            ? 0f
            : Mathf.Clamp(movementBehaviour.VerticalVelocity, -9f, 9f);
        
        // Set all parameters
        _animator.SetFloat(Forward, forward);
        _animator.SetFloat(Horizontal, horizontal);
        _animator.SetFloat(Turn, turn);
        _animator.SetBool(Crouch, movementBehaviour.IsCrouching);
        _animator.SetBool(OnGround, movementBehaviour.IsGrounded);
        _animator.SetFloat(Jump, jumpValue);
        _animator.SetBool(IsAiming, isAiming);
        
        float forwardAmount = Mathf.Abs(forward);
        float runCycle =
            Mathf.Repeat(
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
        float jumpLeg = (runCycle < _half ? 1 : -1) * forwardAmount;
        if (_isGrounded)
        {
            _animator.SetFloat(JumpLeg, jumpLeg);
        }
    }

    public void TriggerShoot()
    {
        _animator.SetTrigger(Shoot);
    }

    public void TriggerDance()
    {
        _animator.SetTrigger(Dance);
    }

    public void SetGrounded(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }
}