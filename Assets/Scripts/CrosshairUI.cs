using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite aimCrosshair;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;

    private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();

        if (crosshairImage != null)
        {
            crosshairImage.enabled = false;
        }
    }

    private void Update()
    {
        if (crosshairImage == null) return;

        // Mostrar solo al apuntar
        bool isAiming = CameraManager.Instance != null && CameraManager.Instance.IsShoulderCameraActive();
        crosshairImage.enabled = isAiming;

        if (isAiming)
        {
            UpdateCrosshair();
        }
    }

    private void UpdateCrosshair()
    {
        // Cambiar sprite
        if (aimCrosshair != null)
        {
            crosshairImage.sprite = aimCrosshair;
        }

        // Cambiar color si apunta a un enemigo
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                crosshairImage.color = enemyColor;
            }
            else
            {
                crosshairImage.color = normalColor;
            }
        }
        else
        {
            crosshairImage.color = normalColor;
        }
    }
}