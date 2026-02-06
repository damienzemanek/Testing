using System;
using System.Collections;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.AnimEX;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Extensions.NumEX;
using static EMILtools.Extensions.PhysEX;
using static EMILtools.Timers.TimerUtility;
using static Ledge;

public class TwoDimensionalController : MonoBehaviour, ITimerUser
{
     Vector3 left = Vector3.left;
     Vector3 right = Vector3.right;
     private const float NINETYF = 90f;
     private const float ZEROF = 0f;
     static readonly int Speed = Animator.StringToHash("Speed");
     private const float WALK_ALPHA_MAX = 1f;
     private const float RUN_ALPHA_MAX = 2.2f; // Should be greater than the greatest blend tree value to avoid jitter
     public enum LookDir { None, Left, Right }
     public enum AnimState { Locomotion, Jump, InAir, Land, Mantle }

    
    [BoxGroup("References")] [SerializeField] Animator animator; 
    [BoxGroup("References")] [SerializeField] TwoD_InputReader input;
    [BoxGroup("References")] [SerializeField] Rigidbody rb;
    [BoxGroup("References")] [SerializeField] Transform facing;
    
    [BoxGroup("Timers")] [SerializeField] DecayTimer moveDecay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer jumpDelay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer turnSlowdown;
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool moving = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool running = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumpOnCooldown => jumpDelay.isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector]
    float currentSpeed // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZEROF;
        set => moveDecay.Time = value;
    }
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir moveDir;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] ReactiveInterceptVT<bool> isGrounded;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isLooking;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isMantled;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isShooting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool hasJumped;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool hasDoubleJumped;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool canMantle;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => this.Get<CapsuleCollider>().height;

    
    [InlineEditor] public Movement_TwoD_Config movement;
    [BoxGroup("Weapons")] [InlineEditor] public WeaponManager weapons;
    [BoxGroup("Weapons")] public ProjectileSpawnManager bulletSpawner;
    
    [BoxGroup("Orientation")] [SerializeField] RotateToMouseWorldSpace mouseLook;
    [BoxGroup("Orientation")] [SerializeField] MouseCallbackZones mouseZones;
    
    AnimationCurve currentTurnSlowDownCurve => (isGrounded.Value ? turnSlowDownCurveGrounded : turnSlowDownCurveInAir);
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveGrounded;
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveInAir;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownDuration = 0.1f;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownMult = 0.5f;

    [BoxGroup("PhysEX")] [SerializeField] private float dblJumpMult = 1.5f;
    [BoxGroup("PhysEX")] [SerializeField] JumpSettings jumpSettings;
    [BoxGroup("PhysEX")] [SerializeField] GroundedSettings groundedSettings;
    [BoxGroup("PhysEX")] [SerializeField] FallSettings fallSettings;
    
    
    [BoxGroup("Animation")] [SerializeField] float ANIM_smoothTime = 0.5f;
    [BoxGroup("Animation")] [SerializeField] float ANIM_speedStep = 0.05f;
    [BoxGroup("Animation")] [SerializeField] float moveAnimJitterTolerance = 0.2f;
    [BoxGroup("Animation")] [SerializeField] Animatable animatable;
    [BoxGroup("Animation")] [SerializeField] AnimState animState;
    
    static readonly int jumpAnim = Animator.StringToHash("jump");
    static readonly int dblJumpAnim = Animator.StringToHash("dbljump");
    static readonly int inAirAnim = Animator.StringToHash("inair");
    static readonly int landAnim = Animator.StringToHash("land");
    static readonly int mantleAnim = Animator.StringToHash("mantle");
    static readonly int climbAnim = Animator.StringToHash("climb");
    static readonly int shootAnim = Animator.StringToHash("shoot");
    static readonly int upperbodyidle = Animator.StringToHash("upperbodyidle");
    static readonly int moveLocomotion = Animator.StringToHash("Move");
    static readonly int moveBackLocomotion = Animator.StringToHash("MoveBack");

    
    
    void OnEnable()
    {
        input.Move += Move;
        input.Run += Run;
        input.Jump += Jump;
        input.Look += Look;
        input.Shoot += Shoot;
    }
    void OnDisable()
    {
        input.Move -= Move;
        input.Run -= Run;
        input.Jump -= Jump;
        input.Look -= Look;
        input.Shoot -= Shoot;
    }

    void Awake()
    {
        moveDecay = new DecayTimer(movement.runForce, movement.decayScalar);
        jumpDelay = new CountdownTimer(jumpSettings.cooldown);
        turnSlowdown = new CountdownTimer(turnSlowDownDuration);
        this.InitializeTimers((moveDecay, false),
                                (jumpDelay, false),
                                (turnSlowdown, true));
        
        isGrounded.core.Reactions.Add(OnLand);

        void OnLand(bool landed)
        {
            if (!landed) return;

            animState = AnimState.Locomotion;
            jumpDelay.Start();
            animatable.Animate(landAnim);
            hasJumped = false;
            hasDoubleJumped = false;
        }
        
        rb.maxLinearVelocity = movement.maxVelMagnitude;
        rb.maxAngularVelocity = movement.maxVelMagnitude;
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        float halfScreenWidth = mouseZones.w * 0.5f;
        float screenHeight = mouseZones.h;
        
        mouseZones.AddInitalZones(
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { HandleLookInDirection(LookDir.Right); }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => {HandleLookInDirection(LookDir.Left); }));

    }

    void Start() => moveDecay.Start();

    void Update()
    {
        if(animState == AnimState.Locomotion) UpdateAnimator();
        if(isLooking && !isMantled) mouseZones.CheckAllZones(input.mouse);
    }
    
    void FixedUpdate()
    {
        isGrounded.Value = transform.IsGrounded(ref groundedSettings);
        rb.FallFaster(fallSettings);
        
        if(moving) HandleMovement();
        if(!isMantled) HandleShooting();
    }

    void LateUpdate()
    {
        if(isMantled) return;
        HandleLooking(); // Needs to be constantly polled for or else player will reset rot when not "looking"
    }

    void Move(bool v) => moving = v;
    void Run(bool v) => running = v;
    void Look(bool v) => isLooking = v;

    void Shoot(bool v) => isShooting = v;
    
    
    /// <summary>
    /// Sequencing for movement
    /// </summary>
    void HandleMovement()
    {
        if (!running) Walk();
        else Run();
        
        Move(input.movement);
        
        void Walk()
        {
            if(currentSpeed < WALK_ALPHA_MAX) 
                currentSpeed += ANIM_speedStep;
            currentSpeed = ToleranceSet(currentSpeed, WALK_ALPHA_MAX, moveAnimJitterTolerance);
        }
        void Run()
        {
            if (currentSpeed > RUN_ALPHA_MAX)
                currentSpeed = RUN_ALPHA_MAX;
            else if(currentSpeed < RUN_ALPHA_MAX) 
                currentSpeed += ANIM_speedStep;
        }
        void Move(Vector2 move)
        {
            if (move.x == 0) return;
            
            Vector3 dir = move.x < 0 ? left : right;
            moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
            //FaceDirectionWithY(dir);
            ApplyMoveForce(dir);
        }
        
        // DEPRACTED
        // void FaceDirectionWithY(Vector3 dir)
        // {
        //     Vector3 newDir = new Vector3().With(y: -dir.x * NINETYF);
        //     facing.transform.rotation = Quaternion.Euler(newDir);   
        // }
        

        void ApplyMoveForce(Vector3 dir)
        {
            float runSpeedIncludingDecay = (currentSpeed > WALK_ALPHA_MAX ? movement.runForce : movement.walkForce);
            float actualSpeed = running ? runSpeedIncludingDecay : movement.walkForce;
            if (turnSlowdown.isRunning) actualSpeed *= currentTurnSlowDownCurve.Evaluate(Flip01(turnSlowdown.Progress));
            if (!isGrounded) actualSpeed *= fallSettings.inAirMoveScalar;
            rb.AddForce(dir * actualSpeed, movement.forceMode);
        }
    }

    void Jump()
    {
        if (isMantled) { HandleClimb(); return; }
        if (canMantle) { HandleMantleLedge(); return; }
        if (hasJumped) { HandleDoubleJump(); return; }
        
        HandleFirstJump();
        
        void HandleFirstJump()
        {
            if (hasJumped) return;
            if (jumpOnCooldown) return;
            if (!isGrounded) return;
            
            animState = AnimState.Jump;
            animatable.Animate(jumpAnim);
            rb.Jump(jumpSettings);
            hasJumped = true;
            //print("jumped");
        }

        void HandleDoubleJump()
        {
            if (hasDoubleJumped) return;
            animatable.Animate(dblJumpAnim);
            rb.AddForce(jumpSettings.jumpForce * dblJumpMult, jumpSettings.forceMode);
            hasDoubleJumped = true;
            //print("dbl jumped");
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat(Speed, currentSpeed);
        if(facingDir != moveDir) animatable.Animate(moveBackLocomotion);
        else animatable.Animate(moveLocomotion);
    }

    void HandleShooting()
    {
        if (isShooting) StartCoroutine(ShootImplementation());
        else animatable.CrossFade(upperbodyidle, layer: 1);

        IEnumerator ShootImplementation()
        {
            bulletSpawner.targetPosition = mouseLook.contactPoint;
            if (bulletSpawner.fireTimer.isRunning) yield break;
            animatable.Animate(shootAnim, layer: 1, restart: true);
            yield return null;
            bulletSpawner.Spawn();
        }
    }

    void HandleLooking()
    {
        mouseLook.LateUpdateMouseLook();
    }

    void HandleLookInDirection(LookDir dir)
    {
        print("looking in dir: " + dir);
        if (dir == LookDir.Left) facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        if (dir == LookDir.Right) facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        facingDir = dir;
    }
    
    #region ========================================= Ledge =============================================================================

    void HandleMantleLedge()
    {
        if(ledgeData.dir == facingDir) return;
        isMantled = true;
        rb.isKinematic = true;
        transform.position = transform.position.With(y: ledgeData.point.position.y - playerHeight, x: ledgeData.point.position.x - movement.mantleXOffset);
        animState = AnimState.Mantle;
        animatable.Animate(mantleAnim);
    }
    
    void HandleUnMantleLedge()
    {
        isMantled = false;
        rb.isKinematic = false;
        animState = AnimState.InAir;
        animatable.CrossFade(inAirAnim);
    }

    void HandleClimb()
    {
        animatable.CrossFade(climbAnim);
    }
    
    public void CompleteClimb()
    {
        isMantled = false;
        rb.isKinematic = false;
        animState = AnimState.Locomotion;
        transform.position = ledgeData.point.position.With(x: ledgeData.point.position.x + movement.mantleXOffset);
    }
    
    
    public void CanMantleLedge(LedgeData ledgeData)
    {
        canMantle = true;
        this.ledgeData = ledgeData;
    }

    public void CantMantleLedge() => canMantle = false;
    
    /// <summary>
    /// For Ledges and movement
    /// </summary>
    void NewFaceDirection()
    {
        turnSlowdown.Restart();
        if(isMantled) HandleUnMantleLedge();
    }

                    #endregion
    
    
    void OnDestroy()
    {
        this.ShutdownTimers();
    }
    
    
}
