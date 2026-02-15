using System;
using System.Collections.Generic;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Signals;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Timers.TimerUtility;
using static TwoD_Config;
using static TwoD_InputAuthority;

public class TwoD_PilotController : MonoFacade<
        TwoD_PilotController,
        TwoD_Functionality, 
        TwoD_Config, 
        TwoD_Blackboard,
        PilotActionMap>,
    ITimerUser,
    IControllable<TwoD_InputMap>,
    IInitializable
{
    [field: ShowInInspector] [field: NonSerialized] [field: ReadOnly]  public TwoD_InputMap Input { get; set; }
    
    public void Init()
    {
        InitializeFacade();
        Blackboard.rb.maxLinearVelocity = Config.move.maxVelMagnitude;
        Blackboard.rb.maxAngularVelocity = Config.move.maxVelMagnitude;
        Functionality.Bind();
    }
    
    void Start()
    {
        Blackboard.moveDecay.Start();
        Blackboard.titanProgressTimer.Start();
    }

    protected override void Update()
    {
        base.Update();
        if(Blackboard.animController.state == AnimState.Locomotion)
            Blackboard.animController.UpdateLocomotion(Blackboard.facingDir, Blackboard.moveDir, Blackboard.speedAlpha);
    }
    

    private void OnDisable()
    {
        Functionality.Unbind();
    }


}