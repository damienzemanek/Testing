using System.Collections;
using System.Collections.Generic;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class AudioStepper : MonoBehaviour
{
    [ShowInInspector] int num;
    [SerializeField] float speed = 1f;
    [SerializeField] bool linear;
    [SerializeField] bool randomize;
    [SerializeField] bool delay;
    [SerializeField] public bool blocked;
    [SerializeField, ShowIf("delay")] Vector2 delayAmount = Vector2.zero;
    bool going = false;
    #region Privates
    [SerializeField] AudioSource source;
    #endregion

    private void Awake()
    {
        if(!source) source = this.TryGetOrAdd<AudioSource>();
    }

    [SerializeField] List<AudioClip> audios;
    public void AudioStart() => AudioStart(false);
    public void AudioStart(bool once = false)
    {
        if (blocked) return;
        if (once && source.isPlaying) return;
        source.pitch = speed;
        going = true;
        StopAllCoroutines();
        if(linear && !delay) StartCoroutine(C_PlayLinear());
        else if(randomize) StartCoroutine(C_PlayRandomized());
    }

    public void AudioStop() => going = false;

    IEnumerator C_PlayLinear()
    {
        num = 0;
        while (going)
        {
            source.PlayOneShot(audios[num]);
            yield return new WaitForSeconds((audios[index: num].length / speed) + 0.01f);
            yield return new WaitForSeconds(seconds: delayAmount.Rand());

            num = (num < audios.Count - 1) ? num + 1 : 0;
        }
    }


    IEnumerator C_PlayRandomized()
    {
        while (going)
        {
            num = Random.Range(0, audios.Count - 1);

            source.PlayOneShot(audios[num]);
            yield return new WaitForSeconds((audios[num].length / speed) + 0.01f);
            yield return new WaitForSeconds(delayAmount.Rand());

            num = (num < audios.Count - 1) ? num + 1 : 0;
        }
    }

    #region Methods

    #endregion

}
