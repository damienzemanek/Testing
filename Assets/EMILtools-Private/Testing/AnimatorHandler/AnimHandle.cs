using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static System.Single;

[InlineProperty]
[Serializable]
public class AnimHandle<TAnimEnum>
    where TAnimEnum : Enum
{
    
    
    [Serializable]
    public struct AnimState
    {
        public string name;
        public TAnimEnum animEnum;
        [ReadOnly] public int hash;
        public AnimState(string name, TAnimEnum animEnum)
        {
            this.name = name;
            this.animEnum = animEnum;
            hash = Animator.StringToHash(name);
        }
        public void CalculateHash() => hash = Animator.StringToHash(name);
    }
    
    
    
    public AnimState[] states;
    [HideLabel, Required] public Animator animator;

    [Button, PropertyOrder(-1)]
    public void RecalculateHashes()
    {
        if (states == null) return;
        for (int i = 0; i < states.Length; i++)
        {
            var s = states[i];
            s.CalculateHash();
            states[i] = s;
        }
    }

    int GetHash(TAnimEnum animEnum)     
    {
        foreach (var state in states)
            if (EqualityComparer<TAnimEnum>.Default.Equals(state.animEnum, animEnum)) return state.hash;
        return -1;
    }
    
    public bool Play(TAnimEnum animEnum, int layer = 0, float normalizedTime = NegativeInfinity)
    {
        if (animator == null) return false;
        if (states == null) return false;
        if (layer < 0 || layer >= animator.layerCount) return false;
        
        foreach (var state in states)
        {
            if (!EqualityComparer<TAnimEnum>.Default.Equals(state.animEnum, animEnum)) continue;
            animator.Play(state.hash, layer, normalizedTime);
            var cur = animator.GetCurrentAnimatorStateInfo(layer);
            Debug.Log($"AnimHandle.Play({animEnum}) requestedHash={state.hash}, currentFullPathHash={cur.fullPathHash}, " +
                      $"currentShortNameHash={cur.shortNameHash}, inTransition={animator.IsInTransition(layer)}, layerWeight={animator.GetLayerWeight(layer)}");
            return true;
        }
        Debug.LogWarning($"AnimHandle: No state mapped for enum {animEnum}");
        return false;
    }
    
    public bool PlayThenOnEnd(TAnimEnum animEnum, Action onEnd, int layer = 0, float normalizedTime = NegativeInfinity)
    {
        if(!Play(animEnum, layer, normalizedTime)) return false;
        PlayThenOnEndAsync(animEnum, onEnd, layer);
        return true;
    }
    

    async void PlayThenOnEndAsync(TAnimEnum animEnum, Action onEnd, int layer)
    {
        int hash = GetHash(animEnum);
        if (hash == -1) { Debug.LogWarning($"AnimHandle: No hash mapped for enum {animEnum} (layer {layer})."); return; }
        
        while (animator.GetCurrentAnimatorStateInfo(layer).fullPathHash != hash)
            await Awaitable.NextFrameAsync();
        
        while(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
            await Awaitable.NextFrameAsync();
        
        onEnd?.Invoke();
    }
    
    public bool PlayWeightSet(
        TAnimEnum animEnum,
        float initialWeight,
        float endWeight,
        int layer = 0,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        animator.SetLayerWeight(layer, initialWeight);
        if(!PlayThenOnEnd(animEnum, SetWeightTo1, layer, normalizedTime)) return false;
        return true;
        void SetWeightTo1() => lclAnimator.SetLayerWeight(layer, endWeight);
    }
    
    public bool PlayWeightSet(
        TAnimEnum animEnum,
        float weight,
        int layer = 0,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        animator.SetLayerWeight(layer, weight);
        if(!Play(animEnum, layer, normalizedTime)) return false;
        return true;
    }
    
    public bool PlayWeightSetOnEnd(
        TAnimEnum animEnum,
        float initialWeight,
        float endWeight,
        Action onEnd,
        int layer = 0,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        Action lclOnEnd = onEnd;
        animator.SetLayerWeight(layer, initialWeight);
        if(!PlayThenOnEnd(animEnum, SetWeightTo1, layer, normalizedTime)) return false;
        return true;
        void SetWeightTo1() {lclAnimator.SetLayerWeight(layer, endWeight); lclOnEnd?.Invoke();}
    }

    

    public static implicit operator Animator(AnimHandle<TAnimEnum> animHandle) => animHandle.animator;
}
