using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    const float VerticalMax = 3f;
    const float VerticalMin = -1f;
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera normalCam;
    [SerializeField] private CinemachineCamera aimCam;
    
    [Header("Settings")]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float aimSensitivity = 0.6f;
    [SerializeField] private float maxVerticalAngle = 70f;
    [SerializeField] private float turnSmoothingFactor = 5f;
    [SerializeField] private float turnDecaySpeed = 10f;
    
    private Transform _playerTransform;
    private float _horizontalRotation;
    private float _verticalRotation;
    private bool _isAiming;
    private float _currentMouseX;
    private float _targetMouseX;
    private CinemachineThirdPersonFollow _normalCamFollow;
    private float _verticalCameraMovement;

    public float VerticalAngle => _verticalRotation;
    public bool IsAiming => _isAiming;
    public float MouseXInput => _currentMouseX;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _normalCamFollow = normalCam.GetComponent<CinemachineThirdPersonFollow>();

        if (player)
        {
            _playerTransform = player.transform;
            _horizontalRotation = _playerTransform.eulerAngles.y;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Rotate(Vector2 input)
    {
        if (_playerTransform)
        {
            // Normalize mouse X for animation Turn parameter (-1 to 1 range)
            _targetMouseX = Mathf.Clamp(input.x / turnSmoothingFactor, -1f, 1f);

            float sens = _isAiming ? aimSensitivity : sensitivity;

            _horizontalRotation += input.x * sens;
            _verticalRotation -= input.y * sens;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -maxVerticalAngle, maxVerticalAngle);
            _verticalCameraMovement = Mathf.Lerp(_verticalCameraMovement, _verticalRotation * 0.05f, Time.deltaTime * turnSmoothingFactor);
            if (_verticalCameraMovement > VerticalMax) _verticalCameraMovement = VerticalMax;
            if (_verticalCameraMovement < VerticalMin) _verticalCameraMovement = VerticalMin;
            if (_normalCamFollow)
            {
                _normalCamFollow.ShoulderOffset.y = _verticalCameraMovement;
            }
        }
    }
    private void Update()
    {
        _currentMouseX = Mathf.Lerp(_currentMouseX, _targetMouseX, Time.deltaTime * turnDecaySpeed);
        
        // Avoids floating point drift
        if (Mathf.Abs(_currentMouseX) < 0.01f)
        {
            _currentMouseX = 0f;
        }
        
        if (_playerTransform)
        {
            _playerTransform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
        }
    }
    
    public void SetAiming(bool aiming)
    {
        _isAiming = aiming;
        if (normalCam) normalCam.Priority.Value = aiming ? 0 : 10;
        if (aimCam) aimCam.Priority.Value = aiming ? 10 : 0;
    }
}