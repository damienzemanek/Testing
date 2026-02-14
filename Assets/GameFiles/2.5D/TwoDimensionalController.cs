using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using KBCore.Refs;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Extensions.NumEX;
using static EMILtools.Signals.ModifierExtensions;
using static EMILtools.Timers.TimerUtility;
using static Ledge;
using static TwoD_Config;

public class TwoDimensionalController : ValidatedMonoBehaviour, ITimerUser
{
     Vector3 left = Vector3.left;
     Vector3 right = Vector3.right;

     private const float WALK_ALPHA_MAX = 1f;
     private const float RUN_ALPHA_MAX = 2.2f; // Should be greater than the greatest blend tree value to avoid jitter
     public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb }
     public Dictionary<Type, IStat> Stats { get; set; }
     
    [BoxGroup("References")] [SerializeField] Animator animator; 
    [BoxGroup("References")] [SerializeField] TwoD_InputReader input;
    [BoxGroup("References")] [SerializeField] Rigidbody rb;
    [BoxGroup("References")] [SerializeField] Transform facing;
    
    [BoxGroup("Orientation")] public RotateToMouseWorldSpace mouseLook;
    
    [BoxGroup("Timers")] [SerializeField] DecayTimer moveDecay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer jumpDelay;
    [BoxGroup("Timers")] [SerializeField] CountdownTimer turnSlowdown;
    
    
    
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool moving = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] public bool canMount = false;
    public bool requestedMount = false;

    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] ReactiveIntercept<bool> isRunning = false;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] bool jumpOnCooldown => jumpDelay.isRunning;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector]
    float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZeroF;
        set => moveDecay.Time = value;
    }
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir moveDir;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isLooking;

    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] ReactiveIntercept<bool> isMantled = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isShooting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] ReactiveIntercept<bool> hasJumped = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool hasDoubleJumped;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public ReactiveIntercept<bool> canMantle = false;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public LedgeData ledgeData;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public float playerHeight => this.Get<CapsuleCollider>().height;

    
    [InlineEditor] public TwoD_Functionality.MoveModule.Config movement;
    [BoxGroup("Weapons")] [InlineEditor] public WeaponManager weapons;
    [BoxGroup("Weapons")] public ProjectileSpawnManager bulletSpawner;
    [SerializeField, Self] AnimatorController_TwoD animController;
    [SerializeField] TurnSlowDown turnSlowDown;
    [BoxGroup("PhysEX")] [SerializeField] private float dblJumpMult = 1.5f;
    [BoxGroup("PhysEX")] [SerializeField, Self] AugmentPhysEX phys;

    
    [BoxGroup("Guards")] [SerializeField] SimpleGuarderImmutable _moveGuarder;
    [BoxGroup("Guards")] [SerializeField] SimpleGuarderImmutable _shootGuarder;
    [ShowInInspector, ReadOnly] public ActionGuarderImmutable cantJumpGuarder;
    
    void OnEnable()
    {
        input.Move.Add(Move);
        input.Run.Add(Run);
        input.Jump.Add(Jump);
        input.Look.Add(Look);
        input.Shoot.Add(Shoot);
        input.Interact.Add(HandleMountTitan); 
        input.FaceDirection.Add(FaceDirection);
    }
    void OnDisable()
    {
        input.Move.Remove(Move);
        input.Run.Remove(Run);
        input.Jump.Remove(Jump);
        input.Look.Remove(Look);
        input.Shoot.Remove(Shoot);
        input.Interact.Remove(HandleMountTitan); 
        input.FaceDirection.Remove(FaceDirection);
    }

    void Awake()
    {
        
        // Super easy to check what flags influence what methods
        _moveGuarder = new SimpleGuarderImmutable(("Not Moving", () => !moving)); // Cant move is !moving
        _shootGuarder = new SimpleGuarderImmutable(("Mantled", () => isMantled)); // Cant Shoot if mantled
        input._lookGuarder = new SimpleGuarderMutable(("Mantled", () => isMantled)); // CAnt look if mantled
        input.mouseZoneGuarder = new SimpleGuarderMutable(("Not Looking", () => !isLooking),
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
        


        cantJumpGuarder = new ActionGuarderImmutable(
            new LazyActionGuard<LazyFuncLite<bool>>(isMantled.SimpleReactions, () => isMantled, then: HandleClimb, "Is Mantled", "Climb"),
            new LazyActionGuard<LazyFuncLite<bool>>(canMantle.SimpleReactions, () => canMantle, HandleMantleLedge, "Can Mantle", "Mantle"),
            new LazyActionGuard<LazyFuncLite<bool>>(hasJumped.SimpleReactions, () => hasJumped, HandleDoubleJump, "Has Jumped", "Double Jump"),
            new ActionGuard(() => jumpOnCooldown, "Jump On Cooldown"),
            new ActionGuard(() => !phys.isGrounded, "In the Air"));
    }
    
    
    
    void Start() => moveDecay.Start();

    void Update()
    {
        if(animController.state == AnimState.Locomotion)
            animController.UpdateLocomotion(facingDir, moveDir, speedAlpha);
        if(!input.mouseZoneGuarder) input.mouseZones.CheckAllZones(input.mouse);
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleShooting();
        if (!canMount) requestedMount = false;
    }
    
    void LateUpdate()
    {
        HandleLooking(); // Needs to be constantly polled for or else player will reset rot when not "looking"
    }
    
    public void HandleLooking()
    {
        if (input._lookGuarder) return;
        mouseLook.Execute();
    }



    void HandleMountTitan()
    {
        if (canMount) requestedMount = true;
    }

    void Jump()
    {
        if (cantJumpGuarder) return;
            
        animController.Play(animController.jump);
        rb.Jump(phys.jumpSettings);
        hasJumped.Value = true;
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
    void HandleMovement() { if (_moveGuarder) return;
        
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
    void HandleShooting() 
    {
        if (_shootGuarder) return;
        
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


    void FaceDirection(LookDir dir, bool active)
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
        hasJumped.Value = false;
        hasDoubleJumped = false;
    }
    
    
    #region ========================================= Ledge =============================================================================

    void HandleMantleLedge()
    {
        if(ledgeData.dir == facingDir) return;
        isMantled.Value = true;
        rb.isKinematic = true;
        float offset = movement.mantleXOffset;
        if(ledgeData.dir == LookDir.Right) offset *= -1;
        transform.position = transform.position.With(y: ledgeData.point.position.y - playerHeight, x: ledgeData.point.position.x + offset);
        animController.state = AnimState.Mantle;
        animController.Play(animController.mantle);
    }
    
    void HandleUnMantleLedge()
    {
        isMantled.Value = false;
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
        isMantled.Value = false;
        rb.isKinematic = false;
        animController.state = AnimState.Locomotion;
        float offset = movement.mantleXOffset;
        if(ledgeData.dir == LookDir.Right) offset *= -1;
        transform.position = ledgeData.point.position.With(x: ledgeData.point.position.x - offset);
    }
    
    
    public void CanMantleLedge(LedgeData ledgeData)
    {
        canMantle.Value = true;
        this.ledgeData = ledgeData;
    }

    public void CantMantleLedge() => canMantle.Value = false;
    

                    #endregion
    
    
    void OnDestroy()
    {
        this.ShutdownTimers();
    }

}
