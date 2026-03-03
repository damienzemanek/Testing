using System;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using UnityEngine;

public class Music : PersistantSingleton<Music>
{
    [Serializable]
    public struct MusicData
    {
        public MusicSounds sound;
        public AudioClip clip;
    }
    
    public enum MusicSounds { Menu, Level }
    public MusicData[] musicData = new MusicData[Enum.GetValues(typeof(MusicSounds)).Length];

    
    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < musicData.Length; i++)
            SoundManager.CacheAudioClip(musicData[i].sound, musicData[i].clip);
    }
    
    public void PlayOneShot(int index)
        => SoundManager.Instance.PlayOneShotRequest(musicData[index].sound).Invoke(musicData[index].sound);
    public void Play(int index) 
        => SoundManager.Instance.PlayRequest(musicData[index].sound).Invoke(musicData[index].sound);
    public void PlayOneShot(MusicSounds sound)
        => SoundManager.Instance.PlayOneShotRequest(sound).Invoke(sound);
    public void Play(MusicSounds sound)
        => SoundManager.Instance.PlayRequest(sound).Invoke(sound);
}
