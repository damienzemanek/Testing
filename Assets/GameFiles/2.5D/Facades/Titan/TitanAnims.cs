using UnityEngine;
using static ITwoD_Blackboard;

public class TitanAnims : MonoBehaviour
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
    public readonly AnimToken falling = new("Falling", AnimState.InAir);
    public readonly AnimToken land = new("Land", AnimState.Locomotion);
    public readonly AnimToken shoot = new("Shoot", AnimState.Locomotion);
    public readonly AnimToken upperbodyidle = new("upperbodyidle", AnimState.Locomotion);
    public readonly AnimToken move = new("Locomotion", AnimState.Locomotion);
    public readonly AnimToken moveback = new("MoveBack", AnimState.Locomotion);
    public readonly AnimToken mountFrontAnim = new("mountFront", AnimState.MountFront);
    public readonly AnimToken dismountAnim = new("dismount", AnimState.Dismount);

    public void UpdateLocomotion(LookDir facingDir, LookDir moveDir, float currentSpeed)
    {
        animator.SetFloat(Speed, currentSpeed);
        if (facingDir == moveDir) Play(moveback);
        else Play(move);
    }

    public void Play(in AnimToken token, int layer = 0, float normalizedTime = float.NegativeInfinity)
    {
        state = token.state;
        animator.Play(token.hash, layer, normalizedTime);
    }



}
