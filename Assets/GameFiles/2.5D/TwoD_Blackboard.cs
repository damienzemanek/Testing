using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Extensions.NumEX;
using static Ledge;
using static TwoD_Config;

public class TwoD_Blackboard : Blackboard, IFacadeCompositionElement<TwoD_Controller>
{
    public TwoD_Controller facade { get; set; }
    
    public Animator animator { get; private set; } 
    public TwoD_InputReader input { get; private set; }
    public Rigidbody rb { get; private set; }
    public Transform facing { get; private set; }
    public CapsuleCollider capsuleCollider { get; private set; }
    
    public Movement_TwoD_Config movement { get; private set; }
    public WeaponManager weapons { get; private set; }
    public ProjectileSpawnManager bulletSpawner { get; private set; }
    public AnimatorController_TwoD animController { get; private set; }
    public TurnSlowDown turnSlowDown { get; private set; }
    public AugmentPhysEX phys { get; private set; }
    
    [BoxGroup("Orientation")] public RotateToMouseWorldSpace mouseLook { get; private set; }
    [BoxGroup("Timers")] public DecayTimer moveDecay { get; private set; }
    [BoxGroup("Timers")] public CountdownTimer jumpDelay { get; private set; }
    [BoxGroup("Timers")] public CountdownTimer turnSlowdown { get; private set; }
    
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly] public LookDir moveDir;
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool canMount = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isLooking;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isShooting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool hasDoubleJumped;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool hasRequestedMount = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> isRunning = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> isMantled = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> hasJumped = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> canMantle = false;
    
    public float dblJumpMult = 1.5f;
    
    // Dynamic Variables
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => capsuleCollider.height;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumpOnCooldown => jumpDelay.isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    
    [BoxGroup("Guards")] [SerializeField] SimpleGuarderImmutable _shootGuarder;
    [ShowInInspector, ReadOnly] public ActionGuarderImmutable cantJumpGuarder;
}