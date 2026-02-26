using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public struct EnemyOneContext : IModuleUsabableContext, ITwoD_Context
{
    public ITwoD_Blackboard.LookDir dir { get; set; }
}
