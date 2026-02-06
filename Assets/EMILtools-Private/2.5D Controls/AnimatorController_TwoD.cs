using UnityEngine;
using static TwoDimensionalController;

public class AnimatorController_TwoD : MonoBehaviour
{ 
    [SerializeField] public float speedStep = 0.15f;
    [SerializeField] public float moveJitterTolerance = 0.15f;
    [SerializeField] public Animator animator;
    [SerializeField] public AnimState state;
    
    static readonly int Speed = Animator.StringToHash("Speed");
    
    public readonly int jumpAnim = Animator.StringToHash("jump");
    public readonly int dblJumpAnim = Animator.StringToHash("dbljump");
    public readonly int inAirAnim = Animator.StringToHash("inair");
    public readonly int landAnim = Animator.StringToHash("land");
    public readonly int mantleAnim = Animator.StringToHash("mantle");
    public readonly int climbAnim = Animator.StringToHash("climb");
    public readonly int shootAnim = Animator.StringToHash("shoot");
    public readonly int upperbodyidle = Animator.StringToHash("upperbodyidle");
    public readonly int moveLocomotion = Animator.StringToHash("Move");
    public readonly int moveBackLocomotion = Animator.StringToHash("MoveBack");
    
    public void UpdateLocomotion(LookDir facingDir, LookDir moveDir, float currentSpeed)
    {
        animator.SetFloat(Speed, currentSpeed);
        if(facingDir != moveDir) animator.Play(moveBackLocomotion);
        else animator.Play(moveLocomotion);
    }
}
