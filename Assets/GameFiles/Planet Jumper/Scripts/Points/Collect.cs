using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class Collect : MonoBehaviour
{
    [Required] public IntEventChannel collectChannel;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.Has(out ScoreSource ss)) return;
        ss.Score++;
        collectChannel.Invoke(ss.Score);
        Destroy(gameObject);
    }
}