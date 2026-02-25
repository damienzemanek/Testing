using System;
using System.Collections;
using EMILtools.Core;
using EMILtools.Timers;
using Sirenix.OdinInspector;
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

    public int frontEndMenuIndx = 1;

    void Awake()
    {
        boundsTimter = new CountdownTimer(boundsMaxTime);
        this.InitTimer(boundsTimter, true);
        boundsTimter.OnTimerStop.Add(Lose);
    }

    [Button]
    void Lose() => StartCoroutine(C_Lose());
    IEnumerator C_Lose()
    {
        playerExplEffect.SetActive(true);
        playerExplEffect.transform.parent = null;
        player.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        var loader = FindAnyObjectByType<LoadScene>();
        if (loader == null) yield break;
        loader.LoadSceneFadeScreenToOpaque(frontEndMenuIndx);
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
