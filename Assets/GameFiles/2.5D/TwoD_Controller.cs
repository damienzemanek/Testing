using System;
using System.Collections.Generic;
using EMILtools.Core;
using EMILtools.Signals;
using UnityEngine;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Timers.TimerUtility;

public class TwoD_Controller : ControlledMonoFacade<TwoD_Controller, TwoD_Functionality, TwoD_Config, TwoD_Blackboard, TwoD_InputReader>
    , IStatUser, ITimerUser
{
    public Dictionary<Type, ModifierExtensions.IStat> Stats { get; set; }
    
    

    private void Awake()
    {
        InitializeFacade();
        this.InitializeTimers((Blackboard.jumpDelay, true), 
                              (Blackboard.moveDecay,true),
                              (Blackboard.turnSlowdown,true));
        
        Blackboard.rb.maxLinearVelocity = Config.move.maxVelMagnitude;
        Blackboard.rb.maxAngularVelocity = Config.move.maxVelMagnitude;
        
        Input.mouseZoneGuarder = new SimpleGuarderMutable(("Mantled", () => Blackboard.isMantled));
    }

    protected override void Update()
    {
        base.Update();
        if(!Input.mouseZoneGuarder) Input.mouseZones.CheckAllZones(Input.mouse);

    }

    private void OnEnable()
    {
        Functionality.Bind();
    }

    private void OnDisable()
    {
        Functionality.Unbind();
    }
}