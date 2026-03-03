using UnityEngine;

public class MusicAdapter : MonoBehaviour
{
    public void Play(int indx)
    {
        Music.Instance.Play(indx);
    }
}