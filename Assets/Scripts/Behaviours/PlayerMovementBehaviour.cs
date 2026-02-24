using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Ground Check")]
    [SerializeField] private float sphereRadius = 0.18f; //same as CharacterController's radius
    [SerializeField] private float groundCheckDistance = 2.1f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.95f, 0);
    [SerializeField] private LayerMask groundLayer;
    
    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isSprinting;
    private bool _isJumping;
    
    public bool IsGrounded => _isGrounded;
    public bool IsCrouching => _isCrouching;
    public bool IsSprinting => _isSprinting;
    public bool IsJumping => _isJumping; 
    public float VerticalVelocity => _velocity.y;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    public void Move(Vector3 direction, float speed)
    {
        CheckGround();
        
        Vector3 targetHorizontal = direction * speed;
        _velocity.x = targetHorizontal.x;
        _velocity.z = targetHorizontal.z;
        
        if (_isGrounded)
        { 
            if (_velocity.y < 0) _velocity.y = -2f;
        }
    
        _velocity.y += gravity * Time.deltaTime;
    
        _controller.Move(_velocity * Time.deltaTime);

        if (_isGrounded && _isJumping)
            _isJumping = false;
    }

    public void Jump()
    {
        if (_isGrounded && !_isCrouching)
        {
            _velocity.y = jumpForce;
            _isJumping = true;
        }
    }

    public void SetCrouch(bool crouch)
    {
        _isCrouching = crouch;
        if (crouch) _isSprinting = false;
    }

    public void SetSprint(bool sprint)
    {
        if (!_isCrouching) _isSprinting = sprint;
    }

    public float GetCurrentSpeed()
    {
        float speed;
        if (_isCrouching) speed = crouchSpeed;
        else if (_isSprinting) speed = runSpeed;
        else speed = walkSpeed;
        
        return speed;
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + offset;
    
        bool hitGround = Physics.SphereCast(
            origin,
            sphereRadius,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    
        // Only grounded if hit AND moving downward (prevents early ground detection on jump)
        _isGrounded = hitGround && _velocity.y <= 0.5f;
    
        Color debugColor = _isGrounded ? Color.green : Color.red;
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, debugColor);
    
        // Radius debug
        Debug.DrawLine(origin + Vector3.right * sphereRadius, origin + Vector3.right * sphereRadius + Vector3.down * groundCheckDistance, debugColor);
        Debug.DrawLine(origin - Vector3.right * sphereRadius, origin - Vector3.right * sphereRadius + Vector3.down * groundCheckDistance, debugColor);
        Debug.DrawLine(origin + Vector3.forward * sphereRadius, origin + Vector3.forward * sphereRadius + Vector3.down * groundCheckDistance, debugColor);
        Debug.DrawLine(origin - Vector3.forward * sphereRadius, origin - Vector3.forward * sphereRadius + Vector3.down * groundCheckDistance, debugColor);
    }
}