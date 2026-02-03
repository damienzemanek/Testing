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

public class TwoDimensionalController : MonoBehaviour, ITimerUser
{
     Vector3 left = Vector3.left;
     Vector3 right = Vector3.right;
     private const float NINETYF = 90f;
     private const float ZEROF = 0f;
     static readonly int Speed = Animator.StringToHash("Speed");
     private const float WALK_ALPHA_MAX = 1f;
     private const float RUN_ALPHA_MAX = 2.2f; // Should be greater than the greatest blend tree value to avoid jitter
     enum LookDir { Fwd, Left, Right }

    
    [Title("References")] 
    [SerializeField] Animator animator;
    [SerializeField] TwoD_InputReader input;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform facing;
    [BoxGroup("Timers")] [SerializeField] DecayTimer moveDecay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer jumpInput;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer jumpDelay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer turnSlowdown;
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool moving = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool running = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumping = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumpOnCooldown => jumpDelay.isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector]
    float currentSpeed // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZEROF;
        set => moveDecay.Time = value;
    }
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir facingDir;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] ReactiveInterceptVT<bool> isGrounded;


    [Title("Settings")] 
    [BoxGroup("Movement")] [SerializeField] float walkForce = 230f; // value based on mass 1
    [BoxGroup("Movement")] [SerializeField] Ref<float> runForce = 390f; // value based on mass 1
    [BoxGroup("Movement")] [SerializeField] ForceMode moveForceMode = ForceMode.Force;
    [BoxGroup("Movement")] [SerializeField] Ref<float> moveDecayMult = 2.5f;
    
    AnimationCurve currentTurnSlowDownCurve => (isGrounded.Value ? turnSlowDownCurveGrounded : turnSlowDownCurveInAir);
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveGrounded;
    [BoxGroup("Turn Slow Down")] [SerializeField] AnimationCurve turnSlowDownCurveInAir;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownDuration = 0.1f;
    [BoxGroup("Turn Slow Down")] [SerializeField] float turnSlowDownMult = 0.5f;
    
    
    [BoxGroup("PhysEX")] [SerializeField] JumpSettings jumpSettings;
    [BoxGroup("PhysEX")] [SerializeField] GroundedSettings groundedSettings;
    [BoxGroup("PhysEX")] [SerializeField] FallSettings fallSettings;
    
    [Title("Animation")]
    [SerializeField] float ANIM_smoothTime = 0.5f;
    [SerializeField] float ANIM_speedStep = 0.05f;
    [SerializeField] float moveAnimJitterTolerance = 0.2f;
    [SerializeField] Animatable animatable;

    
    
    
    
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
        jumpInput = new CountdownTimer(jumpSettings.inputMaxDuration);
        jumpDelay = new CountdownTimer(jumpSettings.cooldown);
        turnSlowdown = new CountdownTimer(turnSlowDownDuration);
        this.InitializeTimers((moveDecay, false),
                              (jumpInput, true),
                              (jumpDelay, false),
                              (turnSlowdown, true));
        
        isGrounded.core.Reactions.Add(OnLand);
        void OnLand(bool landed) { if(landed) jumpDelay.Start();}
    }

    void Start() => moveDecay.Start();
    void Update() => UpdateAnimator();
    
    void FixedUpdate()
    {
        isGrounded.Value = transform.IsGrounded(ref groundedSettings);
        rb.FallFaster(fallSettings);
        
        if(moving) HandleMovement();
        if(jumping) HandleJump();
    }

    void Move(bool v) => moving = v;
    void Run(bool v) => running = v;
    void Jump(bool v) => jumping = v;
    
    void HandleMovement()
    {
        // A -> (-1, 0)
        // D -> (+1, 0)
        Vector2 move = input.movement;

        if (!running) // Walking
        {
            if(currentSpeed < WALK_ALPHA_MAX) 
                currentSpeed += ANIM_speedStep;
            
            currentSpeed = ToleranceSet(currentSpeed, WALK_ALPHA_MAX, moveAnimJitterTolerance);
        }
        else         // Running
        {
            if (currentSpeed > RUN_ALPHA_MAX)
                currentSpeed = RUN_ALPHA_MAX;
            else if(currentSpeed < RUN_ALPHA_MAX) 
                currentSpeed += ANIM_speedStep;
            
            //currentSpeed = ToleranceSet(currentSpeed, RUN_MAX_SPEED, 0.2f);
        }
        
        if (move.x != 0)
        {
            Vector3 dir = move.x < 0 ? left : right;
            HandleChangeDirectionSlowdown(dir);
            FaceDirectionWithY(dir);
            
            float runSpeedIncludingDecay = (currentSpeed > WALK_ALPHA_MAX ? runForce : walkForce);
            float actualSpeed = running ? runSpeedIncludingDecay : walkForce;
            
            if (turnSlowdown.isRunning) actualSpeed *= currentTurnSlowDownCurve.Evaluate(Flip01(turnSlowdown.Progress));
            
            rb.AddForce( dir * actualSpeed, moveForceMode);
        }
    }

    void HandleJump()
    {
        if (jumpOnCooldown) return;
        
        if(!jumpInput.isRunning && isGrounded)
            jumpInput.Start();
        
        if(jumpInput.isRunning)
            rb.Jump(jumpSettings, jumpInput.Progress);
    }
    

    void UpdateAnimator() => animator.SetFloat(Speed, currentSpeed);
    

    void FaceDirectionWithY(Vector3 dir)
    {
        Vector3 newDir = new Vector3().With(y: -dir.x * NINETYF);
        facing.transform.rotation = Quaternion.Euler(newDir);   
    }

    void HandleChangeDirectionSlowdown(Vector3 dir)
    {
        LookDir newFacingDir = (dir.x < 0) ? LookDir.Left : LookDir.Right;
        if (newFacingDir != facingDir) turnSlowdown.Restart();
        facingDir = newFacingDir;
    }

    void OnDestroy()
    {
        this.ShutdownTimers();
    }
}
