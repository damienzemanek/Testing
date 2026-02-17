using System;
using System.Collections;
using EMILtools.Core;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.LoadSceneMode;
using static UnityEngine.SceneManagement.SceneManager;

public class MapBounds : MonoBehaviour, ITimerUser
{
    public BoolEventChannel outOfBounds;
    public string targetTag;

    public GameObject playerExplEffect;
    public GameObject player;
    
    public Ref<float> boundsMaxTime;
    public CountdownTimer boundsTimter;

    void Awake()
    {
        boundsTimter = new CountdownTimer(boundsMaxTime);
        this.InitTimer(boundsTimter, true);

        boundsTimter.OnTimerStop.Add(() => StartCoroutine(Lose()));
    }

    IEnumerator Lose()
    {
        playerExplEffect.SetActive(true);
        playerExplEffect.transform.parent = null;
        player.SetActive(false);
        yield return new WaitForSeconds(2);
        LoadSceneAsync(0, LoadSceneMode.Single).completed += _ =>
        {
            LoadSceneAsync(1, Additive);
            outOfBounds.Invoke(false);
        };
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        outOfBounds.Invoke(true);
        boundsTimter.Restart();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        outOfBounds.Invoke(false);
        boundsTimter.Restart();
        boundsTimter.Pause();
    }
}
