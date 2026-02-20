using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static EnemyOneBlackboard;
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
        protected override void OnPress(Transform t) => facade.Blackboard.trackingTarget = t;
    }

    public class AimAtTarget : UnboundFunctionalityModuleFacade<EnemyOneController>, LATEUPDATE
    {
        public AimAtTarget(EnemyOneController facade) : base(facade, true) { }

        protected override void Awake() => executeGuarder.Add
            (new ActionGuard(() => !facade.Blackboard.volleySpawner.canFire, "Can't Fire"));
        
        public override void Execute()
        {
            Vector3 lookAtLoc = facade.Blackboard.trackingTarget.position + facade.Blackboard.aimOffset;
            var aimPivot = facade.Blackboard.aimPivot;
            aimPivot.LookAt(lookAtLoc);
            Vector3 lockedEuler = aimPivot.eulerAngles;
            lockedEuler.z = 0;
            lockedEuler.y = 0;
            aimPivot.localEulerAngles = lockedEuler;
        }

        public void LateTick(float dt) => ExecuteTemplateCall(dt);
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
            
            if(targX > myX) facade.Actions.FaceDirection.Invoke(ITwoD_Blackboard.LookDir.Left, true);
            else if(targX < myX) facade.Actions.FaceDirection.Invoke(ITwoD_Blackboard.LookDir.Right, true);
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class SightModule : InputPressedModuleFacade<bool, EnemyOneController>
    {
        public SightModule(PersistentAction<bool> action, EnemyOneController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.volleySpawner.canFire = false;
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn = new PersistentAction();
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn.Add(ShootStateChange);
            facade.Blackboard.volleySpawner.onVolleyEnd += VolleyEnd;
        }

        void ShootStateChange()
        {
            Debug.Log("A");
            facade.Blackboard.animState = AnimState.Shoot;
            facade.Blackboard.anims.Play(AnimState.Shoot, normalizedTime: 0f);
        }
        void VolleyEnd() { if(facade.Blackboard.animState == AnimState.Idle) facade.Blackboard.animState = AnimState.Aim; }

        protected override void OnPress(bool canSeeTarget)
        {
            if (canSeeTarget)
            {
                facade.Blackboard.volleySpawner.canFire = true;
                VolleyEnd();
            }
            else
            {
                facade.Blackboard.volleySpawner.canFire = false;
                facade.Blackboard.animState = AnimState.Idle;
            }
            facade.Blackboard.anims.Play(facade.Blackboard.animState);
        }
    }
    
}