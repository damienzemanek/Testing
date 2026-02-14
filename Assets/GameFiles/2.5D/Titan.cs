using System;
using System.Collections;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using KBCore.Refs;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Timers.TimerUtility;
using static TwoD_Config;
using static TwoDimensionalController;

public class Titan : ValidatedMonoBehaviour, ITimerUser
{
    Vector3 left = Vector3.left;
    Vector3 right = Vector3.right;
    private const float ZEROF = 0f;

    
    [BoxGroup("Timers")] [SerializeField] DecayTimer moveDecay;
    public Ref<float> moveDecayScalar = 3f;
    
    [SerializeField] TwoD_InputReader input;
    [BoxGroup("References")] [SerializeField] Transform facing;

    public MountZone mountZone;
    public bool hasMounted = false;

    public Vector3 followCamOffset = new Vector3(0, 14, -18);
    public Vector3 targetCamOffset = new Vector3(0, 7, 0);
    public float speed = 20f;
    public ForceMode moveForceMode = ForceMode.VelocityChange;
    
    public Transform mountLocation;
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] CinemachineFollow follow;
    [SerializeField] CinemachineRotationComposer rotation;
    [BoxGroup("Orientation")] public RotateToMouseWorldSpace mouseLook;


    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir facingDir;
    [BoxGroup("ReadOnly")] [ReadOnly, ShowInInspector] LookDir moveDir;

    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;
    static readonly int Speed = Animator.StringToHash("Speed");
    static readonly int mountFrontAnim = Animator.StringToHash("mountFront");

    float speedAlpha // Represents the move alpha 
    {
        get => moveDecay != null ? moveDecay.Time : ZEROF;
        set => moveDecay.Time = value;
    }
    
    public bool isMoving;
    public bool isShooting;
    public bool isLooking;

    void Awake()
    {
        moveDecay = new DecayTimer(1f, moveDecayScalar);
    }

    void OnEnable()
    {
        input.Move.Add(Move);
        input.Look.Add(Look);
    }

    void OnDisable()
    {
        input.Move.Remove(Move);
        input.Look.Remove(Look);

        input.FaceDirection.Remove(FaceDirection);
    }

    void Start()
    {
        if(cinemachineCamera == null) cinemachineCamera = FindFirstObjectByType(typeof(CinemachineCamera)) as CinemachineCamera;
        if (follow == null) follow = cinemachineCamera.Get<CinemachineFollow>();
        if (rotation == null) rotation = cinemachineCamera.Get<CinemachineRotationComposer>();
        mouseLook.cam = Camera.main;
    }

    void Update()
    {
        if(!hasMounted && mountZone.inZone && mountZone.playerRequestedMount)
            StartCoroutine(HandleMount());
        HandleMovement();
        if(!input.mouseZoneGuarder) input.mouseZones.CheckAllZones(input.mouse);
        animator.SetFloat(Speed, speedAlpha);

    }
    void LateUpdate()
    {
        HandleLooking(); // Needs to be constantly polled for or else player will reset rot when not "looking"
    }
    
    public void HandleLooking()
    {
        if (input._lookGuarder || !hasMounted) return;
        mouseLook.Execute();
    }
    
    void FaceDirection(LookDir dir, bool active)
    {
        print("a"); 
        if (!hasMounted) return;
        
        print("looking in dir: " + dir);
        if (dir == LookDir.Left) facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        if (dir == LookDir.Right) facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        facingDir = dir;
    }

    IEnumerator HandleMount()
    {
        mountZone.playerTransform.position = mountLocation.position; 
        mountZone.playerTransform.parent = mountLocation;
        cinemachineCamera.Target.TrackingTarget = transform;
        ApplyCamSettings();
        hasMounted = true;
        //input.mouseZoneGuarder = new SimpleGuarderMutable(("Not Looking", () => !isLooking));
        input._lookGuarder = new SimpleGuarderMutable();
        input.FaceDirection = new PersistentAction<LookDir, bool>();
        input.FaceDirection.Add(FaceDirection);
        this.InitializeTimers((moveDecay, true));
        moveDecay.Start();
        animator.Play(mountFrontAnim);
        mountZone.playerTransform.Get<Collider>().enabled = false;
        this.Get<AugmentPhysEX>().fallFaster = false;

        yield return new WaitForSeconds(1f);
        mountZone.playerTransform.gameObject.SetActive(false);
    }

    void ApplyCamSettings()
    {
        follow.FollowOffset = followCamOffset;
        rotation.TargetOffset = targetCamOffset;
    }


    void Move(bool v) => isMoving = v;
    void Shoot(bool v) => isShooting = v;
    void Look(bool v) => isLooking = v;
    void HandleMovement()
    {
        if (!hasMounted) return;
        if (!isMoving) return;
        
        Walk();
        Move(input.movement);
        
        void Walk()
        {
             if(speedAlpha < 1f) speedAlpha += 0.1f;
             speedAlpha = NumEX.ToleranceSet(speedAlpha, 1, 0.2f);
        }

        void Move(Vector2 move)
        {
            if (move.x == 0) return;
            LookDir prevMoveDir = moveDir;
            
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
            
            // float runSpeedIncludingDecay = (speedAlpha > WALK_ALPHA_MAX ? movement.maxSpeed : movement.moveForce);
            // float actualSpeed = isRunning ? runSpeedIncludingDecay : movement.moveForce;
            // if (turnSlowdown.isRunning) actualSpeed *= turnSlowDown.Eval(phys.isGrounded, turnSlowdown.Progress);
            // if (!phys.isGrounded) actualSpeed *= phys.fallSettings.inAirMoveScalar;
            rb.AddForce(dir * speed, moveForceMode);
        }
    }
}
