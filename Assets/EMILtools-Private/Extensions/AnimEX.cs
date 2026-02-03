using System;
using System.Collections;
using EMILtools.Utilities;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class AnimEX
    {
        [Serializable]
        public struct Animatable
        {
            [SerializeField] public Animator animator;
            public Deviatable animSpeed;
            [HideInInspector] public bool blocked;
           

            public void Animate(int animHash, MonoBehaviour host = null, Action postHook = null, int layer = 0)
            {
                if (blocked) return;
                if (animator == null) { Debug.LogWarning("Early Return: NO Animator found"); return; }

                float speed = animSpeed.value;
                if (speed == 0) speed = 1;

                this.Log($"Animating {animHash}");


                if (postHook != null)
                {
                    if (host == null) this.Error("Need a Monobehaviour host parameter to use PostHook");
                    animator.PlayWithHook(animHash, host, postHook, layer);
                }
                else
                    animator.Play(animHash, layer);

                if (animSpeed.deviate)
                    animator.speed = speed;
            }

            public void CrossFade(int stateHash, MonoBehaviour host = null, Action postHook = null, float fade = 0.25f, int layer = 0)
            {
                if(blocked) return;
                if (animator == null) { Debug.LogWarning("Early Return: NO Animator found"); return; }

                float speed = animSpeed.value;
                if (speed == 0) speed = 1;
                
                if (postHook != null)
                    animator.CrossFadeWithHook(stateHash, host, postHook, fade, layer);
                else
                    animator.CrossFade(stateHash, fade, layer);

                if (animSpeed.deviate)
                    animator.speed = speed;

            }
        }

        public static Animatable OnlyExecuteIf(this Animatable a, Func<bool> check)
        {
            if (check.Invoke()) return a;
            a.blocked = true;
            return a;
        }

        public static bool IsPlaying(this Animator animator, string clipName, int layer = 0)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);

            if (info.IsName(clipName)) return true;
            if (animator.IsTransitionTo(clipName, layer)) return true;

            return false;
        }

        public static bool IsTransitionTo(this Animator animator, string clipname, int layer = 0)
        {
            return (animator.IsInTransition(layer) && animator.GetNextAnimatorStateInfo(layer).IsName(clipname));
        }

        public static void PlayWithHook(this Animator animator, int animHash, MonoBehaviour mono, Action postHook, int layer = 0)
            => mono.StartCoroutine(C_PlayWithHook(animator, animHash, mono, postHook, layer));

        public static IEnumerator C_PlayWithHook(this Animator animator, int animHash, MonoBehaviour mono, Action postHook, int layer = 0)
        {
            animator.Play(animHash, layer);

            yield return null;

            AnimatorStateInfo animInfo = animator.GetCurrentAnimatorStateInfo(0);
            float duration = animInfo.length / animator.speed;

            yield return new WaitForSeconds(duration);

            postHook?.Invoke();
        }

        public static void CrossFadeWithHook(this Animator animator, int stateHash, MonoBehaviour mono, Action postHook, float fade = 0.25f, int layer = 0)
    => mono.StartCoroutine(C_CrossFadeWithHook(animator, stateHash, mono, postHook, fade, layer));

        public static IEnumerator C_CrossFadeWithHook(this Animator animator, int stateHash, MonoBehaviour mono, Action postHook, float fade = 0.25f, int layer = 0)
        {
            animator.CrossFade(stateHash, fade, layer);

            yield return null;

            AnimatorStateInfo animInfo = animator.GetCurrentAnimatorStateInfo(0);
            float duration = animInfo.length / animator.speed;

            yield return new WaitForSeconds(duration);

            postHook?.Invoke();
        }
    }
}
