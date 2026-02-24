using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class DoorBehaviour : MonoBehaviour, IInteractable
{
    private const string MessageWeapon = "You need a weapon to open this door!";
    
    [SerializeField] private bool requiresWeapon = true;
    [SerializeField] private TextMeshProUGUI interactionMessage;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private float closeDelay = 3f;

    private bool _textShown=false;

    private void Start()
    {
        interactionMessage.text = MessageWeapon;
    }

    public void OnInteract(PlayerController player)
    {
        if (requiresWeapon && !player.HasWeapon)
        {
            if(!_textShown) StartCoroutine(ShowMessage());
        }
        else OpenOrCloseDoor();
    }

    private void OpenOrCloseDoor()
    {
        if (doorAnimator)
        {
            doorAnimator.Play("DoorAnimation");

            StartCoroutine(CloseDoorWithDelay());
        }
    }

    private IEnumerator ShowMessage()
    {
        interactionMessage.maxVisibleCharacters = 0;
        interactionMessage.gameObject.SetActive(true);
        _textShown = true;
        
        foreach (char letter in interactionMessage.text)
        {
            interactionMessage.maxVisibleCharacters ++;
            yield return new WaitForSeconds(0.05f);
        }
        _textShown = false;
    }

    private IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        doorAnimator.Play("CloseDoorAnimation");
    }


}