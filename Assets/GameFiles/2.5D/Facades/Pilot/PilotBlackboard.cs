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
}

public interface ITwoD_Context : IBlackboard
{
    public LookDir dir { get; set; }
}

[Serializable]
public class PilotBlackboard : Blackboard, ITwoD_Blackboard
{
    [field: BoxGroup("References")] [field: SerializeField] public Rigidbody rb { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Transform facing { get; set; }
    [field: BoxGroup("References")] [field: SerializeField]  public CapsuleCollider capsuleCollider { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public WeaponManager weapons { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public ProjectileSpawnManager bulletSpawner { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public AnimatorController_TwoD animController { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField]  public TurnSlowDown turnSlowDown { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public AugmentPhysEX phys { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public CameraContext camContext { get; private set; }
    [field: BoxGroup("References")] [field: SerializeField] public Transform camFollowTransform { get; private set; }

    [BoxGroup("Orientation")] [field: SerializeField] public RotateToMouseWorldSpace mouseLook { get; private set; }
    [field: SerializeField] public PositionToMouseWorldSpace posToMouse { get; private set; }

    [BoxGroup("Timers")] [field: SerializeField] public DecayTimer moveDecay { get; set; }
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer jumpDelay { get; set; } 
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer turnSlowdown { get; set; }
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer titanProgressTimer { get; set; }
    [BoxGroup("Timers")] [field: SerializeField] public CountdownTimer spawnTitanTimer { get; set; }
    
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir facingDir { get; set; }
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir moveDir;
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool canMount = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isShooting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool hasDoubleJumped;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool hasRequestedMount = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> titanReady = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> isRunning = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> isMantled = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> hasJumped = new ReactiveIntercept<bool>(false);
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> canMantle = new ReactiveIntercept<bool>(false);
    
    public float dblJumpMult = 1.5f;
    
    // Dynamic Variables
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;

    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider != null ? capsuleCollider.height : 0;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool jumpOnCooldown => jumpDelay != null ? jumpDelay.isRunning : true;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    
    [BoxGroup("Guards")] [ShowInInspector, ReadOnly] public SimpleGuarderImmutable shootGuarder;
    [BoxGroup("Guards")] [ShowInInspector, ReadOnly] public ActionGuarderImmutable cantJumpGuarder;
    [BoxGroup("Guards")] [ShowInInspector, ReadOnly] public LazyGuarderMutable mouseZoneGuarder;

}