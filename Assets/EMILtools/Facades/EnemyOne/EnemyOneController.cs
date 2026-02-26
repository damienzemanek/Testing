using System;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static EnemyOneController;
using static ITwoD_Blackboard;

public class EnemyOneController : MonoFacade<
    EnemyOneController, 
    EnemyOneFunctionality, 
    EnemyOneConfig, 
    EnemyOneBlackboard, 
    EnemyOneContext,
    ActionMap>,
    IBoundsCheckReceiver
{
    
    public class ActionMap : IActionMap
    {
        [NonSerialized] public PersistentAction<Transform> TrackTarget = new();
        [NonSerialized] public PersistentAction<bool> SeeTarget = new();
        [NonSerialized] public PersistentAction<bool, LookDir> FaceDirection = new();

    }

    protected void Awake() 
    {
        InitializeFacade();
    }

    void OnEnable()
    {
        Functionality.Bind();
    }

    void OnDisable()
    {
        Functionality.Unbind();
    }

    public void OnEnterBounds(Collider other)
    {
        Actions.SeeTarget.Invoke(true);
        Actions.TrackTarget.Invoke(other.transform);
    }

    public void OnExitBounds(Collider other)
    {
        Actions.SeeTarget.Invoke(false);
        Actions.TrackTarget.Invoke(null);
    }

}