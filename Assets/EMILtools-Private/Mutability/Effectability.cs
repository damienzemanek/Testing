using System;
using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Object = UnityEngine.Object;


public static class Effectability
{
    public static void UseEffectsAll(this EffectUser[] effects)
    {
        foreach (EffectUser effect in effects)
            effect.UseEffect();
    }

    public static void UseEffectsAllWithDelayInbetween(this EffectUser[] effects, MonoBehaviour host, float delay) => host.StartCoroutine(C_UseEffectsAllWithDelayInbetween(effects, delay));
    public static IEnumerator C_UseEffectsAllWithDelayInbetween(this EffectUser[] effects, float delay)
    {
        foreach (EffectUser effect in effects)
        {
            effect.UseEffect();
            yield return new WaitForSeconds(delay);
        }
    }


    /// <summary>
    /// Intended for ONE particle system each. with ONE owner
    /// </summary>
    [Serializable]
    public struct EffectUser
    {
        [SerializeField] ParticleSystem effect;
        [SerializeField] bool looping;
        [SerializeField] bool audio;
        [SerializeField] [ShowIf("@!looping")] float _effectLength;
        [SerializeField] float _delay;
        [ShowIf("audio")][SerializeField] AudioSource source;
        [ShowIf("audio")][SerializeField] AudioClip clip;
        
        public float effectLength => _effectLength;
        public float delay => _delay;

        [Button]
        public void UseEffect(System.Threading.CancellationToken token = default)
        {
            if (!effect) return;
            
            ParticleSystem e = effect;
            AudioSource _source = source;
            AudioClip _clip = clip;
            MainModule main = e.main;
            bool canPlayAudio = audio && (source) && (clip);
            main.loop = looping;
            
            if(!looping) main.duration = effectLength;

            void EffectPlay()
            {
                if (!e) return;
                if (e.isPlaying) { e.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
                if (canPlayAudio) _source.Play(_clip, oneShot: false);
                
                e.gameObject.SetActive(true);
                e.Play();
            }

            if (delay <= 0f)    EffectPlay();
            else                _ = DelayUtility.Delay(EffectPlay, delay, token);


        }
    }

    [Serializable]
    public struct EffectObjectRagdoll
    {
        [SerializeField] GameObject[] prePlacedObjects;
        [SerializeField] float lifetime;

        public void UseEffect()
        {
            prePlacedObjects.SetAllActive(true);
            foreach (var obj in prePlacedObjects)
                Object.Destroy(obj, lifetime);
        }
    }

}

