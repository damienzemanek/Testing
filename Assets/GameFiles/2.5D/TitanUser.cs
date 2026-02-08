using System;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Timers.TimerUtility;

public class TitanUser : MonoBehaviour, ITimerUser
{
    [SerializeField] TwoD_InputReader input;
    [SerializeField] PositionToMouseWorldSpace posToMouse;
    [SerializeField] GameObject effectCallInPrefab;
    [SerializeField] GameObject titanPrefab;
    [SerializeField] float spawnVerticality = 100f;
    [SerializeField, ReadOnly] Vector3 spawnPointInAir; 
    
    public bool titanReady;
    public CountdownTimer progressTimer = new CountdownTimer(100f);
    public CountdownTimer spawnTitanTimer = new CountdownTimer(5f);

    [Range(0, 1)]
    public float progress;


    void Awake()
    {
        input.CallInTitan += HandleCallInTitan;

        
        progressTimer.OnTimerStop.Add(TitanReady);
        spawnTitanTimer.OnTimerStop.Add(HandleSpawnTitan);
        
        this.InitializeTimers((progressTimer, true),
                             (spawnTitanTimer, true));
        
    }

    void Start()
    {
        progressTimer.Start();
    }

    [Button]
    void AddProgg(float v) => progressTimer.Time -= v;

    void HandleCallInTitan()
    {
        if (!titanReady) return;
        posToMouse.objectToMove = Instantiate(effectCallInPrefab, null).transform;    
        posToMouse.Execute();
        spawnPointInAir = posToMouse.objectToMove.position + Vector3.up * spawnVerticality;
        spawnTitanTimer.Start();
    }

    void HandleSpawnTitan()
    {
        GameObject.Instantiate(titanPrefab, spawnPointInAir, Quaternion.identity);
    }

    void TitanReady() => titanReady = true;
}
