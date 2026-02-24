using System;
using UnityEngine;
using System.Collections;

public class SelfDisableTimerBehaviour : MonoBehaviour
{
    [SerializeField] private float timer = 5f;

    private void OnEnable()
    {
        StartCoroutine(StartDisableCountdown());
    }

    private IEnumerator StartDisableCountdown()
    {
        yield return new WaitForSeconds(timer);
        this.gameObject.SetActive(false);
    }
}
