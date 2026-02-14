using System;
using System.Collections.Generic;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Signals;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Timers.TimerUtility;
using static TwoD_Config;

public class TwoD_Controller : ControlledMonoFacade<
        TwoD_Controller,
        TwoD_Functionality, 
        TwoD_Config, 
        TwoD_Blackboard,
        TwoD_InputReader>,
    IStatUser,
    ITimerUser
{
    public Dictionary<Type, ModifierExtensions.IStat> Stats { get; set; }
    
    

    private void Awake()
    {
        InitializeFacade();

        Blackboard.jumpDelay = new CountdownTimer(0.1f);
        Blackboard.moveDecay = new DecayTimer(2.2f, 2.5f);
        Blackboard.turnSlowdown = new CountdownTimer(0.5f);
        
        this.InitializeTimers((Blackboard.jumpDelay, true), 
                              (Blackboard.moveDecay,true),
                              (Blackboard.turnSlowdown,true));
        
        Blackboard.rb.maxLinearVelocity = Config.move.maxVelMagnitude;
        Blackboard.rb.maxAngularVelocity = Config.move.maxVelMagnitude;
        
        Input.mouseZoneGuarder = new LazyGuarderMutable(
            (new LazyGuard(Blackboard.isMantled.SimpleReactions, () => Blackboard.isMantled, "Is Mantled")));
    }

    void Start()
    {
        Blackboard.moveDecay.Start();
    }

    protected override void Update()
    {
        base.Update();
        if(!Input.mouseZoneGuarder) Input.mouseZones.CheckAllZones(Input.mouse);
        if(Blackboard.animController.state == AnimState.Locomotion)
            Blackboard.animController.UpdateLocomotion(Blackboard.facingDir, Blackboard.moveDir, Blackboard.speedAlpha);
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