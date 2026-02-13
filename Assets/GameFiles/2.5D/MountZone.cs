using System;
using EMILtools.Extensions;
using UnityEngine;

public class MountZone : MonoBehaviour
{
    public bool inZone;
    public bool playerRequestedMount => playerTransform != null && playerTransform.Get<TwoDimensionalController>().requestedMount;
    public Transform playerTransform;
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.Get<TwoDimensionalController>().canMount = true;
        inZone = true;
        playerTransform = other.transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.Get<TwoDimensionalController>().canMount = false;
        inZone = false;
        playerTransform = null;
    }
}
