using System;
using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class TwoDimensionalController : MonoBehaviour
{
     Vector3 left = Vector3.left;
     Vector3 right = Vector3.right;
     private const float NINETYF = 90f;
     private const float ZEROF = 0f;
     static readonly int Speed = Animator.StringToHash("Speed");
     private const float WALK_MAX_SPEED = 1f;
     private const float RUN_MAX_SPEED = 2f;

    
    [Header("References")] 
    [SerializeField] Animator animator;
    [SerializeField] TwoD_InputReader input;
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform facing;

    [Header("ReadOnly")] 
    [ReadOnly, ShowInInspector] bool moving = false;
    [ReadOnly, ShowInInspector] bool running = false;
    [ReadOnly, ShowInInspector] float currentVelocity = 0f;
    [ReadOnly, ShowInInspector] float currentSpeedIncrease = 0f;

    [Header("Settings")]
    [SerializeField] float walkSpeed = 1f;
    [SerializeField] float runSpeed = 2f;
    
    [Header("Animation")]
    [SerializeField] float ANIM_smoothTime = 0.5f;
    [SerializeField] float ANIM_speedStep = 0.05f;

    
    
    private void OnEnable()
    {
        input.MoveStart += StartMove;
        input.MoveStop +=  StopMove;

        input.RunStart += StartRun;
        input.RunStop += StopRun;
    }
    private void OnDisable()
    {
        input.MoveStart -= StartMove;
        input.MoveStop -=  StopMove;
        
        input.RunStart -= StartRun;
        input.RunStop -= StopRun;
    }

    private void Update()
    {
        UpdateAniamtor();
    }

    private void FixedUpdate()
    {
        if(moving) HandleMovement();
    }

    void StartMove() => moving = true;
    void StopMove()
    {
        moving = false;
        currentSpeedIncrease = 0f;
    }
    
    void StartRun() => running = true;
    void StopRun() => running = false;
    
    void HandleMovement()
    {
        // A -> (-1, 0)
        // D -> (+1, 0)
        Vector2 move = input.movement;
        print(move);

        currentSpeedIncrease += ANIM_speedStep;
        if (!running && currentSpeedIncrease > WALK_MAX_SPEED) currentSpeedIncrease = WALK_MAX_SPEED;
        if (running && currentSpeedIncrease > RUN_MAX_SPEED) currentSpeedIncrease = RUN_MAX_SPEED;

        Vector3 newDir = Vector3.zero;
        // A
        if (move.x < 0)
        {
            newDir = left;
            FaceDirectionWithY(left);
        }
        
        //D
        if (move.x > 0)
        {
            newDir = right;
            FaceDirectionWithY(right);
        }
        
        rb.AddForce(newDir *= (
                running ? 
                ((currentSpeedIncrease > WALK_MAX_SPEED ? runSpeed : walkSpeed)) 
                : walkSpeed), 
            ForceMode.Impulse);

        
    }
    

    void UpdateAniamtor() => animator.SetFloat(Speed, currentSpeedIncrease);
    

    void FaceDirectionWithY(Vector3 dir)
    {
        Vector3 newDir = new Vector3().With(y: -dir.x * NINETYF);
        facing.transform.rotation = Quaternion.Euler(newDir);   
    }
}
