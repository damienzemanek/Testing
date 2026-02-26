using System;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;

public struct TitanContext : IModuleUsabableContext, ITwoD_Context
{ 
    [ShowInInspector, ReadOnly] public bool isShooting;
    [ReadOnly, ShowInInspector] public ReactiveIntercept<bool> isRunning;
    public ITwoD_Blackboard.LookDir dir { get; set; }

}