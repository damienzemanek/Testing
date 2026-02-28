using System;
using UnityEngine;

[Serializable]
public struct Disabler
{
    public Behaviour[] behaviours;
    public Collider[] colliders;

    public void DisableAll()
    {
        foreach (var b in behaviours) b.enabled = false;
        foreach (var c in colliders) c.enabled = false;
    }
}
