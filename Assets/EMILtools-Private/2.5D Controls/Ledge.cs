using System;
using EMILtools.Extensions;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    [Serializable]
    public struct LedgeData
    {
        public TwoDimensionalController.LookDir dir;
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
        var player = other.Get<TwoDimensionalController>();
        player.CanMantleLedge(data);
    }
}
