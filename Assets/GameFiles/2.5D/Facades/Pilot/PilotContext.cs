using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public struct PilotContext : IModuleUsabableContext, ITwoD_Context
{
    public ITwoD_Blackboard.LookDir dir { get; set; }
}
