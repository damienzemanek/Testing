using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class Collect : MonoBehaviour
{
    public enum CollectSounds { Collect }
    public AudioClip collectSoundClip;
    
    [Required] public IntEventChannel collectChannel;

    private void OnEnable()
    {
        SoundManager.CacheAudioClip(CollectSounds.Collect, collectSoundClip);
        EnumEventSystem<CollectSounds>.Add(SoundManager.Instance.PlayOneShotRequest(CollectSounds.Collect), CollectSounds.Collect);
        Debug.Log("enabled");
    }
    
    private void OnDisable()
    {
        EnumEventSystem<CollectSounds>.Remove(SoundManager.Instance.PlayOneShotRequest(CollectSounds.Collect), CollectSounds.Collect);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.Has(out ScoreSource ss)) return;
        ss.Score++;
        collectChannel.Invoke(ss.Score);
        EnumEventSystem<CollectSounds>.Raise(CollectSounds.Collect);
        Destroy(gameObject);
    }
}