using UnityEngine;

/// <summary>
/// Handles vertical aiming by rotating spine bone
/// </summary>
public class SpineAimingBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform spineBone;
    
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float minAngle = -30f;
    
    [Header("Rotation Axis")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.LocalY;
    [SerializeField] private bool invertRotation = true;
    
    private float _currentAngle;
    private Quaternion _initialRotation;
    
    public enum RotationAxis
    {
        LocalX,
        LocalY,
        LocalZ,
        WorldRight
    }

    private void Awake()
    {
        FindCameraController();
        FindSpineBone();
        CacheInitialRotation();
    }

    private void FindCameraController()
    {
        if (!cameraController)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
    }

    private void FindSpineBone()
    {
        if (!spineBone)
        {
            Animator animator = GetComponent<Animator>();
            if (animator)
            {
                spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);
            }
        }
    }

    private void CacheInitialRotation()
    {
        if (spineBone)
        {
            _initialRotation = spineBone.localRotation;
        }
    }

    private void LateUpdate()
    {
        if (spineBone && cameraController)
        {
            UpdateCurrentAngle();
            ApplyRotation();
        }
    }
    
    private void UpdateCurrentAngle()
    {
        float targetAngle = cameraController.IsAiming ? CalculateTargetAngle() : 0f;
        _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
    }

    private float CalculateTargetAngle()
    {
        float angle = cameraController.VerticalAngle;
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        
        if (!invertRotation)
        {
            angle = -angle;
        }
        
        return angle;
    }
    
    private void ApplyRotation()
    {
        spineBone.localRotation = _initialRotation;

        if (Mathf.Abs(_currentAngle) > 0.1f)
        {
            switch (rotationAxis)
            {
                case RotationAxis.LocalX:
                    spineBone.Rotate(_currentAngle, 0, 0, Space.Self);
                    break;

                case RotationAxis.LocalY:
                    spineBone.Rotate(0, _currentAngle, 0, Space.Self);
                    break;

                case RotationAxis.LocalZ:
                    spineBone.Rotate(0, 0, _currentAngle, Space.Self);
                    break;

                case RotationAxis.WorldRight:
                    spineBone.Rotate(transform.right, _currentAngle, Space.World);
                    break;
            }
        }
    }
}