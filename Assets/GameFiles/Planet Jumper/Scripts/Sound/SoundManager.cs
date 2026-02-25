using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SoundManager : PersistantSingleton<SoundManager>
{
    public AudioSource source;
    public Dictionary<Enum, AudioClip> clips = new();
    
    public AudioClip RequestClip<TEnum>(TEnum type)
        where TEnum : Enum
    {
        if (clips.TryGetValue(type, out AudioClip clip)) return clip;
        Debug.LogError($"Clip for type {typeof(TEnum)} not found");
        return clip;
    }

    public Action<TEnum> PlayOneShotRequest<TEnum>(TEnum type) where TEnum : Enum
    {
        return @enum =>
        {
            source.PlayOneShot(RequestClip(type));
            Debug.Log("Playing clip");
        };

    }
    public void PlayRequest() => source.Play();

    public static void CacheAudioClip<TEnum>(TEnum type, AudioClip clip) where TEnum : Enum
    {
        Instance.clips[type] = clip; 
        Debug.Log($"Cached clip for type {typeof(TEnum)}");
    }
}
