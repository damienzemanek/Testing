using System;
using System.Collections;
using UnityEngine;

public class DisableAfter : MonoBehaviour
{
    [SerializeField] float time;
    private void Start() => StartCoroutine(Disable());
    
    IEnumerator Disable()
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
