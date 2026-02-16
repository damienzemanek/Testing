using System;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static TwoD_InputAuthority;

[Serializable]
public class TitanBlackboard : Blackboard
{
    [field: BoxGroup("References")] [field: SerializeField] public Rigidbody rb { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Transform facing { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public CapsuleCollider capsuleCollider { get; private set; }
    //[field: BoxGroup("References")] [field: SerializeField] public WeaponManager weapons { get; private set; }
    //[field: BoxGroup("References")] [field: SerializeField] public ProjectileSpawnManager bulletSpawner { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public TitanAnims anims { get; private set; }
    //[field: BoxGroup("References")] [field: SerializeField] public TurnSlowDown turnSlowDown { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public AugmentPhysEX phys { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public Transform mountLocation { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public MountZone myMountZone { get; private set; }


    [BoxGroup("Orientation")] [field: SerializeField] public MouseLookEX.RotateToMouseWorldSpace mouseLook { get; private set; }
    [field: SerializeField] public MouseLookEX.PositionToMouseWorldSpace posToMouse { get; private set; }

    [BoxGroup("Timers")] [field: SerializeField] public DecayTimer moveDecay { get; set; }
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer turnSlowdown { get; set; }
    
    [BoxGroup("ReadOnly")] [ReadOnly] public PilotConfig.LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly] public PilotConfig.LookDir moveDir;
    
    //[BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool canMount = false;
    //[BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isShooting;
    //[BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> isRunning = new ReactiveIntercept<bool>(false);
    
    // Dynamic Variables
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider != null ? capsuleCollider.height : 0;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    
    [BoxGroup("ReadOnly")] [ReadOnly] public bool hasMounted = false;
    [field: BoxGroup("ReadOnly")][field: SerializeField] [field:ReadOnly] public CameraContext camContext { get; private set; }
    [field: BoxGroup("ReadOnly")] [field: NonSerialized] public TwoD_PilotController myPilot { get; set; }
    
    
    //[BoxGroup("Guards")] [ShowInInspector, ReadOnly] public SimpleGuarderImmutable shootGuarder;
    //[BoxGroup("Guards")] [ShowInInspector, ReadOnly] public ActionGuarderImmutable cantJumpGuarder;
    [BoxGroup("Guards")] [ShowInInspector, ReadOnly] public LazyGuarderMutable mouseZoneGuarder;
    
    
    
    
}
