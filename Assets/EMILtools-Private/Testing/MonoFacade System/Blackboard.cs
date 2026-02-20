using System;
using EMILtools_Private.Testing;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IBlackboard { }

[Serializable]
public abstract class Blackboard : IBlackboard
{

    
}

public interface ITwoD_Blackboard : IBlackboard
{
    public Transform facing { get; set; }
}
