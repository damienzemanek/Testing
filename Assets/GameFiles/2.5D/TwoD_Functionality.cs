using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static Ledge;
using static TwoD_Config;

public class TwoD_Functionality : Functionalities<TwoD_PilotController>
{
    protected override void AddModulesHere()
    {
        // Layer 1
        AddModule(new MoveModule(facade.Input.Move, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
        AddModule(new LookModule(facade.Input.Look, facade));
        AddModule(new FaceDirectionModule(facade.Input.FaceDirection, facade));
        AddModule(new JumpModule(facade.Input.Jump, facade));
        AddModule(new TitanCallInModule(facade.Input.CallInTitan, facade));
        AddModule(new RunModule(facade.Input.Run, facade));

        // Layer 2
        AddModule(new LandModule(facade.Actions.Land, facade));
        AddModule(new ClimbModule(facade.Actions.ClimbLedge, facade));
        AddModule(new MantleModule(facade.Actions.MantleLedge, facade));
        AddModule(new DoubleJumpModule(facade.Actions.DoubleJump, facade));
        AddModule(new MouseModule(facade));

    }


    public class MouseModule : UnboundFunctionalityModuleFacade<TwoD_PilotController, ActionGuarderMutable>, UPDATE
    {
        public MouseModule(TwoD_PilotController facade) : base(facade, true) { }

        protected override void Awake() => executeGuarder.Add(new LazyActionGuard<LazyFuncLite<bool>>
                                           (facade.Blackboard.isMantled.SimpleReactions, () => facade.Blackboard.isMantled, "Is Mantled"));
        public override void Execute() => facade.Input.MouseInputZones.CheckAllZones(facade.Input.mouse);
        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class TitanCallInModule : InputPressedModuleFacade<ActionGuarderMutable, TwoD_PilotController>
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
            onPressGuarder.Add(new LazyActionGuard<LazyFunc<bool>> (facade.Blackboard.titanReady.SimpleReactions, 
                () => !facade.Blackboard.titanReady, "Titan not ready"));
            
            facade.Blackboard.titanProgressTimer = new CountdownTimer(facade.Config.titan.progressTime);
            facade.Blackboard.spawnTitanTimer = new CountdownTimer(facade.Config.titan.spawnTime);
            facade.Blackboard.titanProgressTimer.OnTimerStop.Add(TitanReady);
            facade.Blackboard.spawnTitanTimer.OnTimerStop.Add(SpawnTitan);
            facade.InitTimer(facade.Blackboard.titanProgressTimer, true);
            facade.InitTimer(facade.Blackboard.spawnTitanTimer, true);
        }

        protected override void OnPress()
        {
            facade.Blackboard.posToMouse.objectToMove = GameObject.Instantiate(facade.Config.titan.fxCallInPrefab, null).transform;    
            facade.Blackboard.posToMouse.Execute();
            spawnPointInAir = facade.Blackboard.posToMouse.objectToMove.position + Vector3.up * facade.Config.titan.spawnVerticality;
            facade.Blackboard.spawnTitanTimer.Start();
        }

        public void TitanReady()
        {
            Debug.Log("TITAN READY");
            facade.Blackboard.titanReady.Value = true;
        }
        public void SpawnTitan()
            => GameObject.Instantiate(facade.Config.titan.prefab, spawnPointInAir, Quaternion.identity);

        
        
    }

    public class DoubleJumpModule : BasicFunctionalityModuleFacade<TwoD_PilotController>
    {
        public DoubleJumpModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }

        protected override void Awake() => facade.Actions.DoubleJump.Add(Execute);

        public override void Execute()
        {
            if (facade.Blackboard.hasDoubleJumped) return;
            facade.Blackboard.animController.Play(facade.Blackboard.animController.dbljump);
            facade.Blackboard.rb.AddForce(facade.Blackboard.phys.jumpSettings.jumpForce * facade.Blackboard.dblJumpMult, facade.Blackboard.phys.jumpSettings.forceMode);
            facade.Blackboard.hasDoubleJumped = true;
        }
    }

    public class RunModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_PilotController>
    {
        public RunModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade, true) { }
        protected override void OnSet() => facade.Blackboard.isRunning.Value = isActive;
        protected override void Implementation(float dt) { }
    }

    public class ClimbModule : BasicFunctionalityModuleFacade<TwoD_PilotController>, IAPI_Climb
    {
        public ClimbModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }

        protected override void Awake() => facade.Actions.ClimbLedge.Add(Execute);

        public override void Execute() => facade.Blackboard.animController.animator.CrossFade(facade.Blackboard.animController.climb, 0.1f);

        public void CompleteClimb()
        {
            facade.Blackboard.isMantled.Value = false;
            facade.Blackboard.rb.isKinematic = false;
            facade.Blackboard.animController.state = AnimState.Locomotion;
            float offset = facade.Config.move.mantleXOffset;
            if(facade.Blackboard.ledgeData.dir == LookDir.Right) offset *= -1;
            facade.transform.position = facade.Blackboard.ledgeData.point.position.With(
                x: facade.Blackboard.ledgeData.point.position.x - offset);
        }
    }

    public class MantleModule : BasicFunctionalityModuleFacade<TwoD_PilotController>, IAPI_Mantler
    {
        public MantleModule(PersistentAction action, TwoD_PilotController facade) : base(action, facade) { }

        protected override void Awake() => facade.Actions.MantleLedge.Add(Execute);

        public override void Execute()
        {
            if(facade.Blackboard.ledgeData.dir != facade.Blackboard.facingDir) return;
            
            facade.Blackboard.isMantled.Value = true;
            facade.Blackboard.rb.isKinematic = true;
            float offset = facade.Config.move.mantleXOffset;
            if(facade.Blackboard.ledgeData.dir == LookDir.Right) offset *= -1;
            facade.transform.position = facade.transform.position.With(
                y: facade.Blackboard.ledgeData.point.position.y - facade.Blackboard.playerHeight, 
                x: facade.Blackboard.ledgeData.point.position.x + offset);
            facade.Blackboard.animController.state = AnimState.Mantle;
            facade.Blackboard.animController.Play(facade.Blackboard.animController.mantle);
        }

        public void CanMantleLedge(LedgeData data)
        {
            facade.Blackboard.canMantle.Value = true;
            facade.Blackboard.ledgeData = data;
        }

        public void CantMantleLedge() => facade.Blackboard.canMantle.Value = false;
    }

    public class LandModule : BasicFunctionalityModuleFacade<bool, TwoD_PilotController, ActionGuarderMutable>
    {
        public LandModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade, false) { }

        protected override void Awake() => facade.Blackboard.phys.isGrounded.Reactions.Add(facade.Actions.Land.Invoke);

        public override void Execute(bool landed)
        {
            Debug.Log("attempted land");
            if (!landed) return;
            Debug.Log("failed land");

            facade.Blackboard.animController.state = AnimState.Locomotion;
            facade.Blackboard.jumpDelay.Start();
            facade.Blackboard.animController.Play(facade.Blackboard.animController.land);
            facade.Blackboard.hasJumped.Value = false;
            facade.Blackboard.hasDoubleJumped = false;
        }
    }

    public class JumpModule : InputPressedModuleFacade<ActionGuarderMutable, TwoD_PilotController>
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

            onPressGuarder = new ActionGuarderMutable(
                new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.isMantled.SimpleReactions, 
                                () => facade.Blackboard.isMantled, () => facade.Actions.ClimbLedge.Invoke(), "Is Mantled", "Climb"),
                
                        new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.canMantle.SimpleReactions,
                                () => facade.Blackboard.canMantle, () => facade.Actions.MantleLedge.Invoke(), "Can Mantle", "Mantle"),
                
                        new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.hasJumped.SimpleReactions,
                                () => facade.Blackboard.hasJumped, () => facade.Actions.DoubleJump.Invoke(), "Has Jumped", "Double Jump"),
                
                        new ActionGuard(() => facade.Blackboard.jumpOnCooldown, "Jump On Cooldown"),
                        new ActionGuard(() => !facade.Blackboard.phys.isGrounded, "In the Air"));
        }

        protected override void OnPress()
        {
            facade.Blackboard.animController.Play(facade.Blackboard.animController.jump);
            PhysEX.Jump(facade.Blackboard.rb, facade.Blackboard.phys.jumpSettings);
            facade.Blackboard.hasJumped.Value = true;
        }
    }

    public class FaceDirectionModule : InputHeldModuleFacade<LookDir, ActionGuarderMutable, TwoD_PilotController>, UPDATE
    {
        
        public FaceDirectionModule(PersistentAction<LookDir, bool> action, TwoD_PilotController facade) : base(action, facade, false) { }
        
        [ShowInInspector] LookDir dir;
        
        protected override void OnSetImplementation(LookDir args) => dir = args;

        protected override void Implementation(float dt)
        {
            if (dir == LookDir.Right) facade.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            if (dir == LookDir.Left) facade.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            facade.Blackboard.facingDir = dir;
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class LookModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_PilotController>, LATEUPDATE
    {
        public LookModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade, false) { }

        protected override void Awake()
        {
            executeGuarder.Add(new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.isMantled.SimpleReactions,
                () => facade.Blackboard.isMantled, "Is Mantled"));
        }


        protected override void Implementation(float dt) => facade.Blackboard.mouseLook.Execute();

        public void LateTick(float dt) => ExecuteTemplateCall(dt);
    }
    

    public class ShootModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_PilotController>, FIXEDUPDATE
    {
        public ShootModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade, true) { }

        protected override void Awake()
        {
            executeGuarder = new ActionGuarderMutable((
                    new ActionGuard(() => !isActive, AnimateBackToIdle, "Is Active", "Idle Anim")), 
                    new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.isMantled.SimpleReactions, () => facade.Blackboard.isMantled, "Is Mantled"));
            
        }
        
        protected override void Implementation(float dt)
        {
            facade.StartCoroutine(ShootImplementation());
            IEnumerator ShootImplementation()
            {
                facade.Blackboard.bulletSpawner.targetPosition = facade.Blackboard.mouseLook.core.contactPoint;
                if (facade.Blackboard.bulletSpawner.fireTimer.isRunning) yield break;
                facade.Blackboard.animController.animator.Play(facade.Blackboard.animController.shoot, layer: 1, normalizedTime: 0f);
                yield return null;
                facade.Blackboard.bulletSpawner.Spawn();
                //Debug.Log("Spawning Projectile");
            }
        }
        
        void AnimateBackToIdle() 
            => facade.Blackboard.animController.animator.CrossFade(facade.Blackboard.animController.upperbodyidle, 0.1f, 1);


        public void OnFixedTick(float dt) => ExecuteTemplateCall(dt);
    }
    
    
    public class MoveModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_PilotController>, FIXEDUPDATE
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
            public Ref<float> runAlphaMax; // Should be greater than the greatest blend tree value to avoid jitter
            [field: SerializeField] public Ref<float> slowdownTime { get; private set; }

            [Button]
            void Init() => decayScalar = new Ref<float>(1);

        }

        Config cfg => facade.Config.move;
        
        public MoveModule(PersistentAction<bool> action, TwoD_PilotController facade) : base(action, facade, true) { }

        protected override void Awake()
        {
            facade.Blackboard.moveDecay = new DecayTimer(facade.Config.move.runAlphaMax, facade.Config.move.decayScalar);
            facade.Blackboard.turnSlowdown = new CountdownTimer(facade.Config.move.slowdownTime);

            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.InitTimer(facade.Blackboard.turnSlowdown, true);
        }

        protected override void OnSet() { }

        protected override void Implementation(float dt)
        {
            //Debug.Log("Attempting move, input is : " + facade.Input.movement);
            if (!facade.Blackboard.isRunning) Walk();
            else Run();
            Move(facade.Input.movement);
            
            void Walk()
            {
                if(facade.Blackboard.speedAlpha < cfg.walkAlphaMax) facade.Blackboard.speedAlpha += facade.Blackboard.animController.speedStep;
                facade.Blackboard.speedAlpha = ToleranceSet(facade.Blackboard.speedAlpha, cfg.walkAlphaMax, facade.Blackboard.animController.moveJitterTolerance);
            }
            void Run()
            {
                if (facade.Blackboard.speedAlpha > cfg.runAlphaMax)
                    facade.Blackboard.speedAlpha = cfg.runAlphaMax;
                else if(facade.Blackboard.speedAlpha < cfg.runAlphaMax) 
                    facade.Blackboard.speedAlpha += facade.Blackboard.animController.speedStep;
            }
            
            void Move(Vector2 move)
            {
                if (move.x == 0) return;
                LookDir prevMoveDir = facade.Blackboard.moveDir;

                Vector3 dir = new Vector3(move.x, 0, 0);
                facade.Blackboard.moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
                ApplyMoveForce(dir);
                
                if (prevMoveDir != facade.Blackboard.moveDir)
                {
                    facade.Blackboard.turnSlowdown.Restart();
                    // if(facade.Blackboard.isMantled) facade.Input.UnMantleLedge?.Invoke();
                }
                
                
                
                void ApplyMoveForce(Vector3 dir)
                {
                    float runSpeedIncludingDecay = (facade.Blackboard.speedAlpha > cfg.walkAlphaMax ? cfg.maxSpeed : cfg.moveForce);
                    float actualSpeed = facade.Blackboard.isRunning ? runSpeedIncludingDecay : cfg.moveForce;
                    if (facade.Blackboard.turnSlowdown.isRunning) actualSpeed *= facade.Blackboard.turnSlowDown.Eval(facade.Blackboard.phys.isGrounded, facade.Blackboard.turnSlowdown.Progress);
                    if (!facade.Blackboard.phys.isGrounded) actualSpeed *= facade.Blackboard.phys.fallSettings.inAirMoveScalar;
                    Debug.Log("Appling force in direction " + dir + " with  speed " + actualSpeed);
                    facade.Blackboard.rb.AddForce(dir * actualSpeed, cfg.forceMode);
                }
            }
        }

        public void OnFixedTick(float dt) => ExecuteTemplateCall(dt);
    }
}