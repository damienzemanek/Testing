using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static TwoD_SharedModules;

public class EnemyOneFunctionality : Functionalities<EnemyOneController>
{
    protected override void AddModulesHere() 
    {
       AddModule(new SightModule(facade.Actions.SeeTarget, facade));
       AddModule(new FaceDirectionModule<EnemyOneController>(facade.Actions.FaceDirection, facade));
    }


    public class SightModule : InputPressedModuleFacade<bool, EnemyOneController>
    {
        public SightModule(PersistentAction<bool> action, EnemyOneController facade) : base(action, facade) { }

        protected override void Awake() => facade.Blackboard.volleySpawner.canFire = false;

        protected override void OnPress(bool canSeeTarget)
        {
            Debug.Log($"[SightModule] Can see target: {canSeeTarget}");
            if(canSeeTarget) facade.Blackboard.volleySpawner.canFire = true;
            else facade.Blackboard.volleySpawner.canFire = false;
        }
    }
    
}