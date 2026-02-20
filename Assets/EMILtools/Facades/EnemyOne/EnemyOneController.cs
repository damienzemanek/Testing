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
    ActionMap>,
    IBoundsCheckReceiver
{
    public class ActionMap : IActionMap
    {
        [NonSerialized] public PersistentAction<bool> SeeTarget = new();
        [NonSerialized] public PersistentAction<LookDir, bool> FaceDirection = new();

    }

    protected override void Awake() 
    {
        base.Awake();
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
        Debug.Log("Recfeived");
    }
    public void OnExitBounds(Collider other) => Actions.SeeTarget.Invoke(false);
}