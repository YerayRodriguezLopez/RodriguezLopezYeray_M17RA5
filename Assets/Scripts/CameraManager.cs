using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
    [SerializeField] private CinemachineVirtualCamera shoulderCamera; // CAMBIADO de firstPerson
    [SerializeField] private CinemachineVirtualCamera danceCamera;

    [Header("Camera Targets")]
    [SerializeField] private Transform thirdPersonTarget;
    [SerializeField] private Transform shoulderTarget;
    [SerializeField] private Transform danceTarget;

    [Header("Input Settings")]
    [SerializeField] private bool invertYAxis = false;
    [SerializeField] private float cameraSensitivity = 1f;

    private CinemachineVirtualCamera activeCamera;
    private CinemachinePOV shoulderPOV;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Obtener el componente POV de la cámara de hombro
        if (shoulderCamera != null)
        {
            shoulderPOV = shoulderCamera.GetCinemachineComponent<CinemachinePOV>();
        }
    }

    private void Start()
    {
        SwitchToThirdPerson();
    }

    public void SwitchToThirdPerson()
    {
        SetCameraPriorities(10, 0, 0);
        activeCamera = thirdPersonCamera;

        Debug.Log("Cámara: Tercera Persona");
    }

    public void SwitchToShoulder()
    {
        SetCameraPriorities(0, 10, 0);
        activeCamera = shoulderCamera;

        // Reset POV al cambiar
        if (shoulderPOV != null)
        {
            shoulderPOV.m_VerticalAxis.Value = 0;
        }

        Debug.Log("Cámara: Hombro (Apuntando)");
    }

    public void SwitchToDanceCamera()
    {
        SetCameraPriorities(0, 0, 10);
        activeCamera = danceCamera;

        Debug.Log("Cámara: Baile");
    }

    private void SetCameraPriorities(int third, int shoulder, int dance)
    {
        if (thirdPersonCamera != null)
            thirdPersonCamera.Priority = third;
        if (shoulderCamera != null)
            shoulderCamera.Priority = shoulder;
        if (danceCamera != null)
            danceCamera.Priority = dance;
    }

    // Llamado por PlayerController para controlar la cámara POV
    public void UpdateShoulderCamera(Vector2 lookInput)
    {
        if (shoulderPOV == null || activeCamera != shoulderCamera) return;

        // Aplicar input del ratón/stick
        float verticalInput = invertYAxis ? lookInput.y : -lookInput.y;

        shoulderPOV.m_VerticalAxis.Value += verticalInput * cameraSensitivity;
        shoulderPOV.m_HorizontalAxis.Value += lookInput.x * cameraSensitivity;
    }

    public Transform GetActiveTarget()
    {
        if (activeCamera == thirdPersonCamera)
            return thirdPersonTarget;
        else if (activeCamera == shoulderCamera)
            return shoulderTarget;
        else if (activeCamera == danceCamera)
            return danceTarget;

        return thirdPersonTarget;
    }

    public bool IsShoulderCameraActive()
    {
        return activeCamera == shoulderCamera;
    }

    public Vector3 GetCameraForward()
    {
        if (Camera.main != null)
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0; // Mantener en plano horizontal
            return forward.normalized;
        }
        return Vector3.forward;
    }

    public void SetCameraSensitivity(float sensitivity)
    {
        cameraSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
    }

    public void SetInvertY(bool invert)
    {
        invertYAxis = invert;
    }
}