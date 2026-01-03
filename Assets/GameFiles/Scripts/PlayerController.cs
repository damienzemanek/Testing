using UnityEngine;
using KBCore.Refs;
using Unity.Cinemachine;
using System;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static UnityEngine.Quaternion;
using static EMILtools.Extensions.PhysEX;
using EMILtools.Timers;
using EMILtools.Extensions;
using EMILtools.Signals;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Timers.TimerUtility;

public class PlayerController : ValidatedMonoBehaviour, ITimerUser, IStatUser
{
    public Dictionary<Type, ModifierExtensions.IStat> Stats { get; set; }

    [Header("References")]
    [SerializeField, Self] Animator animator;
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Anywhere] CinemachineCamera freelookCam;
    [SerializeField, Anywhere] InputReader input;

    [Header("Settings")]
    [SerializeField] public Stat<float, SpeedModifier> moveSpeed;
    [SerializeField] float rotSpeed = 15f;
    [SerializeField] float smoothTimeStart = 0.5f;
    [SerializeField] float smoothTimeEnd = 0.1f;
    [SerializeField] float moveSpeedAnimMult = 10f;
    [SerializeField] public GroundedSettings groundedSettings;
    [SerializeField] public JumpSettings jumpSettings;
    [SerializeField] public FallSettings fallSettings;

    const float ZeroF = 0f;

    public bool isGrounded { get; private set; }
    
    CountdownTimer timer_jumpInput;
    CountdownTimer timer_jumpCooldown;

    Transform mainCam;
    Vector3 movement;
    float currentSpeed;
    float currentVelocity;

    //Animator parameters
    static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake()
    {

        mainCam = Camera.main.transform;
        freelookCam.Follow = transform;
        freelookCam.LookAt = transform;

        freelookCam.OnTargetObjectWarped(
            transform,
            transform.position - mainCam.position
        );

        rb.freezeRotation = true;

        timer_jumpInput = new CountdownTimer(jumpSettings.inputMaxDuration);
        timer_jumpCooldown = new CountdownTimer(jumpSettings.cooldown);
        
        this.InitializeTimers(
                (timer_jumpInput, true), 
                (timer_jumpCooldown, false))
            .Sub (timer_jumpInput.OnTimerStop, timer_jumpCooldown.Start);
        
        this.CacheStats();
    }

    void OnDestroy() => this.ShutdownTimers();


    private void OnEnable()
    {
        input.Jump += OnJump;
    }

    private void OnDisable()
    {
        input.Jump -= OnJump;

    }

    void OnJump(bool performed)
    {
        if (!performed) { StopJumping(); return; }
        if (timer_jumpInput.isRunning) { StopJumping(); return; }
        if (timer_jumpCooldown.isRunning) { StopJumping(); return; }
        if (!isGrounded) { StopJumping(); return; }

        timer_jumpInput.Start();

        void StopJumping() => timer_jumpInput.Stop();
    }

    private void Start()
    {
        input.EnablePlayerActions();
    }

    private void Update()
    {
        isGrounded = transform.IsGrounded(ref groundedSettings);
        movement = new Vector3(input.Direction.x, 0, input.Direction.y);
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleJump();
        HandleMovement();
    }
    int timesJumpForceApplied = 0;
    void HandleJump()
    {
        //Not jumping at all
        if(!timer_jumpInput.isRunning && isGrounded)
        {
            timer_jumpInput.Stop();
            timesJumpForceApplied = 0;
            return;
        }
    
        if (timer_jumpInput.isRunning)
            rb.Jump(jumpSettings, timer_jumpInput.Progress);

        timesJumpForceApplied++;
        this.Log($"timesJumpForceApplied: {timesJumpForceApplied}");

        PhysEX.FallFaster(rb, fallSettings);
    }


    void UpdateAnimator()
    {
        animator.SetFloat(Speed, currentSpeed * moveSpeedAnimMult);
    }

    private void HandleMovement()
    {
        //Get the moveDir, and adjust it angled with the camera
        var adjustedDir = AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;
        bool moving = isMoving(adjustedDir);

        if (moving)
        {
            //Adjust rotation to match movement direction
            RotateThePlayer(LookRotation(adjustedDir));

            //Moving the player
            HandleHorizMovement(adjustedDir);
        }
        SmoothSpeed(adjustedDir, moving);


    }

    void RotateThePlayer(Quaternion targetRot) => transform.rotation = RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);

    void HandleHorizMovement(Vector3 adjustedDir)
    {
        Vector3 vel = adjustedDir * moveSpeed.Value * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(vel.x, rb.linearVelocity.y, vel.z);
    }

    void SmoothSpeed(Vector3 adjustedDir, bool isMoving)
    {
        float targ = (isMoving) ? adjustedDir.magnitude : ZeroF;
        float smoothTime = (isMoving) ? smoothTimeStart : smoothTimeEnd;

        currentSpeed = SmoothDamp(currentSpeed, targ, ref currentVelocity, smoothTime);
    }

    bool isMoving(Vector3 adjustedDir) => adjustedDir.magnitude > ZeroF;
}
