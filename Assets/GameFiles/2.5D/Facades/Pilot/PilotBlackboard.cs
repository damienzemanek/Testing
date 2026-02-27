using System;
using EMILtools.Core;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Extensions.NumEX;
using static ITwoD_Blackboard;
using static Ledge;
using static TwoD_InputAuthority;

public interface ITwoD_Blackboard : IBlackboard
{
    public enum LookDir { None, Left, Right }
    public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb, MountFront, Dismount }
    public Transform facing { get; set; }
    public LookDir facingDir { get; set; }
}

public interface ITwoD_Context : IBlackboard
{
    public LookDir dir { get; set; }
}

[Serializable]
public class PilotBlackboard : Blackboard, ITwoD_Blackboard
{
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public Rigidbody rb { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public Transform facing { get; set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required]public CapsuleCollider capsuleCollider { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public WeaponManager weapons { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public ProjectileSpawnManager bulletSpawner { get; private set; }

    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public Animator animator { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public TurnSlowDown turnSlowDown { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public AugmentPhysEX phys { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public CameraContext camContext { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] [field: Required] public Transform camFollowTransform { get; private set; }

    [BoxGroup("Orientation")] [field: SerializeField] [field: Required] public RotateToMouseWorldSpace mouseLook { get; private set; }
    [field: SerializeField]  public PositionToMouseWorldSpace posToMouse { get; private set; }
    [field: SerializeField] public DecayTimer moveDecay { get; set; }
    [field: SerializeField] public CountdownTimer jumpDelay { get; set; } 
    [field: SerializeField] public CountdownTimer turnSlowdown { get; set; } 
    [field: SerializeField] public CountdownTimer titanProgressTimer { get; set; }
    [field: SerializeField] public CountdownTimer spawnTitanTimer { get; set; }
    
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LookDir facingDir { get; set; }
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LookDir moveDir;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool canMount = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isShooting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool hasRequestedMount = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> titanReady = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> isRunning = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> isMantled = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> hasJumped = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> canMantle = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider != null ? capsuleCollider.height : 0;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool jumpOnCooldown => jumpDelay != null ? jumpDelay.isRunning : true;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
}