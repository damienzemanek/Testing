using System;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static EMILtools.Extensions.NumEX;
using static ITwoD_Blackboard;
using static TwoD_InputAuthority;

[Serializable]
public class TitanBlackboard : Blackboard, ITwoD_Blackboard
{
    [field: BoxGroup("References")] [field: SerializeField] public Rigidbody rb { get; private set; }
    [field: FormerlySerializedAs("<facing>k__BackingField")] [field: BoxGroup("References")] [field: SerializeField] public Transform facingTransformObject { get; set; }
    [field: BoxGroup("References")] [field: SerializeField] public CapsuleCollider capsuleCollider { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Animator animator { get; private set; }

    [field: BoxGroup("References")] [field: SerializeField] public WeaponManager weapons { get; private set; }
    //[field: BoxGroup("References")] [field: SerializeField] public TurnSlowDown turnSlowDown { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public AugmentPhysEX phys { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public Transform mountLocation { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public MountZone myMountZone { get; private set; }
    [field: SerializeField] public ProjectileSpawnManager bulletSpawner { get; private set; }
    [BoxGroup("Orientation")] [field: SerializeField] public MouseLookEX.RotateToMouseWorldSpace mouseLook { get; private set; }
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir facingDir { get; set; }
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir moveDir { get; set; }
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    // Dynamic Variables
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider != null ? capsuleCollider.height : 0;
    [BoxGroup("ReadOnly")] [ReadOnly] public bool canMount = false;
    [BoxGroup("ReadOnly")] [ReadOnly] public bool hasMounted = false;
    [BoxGroup("ReadOnly")] [ReadOnly] public bool isMountingOrDismounting = false;
    [field: SerializeField] [field:ReadOnly] public TwoD_InputAuthority.CameraContext camContext { get; private set; }
    [field: NonSerialized] public TwoD_PilotController myPilot { get; set; }
    
    [field: SerializeField] public DecayTimer moveDecay { get; set; }
    [field: SerializeField] public CountdownTimer turnSlowdown { get; set; }

    
    
    
}
