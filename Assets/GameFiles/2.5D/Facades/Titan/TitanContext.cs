using System;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;

public struct TitanContext : IModuleUsabableContext
{ 
    [field: SerializeField] public ProjectileSpawnManager bulletSpawner { get; private set; }
    [ReadOnly, ShowInInspector] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    [BoxGroup("Timers")] [field: SerializeField] public DecayTimer moveDecay { get; set; }
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer turnSlowdown { get; set; }
    [ReadOnly] public bool canMount;
    [ReadOnly] public bool hasMounted;
    [field: SerializeField] [field:ReadOnly] public TwoD_InputAuthority.CameraContext camContext { get; private set; }
    [field: NonSerialized] public TwoD_PilotController myPilot { get; set; }
}