using UnityEngine;
using static TwoDimensionalController;

public class AnimatorController_TwoD : MonoBehaviour
{

    public readonly struct AnimToken
    {
        public readonly int hash;
        public readonly AnimState state;
        public AnimToken(string str, AnimState state)
        {
            hash = Animator.StringToHash(str);
            this.state = state;
        }
        
        public static implicit operator int(AnimToken token) => token.hash;
    }
    
    [SerializeField] public float speedStep = 0.15f;
    [SerializeField] public float moveJitterTolerance = 0.15f;
    [SerializeField] public Animator animator;
    [SerializeField] public AnimState state;
    
    static readonly int Speed = Animator.StringToHash("Speed");
    
    
    public readonly AnimToken jump = new("jump", AnimState.Jump);
    public readonly AnimToken dbljump = new("dbljump", AnimState.Jump);
    public readonly AnimToken airtime = new("inair", AnimState.InAir);
    public readonly AnimToken land = new("land", AnimState.Locomotion);
    public readonly AnimToken mantle = new("mantle", AnimState.Mantle);
    public readonly AnimToken climb = new("climb", AnimState.Climb);
    public readonly AnimToken shoot = new("shoot", AnimState.Locomotion);
    public readonly AnimToken upperbodyidle = new("upperbodyidle", AnimState.Locomotion);
    public readonly AnimToken move = new("Move", AnimState.Locomotion);
    public readonly AnimToken moveback = new("MoveBack", AnimState.Locomotion);
    
    
    
    public void UpdateLocomotion(LookDir facingDir, LookDir moveDir, float currentSpeed)
    {
        animator.SetFloat(Speed, currentSpeed);
        if (facingDir != moveDir) Play(moveback);
        else Play(move);
    }
    
    public void Play(in AnimToken token, int layer = -1, float normalizedTime = float.NegativeInfinity)
    {
        state = token.state;
        animator.Play(token.hash, layer, normalizedTime);
    }
    
    public static implicit operator Animator(AnimatorController_TwoD ac) => ac.animator;
}
