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
    IBoundsCheckMsgReceiver
{
    
    public class ActionMap : IActionMap
    {
        [NonSerialized] public PersistentAction<bool, Transform> TrackTarget = new();
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
    
    public void OnEnterBounds(Collider collidedWith, BoundsChecker sender)
    {
        Debug.Log("entered bounds");
        Actions.TrackTarget.Invoke(true, collidedWith.transform);
    }

    public void OnExitBounds(Collider collidedWith, BoundsChecker sender)
    {
        Actions.TrackTarget.Invoke(false, null);
    }

}