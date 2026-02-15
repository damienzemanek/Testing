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
using static TwoD_Config;
using static TwoD_InputAuthority;

public class TwoD_PilotController : MonoFacade<
        TwoD_PilotController,
        TwoD_Functionality, 
        TwoD_Config, 
        TwoD_Blackboard,
        PilotActionMap>,
    ITimerUser,
    IInputSubordinate<TwoD_InputMap, Subordinates>,
    IInitializable
{
    
    [field: ShowInInspector] [field: SerializeField] [field: ReadOnly]  public TwoD_InputMap Input { get; set; }
    [field: ShowInInspector] [field: SerializeField] public SubordinateContext subordinateContext { get; set; }

    void Awake() => StartCoroutine(InitWait());
    IEnumerator InitWait() { yield return null; Init(); }

    public void Init()
    {
        subordinateContext.RegisterWithAuthority();
        subordinateContext.RequestAuthority();
        InitializeFacade();
        Blackboard.rb.maxLinearVelocity = Config.move.maxVelMagnitude;
        Blackboard.rb.maxAngularVelocity = Config.move.maxVelMagnitude;
        Functionality.Bind();
        
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