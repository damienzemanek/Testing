using System;
using EMILtools.Core;
using UnityEngine;

public class MapBounds : MonoBehaviour
{
    public BoolEventChannel outOfBounds;
    public string targetTag;
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        outOfBounds.Invoke(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        outOfBounds.Invoke(false);
    }
}
