using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static EnemyOneBlackboard;
using static EnemyOneConfig.EnemyOneAnims;
using static TwoD_SharedModules;

public class EnemyOneFunctionality : Functionalities<EnemyOneController, EnemyOneContext>
{
    protected override void AddModulesHere() 
    {
       AddModule(new FaceDirectionModule<EnemyOneController, EnemyOneContext>(facade.Actions.FaceDirection, facade));
       AddModule(new TrackTarget(facade.Actions.TrackTarget, facade));
       AddModule(new WhichDirectionIsTargetIn(facade));
       AddModule(new AimAtTarget(facade));
    }

    public class TrackTarget : 
        BoundSetFunctionality<EnemyOneController, EnemyOneContext, TrackTarget.Setter>,
        ON_SET
    {
        public class Setter : SettableTemplate<bool, Transform>
        {
            [ShowInInspector] public Transform target => unnamedStoredValue2;
        }

        protected override void Awake()
        { 
            facade.Blackboard.volleySpawner.canFire = false;
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn ??= new PersistentAction();
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn.Add(OnShootAnim);
        }
        void OnShootAnim() => facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot, layer: 1, normalizedTime: 0);

        public TrackTarget(IPersistentDelegate _action, EnemyOneController facade) : base(_action, facade) { }
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder) => builder;

        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            facade.Blackboard.canSeeAndFire = isActive;
            return true;
        }
        public void OnSet()
        {
            facade.Blackboard.trackingTarget = SetContext.target;
            Execute();
        }
    }

    public class AimAtTarget : 
        UnboundFunctionality<EnemyOneController, EnemyOneContext>, 
        LATE_UPDATE
    {
        public AimAtTarget(EnemyOneController facade) : base(facade) { }
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.canSeeAndFire);
        
        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            Vector3 lookAtLoc = Vector3.zero;
            if(facade.Blackboard.trackingTarget.Has<TwoD_TitanController>())
                lookAtLoc = facade.Blackboard.trackingTarget.position + facade.Config.titanAimOffset;
            else if(facade.Blackboard.trackingTarget.Has<TwoD_PilotController>()) 
                lookAtLoc = facade.Blackboard.trackingTarget.position + facade.Config.pilotAimOffset;
            else Debug.LogError("Tracking target is neither titan nor pilot");
            var aimPivot = facade.Blackboard.aimPivot;
            aimPivot.LookAt(lookAtLoc);
            Vector3 lockedEuler = aimPivot.eulerAngles;
            lockedEuler.z = 0;
            lockedEuler.y = 0;
            aimPivot.localEulerAngles = lockedEuler;
            return true;
        }

        public void OnLateTick() => Execute();
    }

    public class WhichDirectionIsTargetIn : UnboundFunctionality<EnemyOneController, EnemyOneContext>, UPDATE
    {
        public WhichDirectionIsTargetIn(EnemyOneController facade) : base(facade) { }
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.canSeeAndFire);

        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            float targX = facade.Blackboard.trackingTarget.position.x;
            float myX = facade.transform.position.x;
            
            if(targX > myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Left);
            else if(targX < myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Right);
            return true;
        }

        public void OnUpdateTick() => Execute();
    }
    
}
