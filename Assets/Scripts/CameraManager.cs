using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
    [SerializeField] private CinemachineVirtualCamera firstPersonCamera;
    [SerializeField] private CinemachineVirtualCamera danceCamera;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.5f;

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
    }

    private void Start()
    {
        SwitchToThirdPerson();
    }

    public void SwitchToThirdPerson()
    {
        SetCameraPriorities(10, 0, 0);
    }

    public void SwitchToFirstPerson()
    {
        SetCameraPriorities(0, 10, 0);
    }

    public void SwitchToDanceCamera()
    {
        SetCameraPriorities(0, 0, 10);
    }

    private void SetCameraPriorities(int third, int first, int dance)
    {
        if (thirdPersonCamera != null)
            thirdPersonCamera.Priority = third;
        if (firstPersonCamera != null)
            firstPersonCamera.Priority = first;
        if (danceCamera != null)
            danceCamera.Priority = dance;
    }
}