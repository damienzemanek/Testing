using UnityEngine;

public class SoundAdaptor : MonoBehaviour
{
    public void PlayOneShot(AudioClip clip)
    {
        SoundManager.Instance.PlayOneShotEasy(clip);
    }
}