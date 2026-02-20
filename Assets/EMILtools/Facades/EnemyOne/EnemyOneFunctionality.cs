using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static ITwoD_Blackboard;
using static TwoD_SharedModules;

public class EnemyOneFunctionality : Functionalities<EnemyOneController>
{
    protected override void AddModulesHere() 
    {
       AddModule(new SightModule(facade.Actions.SeeTarget, facade));
       AddModule(new FaceDirectionModule<EnemyOneController>(facade.Actions.FaceDirection, facade));
       AddModule(new TrackTarget(facade.Actions.TrackTarget, facade));
       AddModule(new WhichDirectionIsTargetIn(facade));
       AddModule(new AimAtTarget(facade));
    }

    public class TrackTarget : InputPressedModuleFacade<Transform, EnemyOneController>
    {
        public TrackTarget(PersistentAction<Transform> action, EnemyOneController facade) : base(action, facade) { }
        
        protected override void OnPress(Transform t)
        {
            facade.Blackboard.trackingTarget = t;
            if(t == null) return;
        }
    }

    public class AimAtTarget : UnboundFunctionalityModuleFacade<EnemyOneController>, UPDATE
    {
        public AimAtTarget(EnemyOneController facade) : base(facade, true) { }

        protected override void Awake() => executeGuarder.Add
            (new ActionGuard(() => !facade.Blackboard.volleySpawner.canFire, "Can't Fire"));
        
        public override void Execute()
        {
            Vector3 lookAtLoc = facade.Blackboard.trackingTarget.position + facade.Blackboard.aimOffset;
            var aimPivot = facade.Blackboard.aimPivot;
            aimPivot.LookAt(lookAtLoc);
            //Vector3 e = aimPivot.eulerAngles;
            //aimPivot.rotation = Quaternion.Euler(0f, e.y, 0f);
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class WhichDirectionIsTargetIn : UnboundFunctionalityModuleFacade<EnemyOneController>, UPDATE
    {
        public WhichDirectionIsTargetIn(EnemyOneController facade) : base(facade, true) { }

        protected override void Awake() => executeGuarder.Add
            (new ActionGuard(() => !facade.Blackboard.volleySpawner.canFire, "Can't Fire"));

        public override void Execute()
        {
            float targX = facade.Blackboard.trackingTarget.position.x;
            float myX = facade.transform.position.x;
            
            if(targX > myX) facade.Actions.FaceDirection.Invoke(LookDir.Left, true);
            else if(targX < myX) facade.Actions.FaceDirection.Invoke(LookDir.Right, true);
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class SightModule : InputPressedModuleFacade<bool, EnemyOneController>
    {
        public SightModule(PersistentAction<bool> action, EnemyOneController facade) : base(action, facade) { }

        protected override void Awake() => facade.Blackboard.volleySpawner.canFire = false;

        protected override void OnPress(bool canSeeTarget)
        {
            if(canSeeTarget) facade.Blackboard.volleySpawner.canFire = true;
            else facade.Blackboard.volleySpawner.canFire = false;
        }
    }
    
}