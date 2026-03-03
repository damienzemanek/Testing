using System;
using EMILtools.Core;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;

public class ScoreSource : MonoBehaviour, ITimerUser
{
    public int Score;
    public float initialGas = 100;
    public Ref<float> decayMult = 100;
    public DecayTimer gasDelay;
    public float gasGainAmount = 20f;
    public FloatEventChannel gasEvent;
    
    public LoadSceneConnector loader;
    public int loseLoaderIndx = 2;

    public bool lost = false;
    
    void Awake()
    {
        gasDelay = new DecayTimer(initialGas, decayMult);
        this.InitTimer(gasDelay, true);
    }

    void Start()
    {
        gasDelay.ResetToFull();
        gasDelay.Start();
        gasDelay.ResetToFull();
    }

    void FixedUpdate()
    {
        gasEvent.Invoke(gasDelay.Time);
        if(gasDelay.Time <= 0.5f && !lost)
        {
            lost = true;
            loader.Load(loseLoaderIndx);
        }
    }

    public void PickupGas()
    {
        gasDelay.Time += gasGainAmount;
    }
}