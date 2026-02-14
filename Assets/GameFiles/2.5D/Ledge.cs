using System;
using EMILtools.Extensions;
using UnityEngine;
using static TwoD_Config;

public class Ledge : MonoBehaviour
{
    [Serializable]
    public struct LedgeData
    {
        public LookDir dir;
        public Transform point;
    }

    public LedgeData data;
    
    private void OnTriggerEnter(Collider other)  => CheckForPlayer(other);
    private void OnTriggerStay(Collider other) => CheckForPlayer(other);

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.Get<TwoDimensionalController>();
        player.CantMantleLedge();
    }

    void CheckForPlayer(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.Get<TwoD_Controller>();
        player.
    }
    
    public void CanMantleLedge(LedgeData ledgeData)
    {
        canMantle.Value = true;
        this.ledgeData = ledgeData;
    }

    public void CantMantleLedge() => canMantle.Value = false;
}
