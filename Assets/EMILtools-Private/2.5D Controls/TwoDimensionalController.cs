using System;
using System.Collections;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.AnimEX;
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
     public enum LookDir { Fwd, Left, Right }

    
    [Title("References")] 
    [SerializeField] Animator animator;
    [SerializeField] TwoD_InputReader input;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform facing;
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
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] ReactiveInterceptVT<bool> isGrounded;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool hasJumped;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool hasDoubleJumped;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool isMantled;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public bool canMantle;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => this.Get<CapsuleCollider>().height;

    
    [BoxGroup("Movement")] [SerializeField] float walkForce = 230f; // value based on mass 1
    [BoxGroup("Movement")] [SerializeField] Ref<float> runForce = 390f; // value based on mass 1
    [BoxGroup("Movement")] [SerializeField] ForceMode moveForceMode = ForceMode.Force;
    [BoxGroup("Movement")] [SerializeField] Ref<float> moveDecayMult = 2.5f;
    [BoxGroup("Movement")] [SerializeField] float mantleXOffset = 1f;
    [BoxGroup("Movement")] [SerializeField] float mantleDelay = 1f;

    
    AnimationCurve currentTurnSlowDownCurve => (isGrounded.Value ? turnSlowDownCurveGrounded : turnSlowDownCurveInAir);
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveGrounded;
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveInAir;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownDuration = 0.1f;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownMult = 0.5f;

    [BoxGroup("PhysEX")] [SerializeField] private float dblJumpMult = 1.5f;
    [BoxGroup("PhysEX")] [SerializeField] JumpSettings jumpSettings;
    [BoxGroup("PhysEX")] [SerializeField] GroundedSettings groundedSettings;
    [BoxGroup("PhysEX")] [SerializeField] FallSettings fallSettings;
    
    
    [Title("Animation")]
    [SerializeField] float ANIM_smoothTime = 0.5f;
    [SerializeField] float ANIM_speedStep = 0.05f;
    [SerializeField] float moveAnimJitterTolerance = 0.2f;
    [SerializeField] Animatable animatable;
    static readonly int jumpAnim = Animator.StringToHash("jump");
    static readonly int dblJumpAnim = Animator.StringToHash("dbljump");
    static readonly int inAirAnim = Animator.StringToHash("inair");
    static readonly int landAnim = Animator.StringToHash("land");
    static readonly int mantleAnim = Animator.StringToHash("mantle");
    static readonly int climbAnim = Animator.StringToHash("climb");
    
    
    
    
    void OnEnable()
    {
        input.Move += Move;
        input.Run += Run;
        input.Jump += Jump;
    }
    void OnDisable()
    {
        input.Move -= Move;
        input.Run -= Run;
        input.Jump -= Jump;
    }

    void Awake()
    {
        moveDecay = new DecayTimer(runForce, moveDecayMult);
        jumpDelay = new CountdownTimer(jumpSettings.cooldown);
        turnSlowdown = new CountdownTimer(turnSlowDownDuration);
        this.InitializeTimers((moveDecay, false),
                              (jumpDelay, false),
                              (turnSlowdown, true));

        isGrounded.core.Reactions.Add(OnLand);

        void OnLand(bool landed)
        {
            if (!landed) return;
            jumpDelay.Start();
            animatable.Animate(landAnim);
            hasJumped = false;
            hasDoubleJumped = false;
        }
        
    }

    void Start() => moveDecay.Start();
    void Update() => UpdateAnimator();
    
    void FixedUpdate()
    {
        isGrounded.Value = transform.IsGrounded(ref groundedSettings);
        rb.FallFaster(fallSettings);
        
        if(moving) HandleMovement();
    }

    void Move(bool v) => moving = v;
    void Run(bool v) => running = v;
    
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
            MoveChangeDirectionSlowdown(dir);
            FaceDirectionWithY(dir);
            ApplyMoveForce(dir);
        }
        
        void FaceDirectionWithY(Vector3 dir)
        {
            Vector3 newDir = new Vector3().With(y: -dir.x * NINETYF);
            facing.transform.rotation = Quaternion.Euler(newDir);   
        }
        void MoveChangeDirectionSlowdown(Vector3 dir)
        {
            LookDir newFacingDir = (dir.x < 0) ? LookDir.Left : LookDir.Right;
            if (newFacingDir != facingDir) NewFaceDirection();
            facingDir = newFacingDir;
        }
        void ApplyMoveForce(Vector3 dir)
        {
            float runSpeedIncludingDecay = (currentSpeed > WALK_ALPHA_MAX ? runForce : walkForce);
            float actualSpeed = running ? runSpeedIncludingDecay : walkForce;
            if (turnSlowdown.isRunning) actualSpeed *= currentTurnSlowDownCurve.Evaluate(Flip01(turnSlowdown.Progress));
            rb.AddForce( dir * actualSpeed, moveForceMode);
        }
    }

    /// <summary>
    /// For mantling and movement
    /// </summary>
    void NewFaceDirection()
    {
        turnSlowdown.Restart();
        if(isMantled) HandleUnMantleLedge();
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
            animatable.Animate(jumpAnim);
            rb.Jump(jumpSettings);
            hasJumped = true;
            print("jumped");
        }

        void HandleDoubleJump()
        {
            if (hasDoubleJumped) return;
            animatable.Animate(dblJumpAnim);
            rb.AddForce(jumpSettings.jumpForce * dblJumpMult, jumpSettings.forceMode);
            hasDoubleJumped = true;
            print("dbl jumped");
        }
    }
    
    void UpdateAnimator() => animator.SetFloat(Speed, currentSpeed);

                        #region  Ledge

    void HandleMantleLedge()
    {
        if(ledgeData.dir != facingDir) return;
        isMantled = true;
        rb.isKinematic = true;
        transform.position = transform.position.With(y: ledgeData.point.position.y - playerHeight, x: ledgeData.point.position.x - mantleXOffset);
        animatable.Animate(mantleAnim);
    }
    
    void HandleUnMantleLedge()
    {
        isMantled = false;
        rb.isKinematic = false;
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
        transform.position = ledgeData.point.position.With(x: ledgeData.point.position.x + mantleXOffset);
    }
    
    
    public void CanMantleLedge(LedgeData ledgeData)
    {
        canMantle = true;
        this.ledgeData = ledgeData;
    }

    public void CantMantleLedge() => canMantle = false;

                    #endregion
    
    
    void OnDestroy()
    {
        this.ShutdownTimers();
    }
    
    
}
