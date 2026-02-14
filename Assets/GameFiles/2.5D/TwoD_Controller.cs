using System;
using System.Collections.Generic;
using EMILtools.Signals;
using UnityEngine;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Timers.TimerUtility;

public class TwoD_Controller : ControlledMonoFacade<TwoD_Controller, TwoD_Functionality, TwoD_Config, TwoD_Blackboard, TwoD_InputReader>
    , IStatUser, ITimerUser
{
    public Dictionary<Type, ModifierExtensions.IStat> Stats { get; set; }
    
    
}