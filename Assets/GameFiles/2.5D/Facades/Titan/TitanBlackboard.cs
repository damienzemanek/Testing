using System;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static ITwoD_Blackboard;
using static TwoD_InputAuthority;

[Serializable]
public class TitanBlackboard : Blackboard, ITwoD_Blackboard
{
    [field: BoxGroup("References")] [field: SerializeField] public Rigidbody rb { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Transform facing { get; set; }
    [field: BoxGroup("References")] [field: SerializeField] public CapsuleCollider capsuleCollider { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Animator animator { get; private set; }

    [field: BoxGroup("References")] [field: SerializeField] public WeaponManager weapons { get; private set; }
    //[field: BoxGroup("References")] [field: SerializeField] public TurnSlowDown turnSlowDown { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public AugmentPhysEX phys { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public Transform mountLocation { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public MountZone myMountZone { get; private set; }
    
    [BoxGroup("Orientation")] [field: SerializeField] public MouseLookEX.RotateToMouseWorldSpace mouseLook { get; private set; }
    
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir facingDir { get; set; }
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir moveDir { get; set; }
    
    //[BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool canMount = false;
    //[BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isShooting;
    //[BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> isRunning = new ReactiveIntercept<bool>(false);
    
    // Dynamic Variables
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider != null ? capsuleCollider.height : 0;
    
    
    //[BoxGroup("Guards")] [ShowInInspector, ReadOnly] public SimpleGuarderImmutable shootGuarder;
    //[BoxGroup("Guards")] [ShowInInspector, ReadOnly] public ActionGuarderImmutable cantJumpGuarder;
    [BoxGroup("Guards")] [ShowInInspector, ReadOnly] public LazyGuarderMutable mouseZoneGuarder;
    
    
    
    
}
