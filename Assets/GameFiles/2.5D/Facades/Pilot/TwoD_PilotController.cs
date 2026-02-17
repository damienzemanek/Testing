using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Signals;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Timers.TimerUtility;
using static IInputSubordinate<TwoD_InputAuthority.TwoD_InputMap,TwoD_InputAuthority.Subordinates>;
using static PilotConfig;
using static TwoD_InputAuthority;

public class TwoD_PilotController : MonoFacade<
        TwoD_PilotController,
        PilotFunctionality, 
        PilotConfig, 
        PilotBlackboard,
        PilotActionMap>,
    ITimerUser,
    IInputSubordinate<TwoD_InputMap, Subordinates>
{
    
    [field: ShowInInspector] [field: NonSerialized] [field: ReadOnly] public TwoD_InputMap Input { get; set; }
    [field: PropertyOrder(-1)] [field: ShowInInspector] [field: SerializeField] public SubordinateContext context { get; set; }
    
    public TwoD_InputMap InjectInputMap() => new("Pilot");

    public void InitSubordinate()
    {
        InitializeFacade();
        Blackboard.moveDecay.Start();
        Blackboard.titanProgressTimer.Start();
    }

    public void OnAuthorityReceived()
    {
        Functionality.Bind();
        Actions.Dismount.Invoke();
    }

    public void OnAuthorityLost()
    {
        Functionality.Unbind();
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