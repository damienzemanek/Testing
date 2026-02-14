using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static TwoD_Config;

public class TwoD_Functionality : Functionalities<TwoD_Controller>
{
    public override void AddModulesHere()
    {
        AddModule(new MoveModule(facade.Input.Move, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
        AddModule(new LookModule(facade.Input.Look, facade));
        AddModule(new FaceDirectionModule(facade.Input.FaceDirection, facade));
    }


    public class FaceDirectionModule : InputHeldModuleFacade<LookDir, ActionGuarderMutable, TwoD_Controller>, UPDATE
    {
        public FaceDirectionModule(PersistentAction<LookDir, bool> action, TwoD_Controller facade) : base(action, facade, false) { }
        
        [ShowInInspector] LookDir dir;

        protected override void OnSetImplementation(LookDir args) => dir = args;

        protected override void Implementation(float dt)
        {
            if (dir == LookDir.Left) facade.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            if (dir == LookDir.Right) facade.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            facade.Blackboard.facingDir = dir;
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }

    public class LookModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_Controller>, LATEUPDATE
    {
        public LookModule(PersistentAction<bool> action, TwoD_Controller facade) : base(action, facade, false) { }

        protected override void Awake() => 
            executeGuarder.Add(new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.isMantled.SimpleReactions,
                                                                    () => facade.Blackboard.isMantled, "Is Mantled"));

        protected override void Implementation(float dt) => facade.Blackboard.mouseLook.Execute();

        public void LateTick(float dt) => ExecuteTemplateCall(dt);
    }
    

    public class ShootModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_Controller>, FIXEDUPDATE
    {
        public ShootModule(PersistentAction<bool> action, TwoD_Controller facade) : base(action, facade, true) { }

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
                Debug.Log("Spawning Projectile");
            }
        }
        
        void AnimateBackToIdle() 
            => facade.Blackboard.animController.animator.CrossFade(facade.Blackboard.animController.upperbodyidle, 0.1f, 1);


        public void OnFixedTick(float dt) => ExecuteTemplateCall(dt);
    }
    
    
    public class MoveModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_Controller>, FIXEDUPDATE
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
            public float runAlphaMax; // Should be greater than the greatest blend tree value to avoid jitter
            [Button]
            void Init() => decayScalar = new Ref<float>(1);
        }

        Config cfg => facade.Config.move;
        
        public MoveModule(PersistentAction<bool> action, TwoD_Controller facade) : base(action, facade, true) { }
        
        protected override void OnSet() { }

        protected override void Implementation(float dt)
        {
            Debug.Log("Attempting move, input is : " + facade.Input.movement);
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
                    if(facade.Blackboard.isMantled) facade.Input.UnMantleLedge?.Invoke();
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