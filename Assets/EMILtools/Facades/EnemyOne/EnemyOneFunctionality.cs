using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static EnemyOneBlackboard;
using static TwoD_SharedModules;

public class EnemyOneFunctionality : Functionalities<EnemyOneController, EnemyOneContext>
{
    protected override void AddModulesHere() 
    {
       AddModule(new SightModule(facade.Actions.SeeTarget, facade));
       AddModule(new FaceDirectionModule<EnemyOneController, EnemyOneContext>(facade.Actions.FaceDirection, facade));
       AddModule(new TrackTarget(facade.Actions.TrackTarget, facade));
       AddModule(new WhichDirectionIsTargetIn(facade));
       AddModule(new AimAtTarget(facade));
    }

    public class TrackTarget : UnboundFunctionality<EnemyOneController, EnemyOneContext>
    {
        readonly PersistentAction<Transform> trackAction;
        public TrackTarget(PersistentAction<Transform> action, EnemyOneController facade) : base(facade) 
        {
            trackAction = action;
        }
        
        public override int injectAmountOfAddedSteps => 0;
        public override PipelineBuilder<EnemyOneContext> AddPipelineStepsHere(PipelineBuilder<EnemyOneContext> builder) => builder;
        public override bool Execute(EnemyOneContext ctx) => true;

        public void Track(Transform t) => facade.Blackboard.trackingTarget = t;
        
        public override void SetupModule()
        {
            base.SetupModule();
            trackAction.Add(Track);
        }
        // Ideally we should have an Unbind in UnboundFunctionality but we don't.
        // For EnemyOne, it's likely persistent.
    }

    public class AimAtTarget : UnboundFunctionality<EnemyOneController, EnemyOneContext>, LATEUPDATE
    {
        public AimAtTarget(EnemyOneController facade) : base(facade) { }
        public override int injectAmountOfAddedSteps => 1;
        public override PipelineBuilder<EnemyOneContext> AddPipelineStepsHere(PipelineBuilder<EnemyOneContext> builder)
            => builder.AddBlockIf(_ => !facade.Blackboard.volleySpawner.canFire);
        
        public override bool Execute(EnemyOneContext ctx)
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

        public void LateTick() => ExecuteTemplateCall();
    }

    public class WhichDirectionIsTargetIn : UnboundFunctionality<EnemyOneController, EnemyOneContext>, UPDATE
    {
        public WhichDirectionIsTargetIn(EnemyOneController facade) : base(facade) { }
        public override int injectAmountOfAddedSteps => 1;
        public override PipelineBuilder<EnemyOneContext> AddPipelineStepsHere(PipelineBuilder<EnemyOneContext> builder)
            => builder.AddBlockIf(_ => !facade.Blackboard.volleySpawner.canFire);

        public override bool Execute(EnemyOneContext ctx)
        {
            if (facade.Blackboard.trackingTarget == null) return false;

            float targX = facade.Blackboard.trackingTarget.position.x;
            float myX = facade.transform.position.x;
            
            if(targX > myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Left);
            else if(targX < myX) facade.Actions.FaceDirection.Invoke(true, ITwoD_Blackboard.LookDir.Right);
            return true;
        }

        public void UpdateTick() => ExecuteTemplateCall();
    }

    public class SightModule : BoundHeldFunctionality<EnemyOneController, EnemyOneContext, SightModule.Setter>
    {
        public class Setter : SettableTemplate<bool> { }
        public SightModule(PersistentAction<bool> action, EnemyOneController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.volleySpawner.canFire = false;
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn ??= new PersistentAction();
            facade.Blackboard.volleySpawner.projSpawner.OnSpawn.Add(ShootStateChange);
            facade.Blackboard.volleySpawner.onVolleyEnd += VolleyEnd;
        }

        void ShootStateChange()
        {
            facade.Blackboard.animState = AnimState.Shoot;
            facade.Blackboard.anims.Play(facade.Blackboard.animator, AnimState.Shoot, normalizedTime: 0f);
        }
        
        void VolleyEnd() 
        { 
            if(facade.Blackboard.animState == AnimState.Idle) facade.Blackboard.animState = AnimState.Aim; 
        }

        public override int injectAmountOfAddedSteps => 0;
        public override PipelineBuilder<EnemyOneContext> AddPipelineStepsHere(PipelineBuilder<EnemyOneContext> builder) => builder;

        public override bool Execute(EnemyOneContext ctx)
        {
            bool canSeeTarget = isActive;
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
            facade.Blackboard.anims.Play(facade.Blackboard.animator, facade.Blackboard.animState);
            return true;
        }
        
        public new void Bind()
        {
            base.Bind();
            facade.Actions.SeeTarget.Add(OnSeeTargetChanged);
        }
        
        public new void Unbind()
        {
            base.Unbind();
            facade.Actions.SeeTarget.Remove(OnSeeTargetChanged);
        }

        void OnSeeTargetChanged(bool obj) => ExecuteTemplateCall();
    }
}
