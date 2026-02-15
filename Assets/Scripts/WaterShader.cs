using UnityEngine;

public class WaterShader : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveScale = 0.1f;

    [Header("Material")]
    [SerializeField] private Material waterMaterial;

    private void Update()
    {
        if (waterMaterial != null)
        {
            // Animar las propiedades del shader
            float offset = Time.time * waveSpeed;
            waterMaterial.SetFloat("_WaveOffset", offset);
            waterMaterial.SetFloat("_WaveScale", waveScale);
        }
    }
}