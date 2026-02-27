using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;
using static TitanConfig;
using static TitanConfig.TitanAnims;
using static TwoD_InputAuthority;
using static TwoD_SharedModules;


public class TitanFunctionality : Functionalities<TwoD_TitanController, TitanContext>
{
    protected override void AddModulesHere()
    {
        // Layer 1 -> Direct Input
        AddModule(new DismountModule(facade.Input.HoldInteract, facade));
        AddModule(new LocomotionModule(facade.Input.Move, facade));
        AddModule(new FaceDirectionModule<TwoD_TitanController, TitanContext>
                                    (facade.Input.FaceDirection, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
        //
        // // Layer 2 -> Actions
        AddModule(new CameraSystemModule(facade.Actions.Mount, facade));
        //
        // // Unbound        
        AddModule(new MountModule(facade));
        AddModule(new MouseInputZonesModule(facade));
        AddModule(new MouseLookModule(facade));
    }
    
    
    public class MouseLookModule :
        UnboundFunctionality<TwoD_TitanController, TitanContext>, 
        LATEUPDATE
    {
        public MouseLookModule(TwoD_TitanController facade) : base(facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.hasMounted);
        public override bool ExecutionImplementation(TitanContext ctx) { facade.Blackboard.mouseLook.Execute(); return true; }
        public void LateTick() => Execute();
    }
    

    public class ShootModule : 
        BoundSetFunctionality< TwoD_TitanController, TitanContext, ShootModule.Setter>, 
        UPDATE
    {
        public class Setter : SettableTemplate<bool> { }
        public ShootModule(PersistentAction<bool> action, TwoD_TitanController facade) : base(action, facade) { }
        
        protected override void Awake()
        {
            facade.Blackboard.bulletSpawner.OnSpawn = new PersistentAction();
            facade.Blackboard.bulletSpawner.OnSpawn.Add(AnimateShoot);
        }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder)
            => builder.ExitIf(_ => !isActive, new Callback(AnimateBackToIdle))
                      .ExitIf(_ => facade.Blackboard.bulletSpawner.fireTimer.isRunning);

        public override bool ExecutionImplementation(TitanContext ctx)
        {
            facade.Blackboard.bulletSpawner.targetPosition = facade.Blackboard.mouseLook.core.contactPoint;
            facade.Blackboard.bulletSpawner.Spawn();
            facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot, layer: 1, normalizedTime: 0f);
            return true;
        }

        public void UpdateTick() => Execute();
        
        void AnimateBackToIdle()
        {
            facade.Config.animHandle.Play(facade.Blackboard.animator, LocomotionFwd);
            facade.Config.animHandle.Play(facade.Blackboard.animator, UpperBodyIdle);
        }
        void AnimateShoot() => facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot, layer: 1, normalizedTime: 0f);
    }
    
    
    
    public class MouseInputZonesModule : 
        UnboundFunctionality<TwoD_TitanController, TitanContext>,
        UPDATE,
        IAPI_Dependant<MouseInputZonesModule.MouseModuleContext>
    {
        public struct MouseModuleContext 
            { public Camera cam; public MouseModuleContext(Camera cam) => this.cam = cam; }
        
        public MouseInputZonesModule(TwoD_TitanController facade) : base(facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.hasMounted);

        public override bool ExecutionImplementation(TitanContext ctx) {
            facade.Input.MouseInputZones.CheckAllZones(facade.Input.mouse);
            return true; }

        public void UpdateTick() => Execute();

        void IAPI_Dependant<MouseModuleContext>.GrabDependancies(MouseModuleContext injectedContext)
            => facade.Blackboard.mouseLook.cam = injectedContext.cam;
    }
    
    
    
    
    public class LocomotionModule : 
        BoundSetFunctionality< TwoD_TitanController, TitanContext, LocomotionModule.Setter>, 
        UPDATE
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float speed { get; private set; }
            [field: SerializeField] public ForceMode forceMode { get; private set; }
            [field: SerializeField] public float runAlphaMax { get; private set; }
            [field: SerializeField] public Ref<float> decayScalar { get; private set; }
            [field: SerializeField] public float slowdownTime { get; private set; }
            [field: SerializeField] public float maxVelMagnitude { get; private set; }
    
        }
        public class Setter : SettableTemplate<bool, Vector2> 
            { [ShowInInspector] public Vector2 moveVector => unnamedStoredValue2; }
        
        public LocomotionModule(PersistentAction<bool, Vector2> action, TwoD_TitanController facade) : base(action, facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder) => builder;

        protected override void Awake()
        {
            facade.Blackboard.moveDecay = new DecayTimer(facade.Config.move.runAlphaMax, facade.Config.move.decayScalar); 
            facade.Blackboard.turnSlowdown = new CountdownTimer(facade.Config.move.slowdownTime);
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.InitTimer(facade.Blackboard.turnSlowdown, true);
            
            facade.Blackboard.rb.maxLinearVelocity = facade.Config.move.maxVelMagnitude;
            facade.Blackboard.rb.maxAngularVelocity = facade.Config.move.maxVelMagnitude;
        }
        
        public override bool ExecutionImplementation(TitanContext ctx)
        {
            Walk();
            Move(SetContext.moveVector);
            
            
            void Walk()
            {
                if(facade.Blackboard.speedAlpha < 1f) facade.Blackboard.speedAlpha += 0.1f;
                facade.Blackboard.speedAlpha = NumEX.ToleranceSet(facade.Blackboard.speedAlpha, 1, 0.2f);
            }
    
            void Move(Vector2 move)
            {
                if (move.x == 0) return;
                LookDir prevMoveDir = facade.Blackboard.moveDir;
    
                Vector3 dir = move.x < 0 ? Vector3.left : Vector3.right;
                facade.Blackboard.moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
                ApplyMoveForce(dir);
            }
            
            void ApplyMoveForce(Vector3 dir)
            {
                // float runSpeedIncludingDecay = (speedAlpha > WALK_ALPHA_MAX ? movement.maxSpeed : movement.moveForce);
                // float actualSpeed = isRunning ? runSpeedIncludingDecay : movement.moveForce;
                // if (turnSlowdown.isRunning) actualSpeed *= turnSlowDown.Eval(phys.isGrounded, turnSlowdown.Progress);
                // if (!phys.isGrounded) actualSpeed *= phys.fallSettings.inAirMoveScalar;
                facade.Blackboard.rb.AddForce(dir * facade.Config.move.speed, facade.Config.move.forceMode);
            }
            return true;
        }

        public void UpdateTick() => Execute();
    }
    
    
    public class DismountModule :
        BoundFunctionality<TwoD_TitanController, TitanContext>
    {
        public DismountModule(PersistentAction action, TwoD_TitanController facade) : base(action, facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder)
            => builder;

        public override bool ExecutionImplementation(TitanContext ctx) {
            facade.StartCoroutine(DismountSequence(ctx));
            return true; }
        
        IEnumerator DismountSequence(TitanContext ctx)
        {
            IInputSubordinate<TwoD_InputMap, Subordinates> thisTitan = facade;
            IInputSubordinate<TwoD_InputMap, Subordinates> pilot = facade.Blackboard.myPilot;
            facade.Config.animHandle.Play(facade.Blackboard.animator, Dismount);
            
            yield return new WaitForSeconds(facade.Config.mount.duration);
            
            bool successful = pilot.RequestAuthorityFrom(thisTitan);
            if(!successful) yield break;
            
            facade.Blackboard.hasMounted = false;
            facade.Blackboard.myPilot.gameObject.SetActive(true);
            facade.Blackboard.myPilot = null;
            Debug.Log("Titan Dismount Sequence Complete");
        }
    }
    
    
    public class CameraSystemModule :
        BoundFunctionality< TwoD_TitanController, TitanContext>, 
        IAPI_CameraSystem
    {
        public CameraSystemModule(PersistentAction action, TwoD_TitanController facade) : base(action, facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder) => builder;

        public override bool ExecutionImplementation(TitanContext ctx)
        {
            facade.Blackboard.camContext.CM.Target.TrackingTarget = facade.transform;
            facade.Blackboard.camContext.follow.FollowOffset = facade.Config.camSettings.followOffset;
            facade.Blackboard.camContext.rotComposer.TargetOffset = facade.Config.camSettings.targetOffset;
            return true;
        }

        void IAPI_Dependant<CameraContext>.GrabDependancies(CameraContext injectedContext)
        {
            var myContext = facade.Blackboard.camContext;
            myContext.CM = injectedContext.CM;
            myContext.follow = injectedContext.follow;
            myContext.rotComposer = injectedContext.rotComposer;
            myContext.camera = injectedContext.camera;
        }
    }
    
    public class MountModule : 
        UnboundFunctionality<TwoD_TitanController, TitanContext>,
        IAPI_Mount
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float duration { get; private set; }   
        }
        
        public MountModule(TwoD_TitanController facade) : base(facade) { }
        public override PipelineBuilder<TitanContext> InjectSteps(PipelineBuilder<TitanContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.canMount);

        public override bool ExecutionImplementation(TitanContext ctx) 
        { facade.StartCoroutine(MountSequence(ctx)); return true; }

        public void Mount() => Execute();
        
        IEnumerator MountSequence(TitanContext ctx)
        {
            Transform playerTransform = facade.Blackboard.myMountZone.playerTransform;
            Transform mountLoc = facade.Blackboard.mountLocation;
            var pilotBB = playerTransform.Get<TwoD_PilotController>().Blackboard;
            var typedPilotBB = pilotBB as PilotBlackboard;
            var camContext = typedPilotBB.camContext;
            
            playerTransform.position = mountLoc.position; 
            playerTransform.parent = mountLoc;
            playerTransform.Get<Rigidbody>().isKinematic = true;
            playerTransform.Get<Collider>().enabled = false;
            facade.Get<AugmentPhysEX>().fallFaster = false;
            facade.Blackboard.myPilot = playerTransform.Get<TwoD_PilotController>();
            facade.GetFunctionality<IAPI_CameraSystem>().SendDependencies(camContext);
            facade.GetFunctionality<IAPI_Dependant<MouseInputZonesModule.MouseModuleContext>>().SendDependencies(new MouseInputZonesModule.MouseModuleContext(camContext.camera));
            // Later: Remake MouseModule for this stuff
            //facade.Blackboard.mouseZoneGuarder = new SimpleGuarderMutable(("Not Looking", () => !isLooking));
            // input._lookGuarder = new SimpleGuarderMutable();
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.Config.animHandle.Play(facade.Blackboard.animator, MountFront);
        
            
            yield return new WaitForSeconds(facade.Config.mount.duration);
            
            facade.Config.animHandle.Play(facade.Blackboard.animator, LocomotionFwd);
            facade.Config.animHandle.Play(facade.Blackboard.animator, UpperBodyIdle, layer: 1);
            facade.Blackboard.moveDecay.Start();
            facade.Blackboard.hasMounted = true;
            if(facade.Blackboard.myPilot != null) facade.Blackboard.myPilot.gameObject.SetActive(false);
            facade.Actions.Mount.Invoke();
        }
    }
}
