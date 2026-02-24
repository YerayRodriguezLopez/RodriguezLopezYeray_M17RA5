using System;
using UnityEngine;

public class CameraTriggerBehaviour : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            cameraTransform.gameObject.SetActive(true);

            PlayerController player = other.GetComponent<PlayerController>();
            PlayerAnimatorBehaviour playerAnimatorBehaviour = player.GetComponent<PlayerAnimatorBehaviour>();

            player.DisableControls();
            playerAnimatorBehaviour.TriggerDance();
        }
    }
}
