using UnityEngine;

public class RotatingObjectBehaviour : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float normalRotationSpeed = 45f;
    [SerializeField] private float burstRotationSpeed = 720f;
    
    [Header("Deceleration")]
    [SerializeField] private float decelerationTime = 2f;
    
    private float _currentRotationSpeed;
    private float _decelerationTimer = 0f;
    private bool _isDecelerating = false;

    private void Start()
    {
        _currentRotationSpeed = normalRotationSpeed;
    }

    private void Update()
    {
        if (_isDecelerating)
        {
            _decelerationTimer += Time.deltaTime;
            float t = _decelerationTimer / decelerationTime;
            
            _currentRotationSpeed = Mathf.Lerp(burstRotationSpeed, normalRotationSpeed, t);
            
            if (t >= 1f)
            {
                _isDecelerating = false;
                _currentRotationSpeed = normalRotationSpeed;
            }
        }
        
        transform.Rotate(Vector3.up, _currentRotationSpeed * Time.deltaTime, Space.Self);
    }

    public void TriggerBurst()
    {
        _currentRotationSpeed = burstRotationSpeed;
        
        _isDecelerating = true;
        _decelerationTimer = 0f;
    }
}