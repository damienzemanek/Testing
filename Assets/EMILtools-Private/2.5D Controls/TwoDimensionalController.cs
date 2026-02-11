using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Signals;
using EMILtools.Timers;
using KBCore.Refs;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Extensions.NumEX;
using static EMILtools.Timers.TimerUtility;
using static FlowOutChain;
using static Ledge;

public class TwoDimensionalController : ValidatedMonoBehaviour, ITimerUser
{
     Vector3 left = Vector3.left;
     Vector3 right = Vector3.right;
     private const float NINETYF = 90f;
     private const float ZEROF = 0f;
     private const float WALK_ALPHA_MAX = 1f;
     private const float RUN_ALPHA_MAX = 2.2f; // Should be greater than the greatest blend tree value to avoid jitter
     public enum LookDir { None, Left, Right }
     public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb }
     public Dictionary<Type, ModifierExtensions.IStat> Stats { get; set; }

    
    [BoxGroup("References")] [SerializeField] Animator animator; 
    [BoxGroup("References")] [SerializeField] TwoD_InputReader input;
    [BoxGroup("References")] [SerializeField] Rigidbody rb;
    [BoxGroup("References")] [SerializeField] Transform facing;
    
    [BoxGroup("Timers")] [SerializeField] DecayTimer moveDecay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer jumpDelay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer turnSlowdown;
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool moving = false;

    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] ReactiveIntercept<bool> isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumpOnCooldown => jumpDelay.isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector]
    float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZEROF;
        set => moveDecay.Time = value;
    }
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir moveDir;
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
    [SerializeField, Self] AnimatorController_TwoD animController;
    [BoxGroup("Orientation")] [SerializeField] MouseCallbackZones mouseZones;
    [BoxGroup("Orientation")] [SerializeField] RotateToMouseWorldSpace mouseLook;
    [SerializeField] TurnSlowDown turnSlowDown;
    [BoxGroup("PhysEX")] [SerializeField] private float dblJumpMult = 1.5f;
    [BoxGroup("PhysEX")] [SerializeField, Self] AugmentPhysEX phys;

    
    [BoxGroup("Guards")] [SerializeField] GuardsImmutable MoveGuards;
    [BoxGroup("Guards")] [SerializeField] GuardsImmutable ShootGuards;
    [BoxGroup("Guards")] [SerializeField] GuardsImmutable LookGuards;
    [BoxGroup("Guards")] [SerializeField] GuardsImmutable MouseZoneGuards;
    public FlowImmutable cantJumpFlowOut;
    
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
        
        // Super easy to check what flags influence what methods
        MoveGuards = new GuardsImmutable(("Not Moving", () => !moving)); // Cant move is !moving
        ShootGuards = new GuardsImmutable(("Mantled", () => isMantled)); // Cant Shoot if mantled
        LookGuards = new GuardsImmutable(("Mantled", () => isMantled)); // CAnt look if mantled
        MouseZoneGuards = new GuardsImmutable(("Not Looking", () => !isLooking),
                                              ("Mantled", () => isMantled));
        
        moveDecay = new DecayTimer(movement.maxSpeed, movement.decayScalar);
        jumpDelay = new CountdownTimer(phys.jumpSettings.cooldown);
        turnSlowdown = new CountdownTimer(turnSlowDown.duration);
        this.InitializeTimers((moveDecay, false),
                                (jumpDelay, false),
                                (turnSlowdown, true));
        
        phys.isGrounded.Reactions.Add(OnLand);
        
        rb.maxLinearVelocity = movement.maxVelMagnitude;
        rb.maxAngularVelocity = movement.maxVelMagnitude;
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        float halfScreenWidth = mouseZones.w * 0.5f;
        float screenHeight = mouseZones.h;
        mouseZones.callbackZones = null;
        mouseZones.AddInitalZones(
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { FaceDirection(LookDir.Right); }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { FaceDirection(LookDir.Left); }));


        cantJumpFlowOut = new FlowImmutable(
            Branch("Is Mantled", () => isMantled, "Climb", HandleClimb),
            Branch("Can Mantle", () => canMantle, "Mantle", HandleMantleLedge),
            Branch("Has Jumped", () => hasJumped, "Double Jump", HandleDoubleJump),
            Return("Jump Cooldown", () => jumpOnCooldown),
            Return("In the Air", () => !phys.isGrounded));
    }
    
    
    
    void Start() => moveDecay.Start();

    void Update()
    {
        if(animController.state == AnimState.Locomotion)
            animController.UpdateLocomotion(facingDir, moveDir, speedAlpha);
        if(!MouseZoneGuards) mouseZones.CheckAllZones(input.mouse);
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleShooting();
    }

    void LateUpdate()
    {
        HandleLooking(); // Needs to be constantly polled for or else player will reset rot when not "looking"
    }
    
    
    
    
    

    void Jump()
    {
        if (cantJumpFlowOut) return;
            
        animController.Play(animController.jump);
        rb.Jump(phys.jumpSettings);
        hasJumped = true;
    }
    
    void HandleDoubleJump()
    {
        if (hasDoubleJumped) return;
        animController.Play(animController.dbljump);
        rb.AddForce(phys.jumpSettings.jumpForce * dblJumpMult, phys.jumpSettings.forceMode);
        hasDoubleJumped = true;
    }
    
    void Move(bool v) => moving = v;
    void Run(bool v) => isRunning.Value = v;
    void Look(bool v) => isLooking = v;
    void Shoot(bool v) => isShooting = v;
    
    
    
    /// <summary>
    /// Sequencing for movement
    /// </summary>
    void HandleMovement() { if (MoveGuards) return;
        
        if (!isRunning) Walk();
        else Run();
        Move(input.movement);
        
        void Walk()
        {
            if(speedAlpha < WALK_ALPHA_MAX) speedAlpha += animController.speedStep;
            speedAlpha = ToleranceSet(speedAlpha, WALK_ALPHA_MAX, animController.moveJitterTolerance);
        }
        void Run()
        {
            if (speedAlpha > RUN_ALPHA_MAX)
                speedAlpha = RUN_ALPHA_MAX;
            else if(speedAlpha < RUN_ALPHA_MAX) 
                speedAlpha += animController.speedStep;
        }
        void Move(Vector2 move)
        {
            if (move.x == 0) return;
            LookDir prevMoveDir = moveDir;
            
            Vector3 dir = move.x < 0 ? left : right;
            moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
            //FaceDirectionWithY(dir);
            ApplyMoveForce(dir);
            
            if (prevMoveDir != moveDir)
            {
                turnSlowdown.Restart();
                if(isMantled) HandleUnMantleLedge();
            }
            
        }
        
        // DEPRACTED
        // void FaceDirectionWithY(Vector3 dir)
        // {
        //     Vector3 newDir = new Vector3().With(y: -dir.x * NINETYF);
        //     facing.transform.rotation = Quaternion.Euler(newDir);   
        // }
        
        void ApplyMoveForce(Vector3 dir)
        {
            
            float runSpeedIncludingDecay = (speedAlpha > WALK_ALPHA_MAX ? movement.maxSpeed : movement.moveForce);
            float actualSpeed = isRunning ? runSpeedIncludingDecay : movement.moveForce;
            if (turnSlowdown.isRunning) actualSpeed *= turnSlowDown.Eval(phys.isGrounded, turnSlowdown.Progress);
            if (!phys.isGrounded) actualSpeed *= phys.fallSettings.inAirMoveScalar;
            rb.AddForce(dir * actualSpeed, movement.forceMode);
        }
    }
    void HandleShooting() { if (ShootGuards) return;
        
        if (isShooting) StartCoroutine(ShootImplementation());
        else animController.animator.CrossFade(animController.upperbodyidle, 0.1f, 1);

        IEnumerator ShootImplementation()
        {
            bulletSpawner.targetPosition = mouseLook.core.contactPoint;
            if (bulletSpawner.fireTimer.isRunning) yield break;
            animController.animator.Play(animController.shoot, layer: 1, normalizedTime: 0f);
            yield return null;
            bulletSpawner.Spawn();
        }
    }
    void HandleLooking() { if (LookGuards) return;
        
        mouseLook.Execute();
    }

    void FaceDirection(LookDir dir)
    {
        print("looking in dir: " + dir);
        if (dir == LookDir.Left) facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        if (dir == LookDir.Right) facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        facingDir = dir;
    }
    void OnLand(bool landed)
    {
        if (!landed) return;

        animController.state = AnimState.Locomotion;
        jumpDelay.Start();
        animController.Play(animController.land);
        hasJumped = false;
        hasDoubleJumped = false;
    }
    
    
    #region ========================================= Ledge =============================================================================

    void HandleMantleLedge()
    {
        if(ledgeData.dir == facingDir) return;
        isMantled = true;
        rb.isKinematic = true;
        float offset = movement.mantleXOffset;
        if(ledgeData.dir == LookDir.Right) offset *= -1;
        transform.position = transform.position.With(y: ledgeData.point.position.y - playerHeight, x: ledgeData.point.position.x + offset);
        animController.state = AnimState.Mantle;
        animController.Play(animController.mantle);
    }
    
    void HandleUnMantleLedge()
    {
        isMantled = false;
        rb.isKinematic = false;
        animController.state = AnimState.InAir;
        animController.animator.CrossFade(animController.airtime, 0.1f);
    }

    void HandleClimb()
    {
        animController.animator.CrossFade(animController.climb, 0.1f);
    }
    
    public void CompleteClimb()
    {
        isMantled = false;
        rb.isKinematic = false;
        animController.state = AnimState.Locomotion;
        float offset = movement.mantleXOffset;
        if(ledgeData.dir == LookDir.Right) offset *= -1;
        transform.position = ledgeData.point.position.With(x: ledgeData.point.position.x - offset);
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
