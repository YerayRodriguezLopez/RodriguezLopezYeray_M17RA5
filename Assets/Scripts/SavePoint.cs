using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SavePoint : MonoBehaviour, IInteractable
{
    [Header("Visual")]
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Light saveLight;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Renderer savePointRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip saveSound;

    private bool isActive = true;

    private void Start()
    {
        if (particles == null)
            particles = GetComponent<ParticleSystem>();

        UpdateVisuals();
    }

    public void Interact(PlayerController player)
    {
        if (isActive)
        {
            SaveGame();
        }
    }

    private void SaveGame()
    {
        SaveSystem.Instance?.SaveGame();

        // Efectos visuales y sonoros
        if (particles != null)
        {
            particles.Play();
        }

        if (saveLight != null)
        {
            StartCoroutine(FlashLight());
        }

        AudioManager.Instance?.PlaySFX(saveSound);
    }

    private System.Collections.IEnumerator FlashLight()
    {
        float originalIntensity = saveLight.intensity;

        for (int i = 0; i < 3; i++)
        {
            saveLight.intensity = originalIntensity * 2f;
            yield return new WaitForSeconds(0.1f);
            saveLight.intensity = originalIntensity;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateVisuals()
    {
        if (savePointRenderer != null)
        {
            savePointRenderer.material = isActive ? activeMaterial : inactiveMaterial;
        }

        if (saveLight != null)
        {
            saveLight.enabled = isActive;
        }
    }
}