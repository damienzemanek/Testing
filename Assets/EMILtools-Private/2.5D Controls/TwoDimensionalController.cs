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
     private const float WALK_MAX_SPEED = 1f;
     private const float RUN_MAX_SPEED = 2.2f;

    
    [Header("References")] 
    [SerializeField] Animator animator;
    [SerializeField] TwoD_InputReader input;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform facing;
    [SerializeField] DecayTimer moveDecay;
    [SerializeField] CountdownTimer jumpInput;
    [SerializeField] CountdownTimer jumpDelay;
    
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

    [Header("Settings")]
    [SerializeField] float walkSpeed = 1f;
    [SerializeField] ForceMode moveForceMode = ForceMode.Force;
    [SerializeField] Ref<float> runSpeed = 2f;
    [SerializeField] Ref<float> moveDecayMult = 2.5f;
    [SerializeField] float moveAnimJitterTolerance = 0.2f;
    [SerializeField] JumpSettings jumpSettings;
    [SerializeField] GroundedSettings groundedSettings;
    [SerializeField] FallSettings fallSettings;
    
    [Header("Animation")]
    [SerializeField] float ANIM_smoothTime = 0.5f;
    [SerializeField] float ANIM_speedStep = 0.05f;
    [SerializeField] Animatable animatable;

    [ShowInInspector] ReactiveInterceptVT<bool> isGrounded;
    
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
        moveDecay = new DecayTimer(runSpeed, moveDecayMult);
        jumpInput = new CountdownTimer(jumpSettings.inputMaxDuration);
        jumpDelay = new CountdownTimer(jumpSettings.cooldown);
        this.InitializeTimers((moveDecay, false),
                              (jumpInput, true),
                              (jumpDelay, false));

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
        print(move);

        if (!running) // Walking
        {
            if(currentSpeed < WALK_MAX_SPEED) 
                currentSpeed += ANIM_speedStep;
            
            currentSpeed = ToleranceSet(currentSpeed, WALK_MAX_SPEED, moveAnimJitterTolerance);
        }
        else         // Running
        {
            if (currentSpeed > RUN_MAX_SPEED)
                currentSpeed = RUN_MAX_SPEED;
            else if(currentSpeed < RUN_MAX_SPEED) 
                currentSpeed += ANIM_speedStep;
            
            //currentSpeed = ToleranceSet(currentSpeed, RUN_MAX_SPEED, 0.2f);
        }
        
        if (move.x != 0)
        {
            Vector3 dir = move.x < 0 ? left : right;
            FaceDirectionWithY(dir);

            rb.AddForce(
                dir * (running
                    ? (currentSpeed > WALK_MAX_SPEED ? runSpeed : walkSpeed)
                    : walkSpeed),
                moveForceMode);
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

    void OnDestroy()
    {
        this.ShutdownTimers();
    }
}
