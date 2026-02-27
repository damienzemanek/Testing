using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static ITwoD_Blackboard;
using static Ledge;
using static PilotConfig.PilotAnims;
using static TwoD_InputAuthority;
using static TwoD_SharedModules;

public class PilotFunctionality : Functionalities<TwoD_PilotController, PilotContext>
{
    protected override void AddModulesHere()
    {
        //Layer 1
        AddModule(new LocomotionModule(facade.Input.Move, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
        AddModule(new LookModule(facade.Input.Look, facade));
        AddModule(new FaceDirectionModule<TwoD_PilotController, PilotContext>(facade.Input.FaceDirection, facade));
        AddModule(new JumpModule(facade.Input.Jump, facade));
        AddModule(new TitanCallInModule(facade.Input.CallInTitan, facade));
        AddModule(new RunModule(facade.Input.Run, facade));
        AddModule(new MountTitan(facade.Input.Interact, facade));

        //Layer 2
        AddModule(new LandModule(facade.Actions.Land, facade));
        AddModule(new ClimbModule(facade.Actions.ClimbLedge, facade));
        AddModule(new MantleModule(facade.Actions.MantleLedge, facade));
        AddModule(new DoubleJumpModule(facade.Actions.DoubleJump, facade));
        AddModule(new CameraSystemModule(facade.Actions.Dismount, facade));
        AddModule(new DismountTitanModule(facade.Actions.Dismount, facade));

        // Unbound
        AddModule(new MouseModule(facade));

    }


    public class DismountTitanModule : BoundFunctionality<TwoD_PilotController, PilotContext>
    {
        public DismountTitanModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder) => builder;

        public override bool Execute(PilotContext ctx)
        {
            facade.transform.parent = null;
            facade.Blackboard.capsuleCollider.enabled = true;
            facade.Blackboard.rb.isKinematic = false;
            facade.Blackboard.hasRequestedMount = false;
            return true;
        }
    }


    public class CameraSystemModule : BoundFunctionality<TwoD_PilotController, PilotContext>, IAPI_CameraSystem
    {
        public CameraSystemModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder) => builder;
        
        public override bool Execute(PilotContext ctx)
        {
            facade.Blackboard.camContext.CM.Target.TrackingTarget = facade.Blackboard.camFollowTransform;
            facade.Blackboard.camContext.follow.FollowOffset = facade.Config.camSettings.followOffset;
            facade.Blackboard.camContext.rotComposer.TargetOffset = facade.Config.camSettings.targetOffset;
            return true;
        }

        void IAPI_Dependant<CameraContext>.GrabDependancies(CameraContext injectedContext)
        {
            CameraContext myContext = facade.Blackboard.camContext;
            myContext.CM = injectedContext.CM;
            myContext.follow = injectedContext.follow;
            myContext.rotComposer = injectedContext.rotComposer;
            myContext.camera = injectedContext.camera;
        }
    }
    
    public class MouseModule : UnboundFunctionality<TwoD_PilotController, PilotContext>, UPDATE
    {
        public MouseModule(TwoD_PilotController facade) : base(facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => facade.Blackboard.isMantled);

        public override bool Execute(PilotContext ctx) { facade.Input.MouseInputZones.CheckAllZones(facade.Input.mouse); return true; }
        public void UpdateTick() => ExecuteTemplateCall();
    }

    public class MountTitan : BoundFunctionality<TwoD_PilotController, PilotContext>
    {
        public MountTitan(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.canMount);

        public override bool Execute(PilotContext ctx) { facade.Blackboard.hasRequestedMount = true; return true; }
    }

    public class TitanCallInModule : BoundFunctionality<TwoD_PilotController, PilotContext>
    {
        [Serializable]
        public struct Config
        { 
            [field:SerializeField] public GameObject fxCallInPrefab { get; private set; }
            [field:SerializeField] public GameObject prefab { get; private set; }
            [field:SerializeField] public float spawnVerticality { get; private set; }
            [field:SerializeField] public Ref<float> progressTime { get; private set; }
            [field:SerializeField] public Ref<float> spawnTime { get; private set; }
        }
        
        [ReadOnly] Vector3 spawnPointInAir;
        
        public TitanCallInModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.titanProgressTimer = new CountdownTimer(facade.Config.titan.progressTime);
            facade.Blackboard.spawnTitanTimer = new CountdownTimer(facade.Config.titan.spawnTime);
            facade.Blackboard.titanProgressTimer.OnTimerStop.Add(TitanReady);
            facade.Blackboard.spawnTitanTimer.OnTimerStop.Add(SpawnTitan);
            facade.InitTimer(facade.Blackboard.titanProgressTimer, true);
            facade.InitTimer(facade.Blackboard.spawnTitanTimer, true);
        }

        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => facade.Blackboard.titanReady);

        public override bool Execute(PilotContext ctx)
        {
            facade.Blackboard.posToMouse.objectToMove = GameObject.Instantiate(facade.Config.titan.fxCallInPrefab, null).transform;    
            facade.Blackboard.posToMouse.Execute();
            spawnPointInAir = facade.Blackboard.posToMouse.objectToMove.position + Vector3.up * facade.Config.titan.spawnVerticality;
            facade.Blackboard.spawnTitanTimer.Start();
            return true;
        }

        public void TitanReady()
        {
            Debug.Log("TITAN READY");
            facade.Blackboard.titanReady.Value = true;
        }
        public void SpawnTitan()
            => GameObject.Instantiate(facade.Config.titan.prefab, spawnPointInAir, Quaternion.identity);
    }

    public class DoubleJumpModule : BoundFunctionality<TwoD_PilotController, PilotContext>
    {
        public DoubleJumpModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => facade.Blackboard.hasDoubleJumped);

        public override bool Execute(PilotContext ctx)
        {
            facade.Config.animHandle.Play(facade.Blackboard.animator, Jump);
            facade.Blackboard.rb.AddForce(facade.Blackboard.phys.jumpSettings.jumpForce * facade.Blackboard.dblJumpMult, facade.Blackboard.phys.jumpSettings.forceMode);
            facade.Blackboard.hasDoubleJumped = true;
            return true;
        }
    }

    public class RunModule :
        BoundSetFunctionality<TwoD_PilotController, PilotContext, RunModule.Setter>
    {
        public class Setter : SettableTemplate<bool> {  }
        public RunModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder) => builder;

        public override bool Execute(PilotContext ctx)
        {
            facade.Blackboard.isRunning.Value = isActive;
            return true;
        }
    }

    public class ClimbModule : BoundFunctionality<TwoD_PilotController, PilotContext>, IAPI_Climb
    {
        public ClimbModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder) 
            => builder.ExitIf(_ => !facade.Blackboard.isMantled);

        public override bool Execute(PilotContext ctx)
        {
            facade.Config.animHandle.Play(facade.Blackboard.animator, Climb);
            return true;
        }

        public void CompleteClimb()
        {
            facade.Blackboard.isMantled.Value = false;
            facade.Blackboard.rb.isKinematic = false;
            facade.Config.animHandle.Play(facade.Blackboard.animator, LocomotionFwd);
            float offset = facade.Config.move.mantleXOffset;
            if(facade.Blackboard.ledgeData.dir == LookDir.Right) offset *= -1;
            facade.transform.position = facade.Blackboard.ledgeData.point.position.With(
                x: facade.Blackboard.ledgeData.point.position.x - offset);
        }
    }

    public class MantleModule : 
        BoundFunctionality<TwoD_PilotController, PilotContext>, 
        IAPI_Mantler
    {
        public MantleModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.canMantle)
                      .ExitIf(_ => facade.Blackboard.ledgeData.dir != facade.Blackboard.facingDir);

        public override bool Execute(PilotContext ctx)
        {
            Debug.Log("MANTLING");
            facade.Blackboard.isMantled.Value = true;
            facade.Blackboard.rb.isKinematic = true;
            float offset = facade.Config.move.mantleXOffset;
            if(facade.Blackboard.ledgeData.dir == LookDir.Right) offset *= -1;
            facade.transform.position = facade.transform.position.With(
                y: facade.Blackboard.ledgeData.point.position.y - facade.Blackboard.playerHeight, 
                x: facade.Blackboard.ledgeData.point.position.x + offset);
            facade.Config.animHandle.Play(facade.Blackboard.animator, Mantle);
            return true;
        }

        public void CanMantleLedge(LedgeData data)
        {
            facade.Blackboard.canMantle.Value = true;
            facade.Blackboard.ledgeData = data;
            Debug.Log("CAN MANTLE");
        }

        public void CantMantleLedge() => facade.Blackboard.canMantle.Value = false;
    }

    public class LandModule : 
        BoundFunctionality<TwoD_PilotController, PilotContext>
    {

        protected override void Awake()
         => facade.Blackboard.phys.isGrounded.SimpleReactions.Add(facade.Actions.Land.Invoke);
        
        public LandModule(PersistentAction _action, TwoD_PilotController facade) : base(_action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => {
                bool isBlocked = !facade.Blackboard.phys.isGrounded.Value;
                Debug.Log($"[LandModule] Pipeline Check: landed={facade.Blackboard.phys.isGrounded.Value}, isBlocked={isBlocked}");
                return isBlocked;
            });

        [Button]
        public override bool Execute(PilotContext ctx)
        {
            Debug.Log("Landed");
            facade.Blackboard.jumpDelay.Start();
            facade.Config.animHandle.Play(facade.Blackboard.animator, Land);
            facade.Blackboard.hasJumped.Value = false;
            facade.Blackboard.hasDoubleJumped = false;
            return true;
            
        }
    }

    public class JumpModule : BoundFunctionality<TwoD_PilotController, PilotContext>
    {
        [Serializable]
        public struct Config
        {
            [field:SerializeField] public Ref<float> delay { get; private set; }
        }
        
        public JumpModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.jumpDelay = new CountdownTimer(facade.Config.jump.delay);
            facade.InitTimer(facade.Blackboard.jumpDelay, true);
        }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => facade.Blackboard.isMantled, new Callback(() => facade.Actions.ClimbLedge.Invoke()))
                      .ExitIf(_ => facade.Blackboard.canMantle, new Callback(() => facade.Actions.MantleLedge.Invoke()))
                      .ExitIf(_ => facade.Blackboard.hasJumped, new Callback(() => facade.Actions.DoubleJump.Invoke()))
                      .ExitIf(_ => facade.Blackboard.jumpOnCooldown)
                      .ExitIf(_ => !facade.Blackboard.phys.isGrounded);

        public override bool Execute(PilotContext ctx)
        {
            facade.Config.animHandle.Play(facade.Blackboard.animator, Jump);
            PhysEX.Jump(facade.Blackboard.rb, facade.Blackboard.phys.jumpSettings);
            facade.Blackboard.hasJumped.Value = true;
            return true;
        }
    }
    

    public class LookModule : 
        BoundSetFunctionality<TwoD_PilotController, PilotContext, LookModule.Setter>, 
        LATEUPDATE
    {
        public class Setter : SettableTemplate<bool> { }
        public LookModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => facade.Blackboard.isMantled);

        public override bool Execute(PilotContext ctx) { facade.Blackboard.mouseLook.Execute(); return true; }

        public void LateTick() => ExecuteTemplateCall();
    }
    

    public class ShootModule :
        BoundSetFunctionality<TwoD_PilotController, PilotContext, ShootModule.Setter>, 
        FIXEDUPDATE
    {
        public class Setter : SettableTemplate<bool> {  }
        public ShootModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade) { }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder)
            => builder.ExitIf(_ => !isActive, new Callback(AnimateBackToIdle))
                      .ExitIf(_ => facade.Blackboard.isMantled);
        
        public override bool Execute(PilotContext ctx)
        {
            facade.StartCoroutine(ShootImplementation());
            return true;

            IEnumerator ShootImplementation()
            {
                facade.Blackboard.bulletSpawner.targetPosition = facade.Blackboard.mouseLook.core.contactPoint;
                if (facade.Blackboard.bulletSpawner.fireTimer.isRunning) yield break;
                facade.Config.animHandle.Play(facade.Blackboard.animator, Shoot, layer: 1, normalizedTime: 0);
                yield return null;
                facade.Blackboard.bulletSpawner.Spawn();
            }
        }
        
        void AnimateBackToIdle() => facade.Config.animHandle.Play(facade.Blackboard.animator, UpperBodyIdle, layer: 1);

        public void FixedTick() => ExecuteTemplateCall();
    }
    
    
    public class LocomotionModule : 
        BoundSetFunctionality<TwoD_PilotController, PilotContext, LocomotionModule.Setter>, 
        FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            public float moveForce;
            public ForceMode forceMode;
            public Ref<float> decayScalar;
            public float mantleXOffset;
            public float mantleDelay;
            public float maxVelMagnitude;
            public float maxSpeed; // run speed
            public float walkAlphaMax;
            public float speedStep;
            public float moveJitterTolerance;
            public Ref<float> runAlphaMax; // Should be greater than the greatest blend tree value to avoid jitter
            [field: SerializeField] public Ref<float> slowdownTime { get; private set; }

            [Button]
            void Init() => decayScalar = new Ref<float>(1);

        }

        public class Setter : SettableTemplate<bool, Vector2>
        {
            public Vector2 movement => unnamedStoredValue2;
        }

        public LocomotionModule(PersistentAction<bool, Vector2> action, TwoD_PilotController facade) : base(action, facade) { }

        Config cfg => facade.Config.move;

        protected override void Awake()
        {
            facade.Blackboard.moveDecay = new DecayTimer(facade.Config.move.runAlphaMax, facade.Config.move.decayScalar);
            facade.Blackboard.turnSlowdown = new CountdownTimer(facade.Config.move.slowdownTime);

            facade.Blackboard.rb.maxLinearVelocity = facade.Config.move.maxVelMagnitude;
            facade.Blackboard.rb.maxAngularVelocity = facade.Config.move.maxVelMagnitude;
            
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.InitTimer(facade.Blackboard.turnSlowdown, true);
        }
        public override PipelineBuilder<PilotContext> InjectSteps(PipelineBuilder<PilotContext> builder) => builder;

        public override bool Execute(PilotContext ctx)
        {
            if (!facade.Blackboard.isRunning) Walk();
            else Run();
            Move(SetContext.movement);
            return true;
            
            void Walk()
            {
                if(facade.Blackboard.speedAlpha < cfg.walkAlphaMax) facade.Blackboard.speedAlpha += facade.Config.move.speedStep;
                facade.Blackboard.speedAlpha = ToleranceSet(facade.Blackboard.speedAlpha, cfg.walkAlphaMax, facade.Config.move.moveJitterTolerance);
            }
            void Run()
            {
                if (facade.Blackboard.speedAlpha > cfg.runAlphaMax)
                    facade.Blackboard.speedAlpha = cfg.runAlphaMax;
                else if(facade.Blackboard.speedAlpha < cfg.runAlphaMax) 
                    facade.Blackboard.speedAlpha += facade.Config.move.speedStep;
            }
            
            void Move(Vector2 move)
            {
                if (move.x == 0) return;
                LookDir prevMoveDir = facade.Blackboard.moveDir;

                Vector3 dir = new Vector3(move.x, 0, 0);
                facade.Blackboard.moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
                facade.Config.animHandle.Play(facade.Blackboard.animator, LocomotionFwd, layer: 1, normalizedTime: 0);
                ApplyMoveForce(dir);

                if (prevMoveDir != facade.Blackboard.moveDir)
                    facade.Blackboard.turnSlowdown.Restart();
                
                
                void ApplyMoveForce(Vector3 dir)
                {
                    float runSpeedIncludingDecay = (facade.Blackboard.speedAlpha > cfg.walkAlphaMax ? cfg.maxSpeed : cfg.moveForce);
                    float actualSpeed = facade.Blackboard.isRunning ? runSpeedIncludingDecay : cfg.moveForce;
                    if (facade.Blackboard.turnSlowdown.isRunning) actualSpeed *= facade.Blackboard.turnSlowDown.Eval(facade.Blackboard.phys.isGrounded, facade.Blackboard.turnSlowdown.Progress);
                    if (!facade.Blackboard.phys.isGrounded) actualSpeed *= facade.Blackboard.phys.fallSettings.inAirMoveScalar;
                    facade.Blackboard.rb.AddForce(dir * actualSpeed, cfg.forceMode);
                }
            }
        }
        
        public void FixedTick() => ExecuteTemplateCall();
    }
}