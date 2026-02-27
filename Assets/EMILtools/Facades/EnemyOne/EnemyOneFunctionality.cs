using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EnemyOneBlackboard;
using static EnemyOneConfig.EnemyOneAnims;
using static TwoD_SharedModules;

public class EnemyOneFunctionality : Functionalities<EnemyOneController, EnemyOneContext>
{
    protected override void AddModulesHere() 
    {
       AddModule(new SightModule(facade.Actions.SeeTarget, facade));
       // AddModule(new FaceDirectionModule<EnemyOneController, EnemyOneContext>(facade.Actions.FaceDirection, facade));
       // AddModule(new TrackTarget(facade.Actions.TrackTarget, facade));
       // AddModule(new WhichDirectionIsTargetIn(facade));
       // AddModule(new AimAtTarget(facade));
    }

    public class TrackTarget : UnboundFunctionality<EnemyOneController, EnemyOneContext>
    {
        readonly PersistentAction<Transform> trackAction;
        public TrackTarget(PersistentAction<Transform> action, EnemyOneController facade) : base(facade) 
        {
            trackAction = action;
        }
        
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder) => builder;
        public override bool ExecutionImplementation(EnemyOneContext ctx) => true;

        public void Track(Transform t) => facade.Blackboard.trackingTarget = t;
        
        public override void SetupModule()
        {
            base.SetupModule();
            trackAction.Add(Track);
        }
        // Ideally we should have an Unbind in UnboundFunctionality but we don't.
        // For EnemyOne, it's likely persistent.
    }

    public class AimAtTarget : UnboundFunctionality<EnemyOneController, EnemyOneContext>, LATE_UPDATE
    {
        public AimAtTarget(EnemyOneController facade) : base(facade) { }
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.volleySpawner.canFire);
        
        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            if (facade.Blackboard.trackingTarget == null) return false;
            
            Vector3 lookAtLoc = facade.Blackboard.trackingTarget.position + facade.Blackboard.aimOffset;
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
            => builder.ExitIf(_ => !facade.Blackboard.volleySpawner.canFire);

        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            if (facade.Blackboard.trackingTarget == null) return false;

            float targX = facade.Blackboard.trackingTarget.position.x;
            float myX = facade.transform.position.x;
            
            if(targX > myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Left);
            else if(targX < myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Right);
            return true;
        }

        public void OnUpdateTick() => Execute();
    }

    public class SightModule :
        BoundSetFunctionality<EnemyOneController, EnemyOneContext, SightModule.Setter>,
        ON_SET
    {
        public class Setter : SettableTemplate<bool> { }
        public SightModule(PersistentAction<bool> action, EnemyOneController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.volleySpawner.canFire = false;
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn ??= new PersistentAction();
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn.Add(OnShootAnim);
        }

        void OnShootAnim() => facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot, layer: 1, normalizedTime: 0);
        
        public override PipelineBuilder<EnemyOneContext> InjectSteps(PipelineBuilder<EnemyOneContext> builder) => builder;

        public override bool ExecutionImplementation(EnemyOneContext ctx)
        {
            Debug.Log("Sight module executed");
            bool canSeeTarget = isActive;
            if (canSeeTarget)
            {
                facade.Blackboard.volleySpawner.canFire = true;
            }
            else
            {
                facade.Blackboard.volleySpawner.canFire = false;
            }
            facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot);
            return true;
        }

        public void OnSet() => Execute();
    }
}
