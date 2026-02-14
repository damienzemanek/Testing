using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static TwoD_Config;

public class TwoD_Functionality : Functionalities<TwoD_Controller>
{
    public override void AddModulesHere()
    {
        AddModule(new MoveModule(facade.Input.Move, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
    }

    

    public class ShootModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_Controller>
    {
        public ShootModule(PersistentAction<bool> action, TwoD_Controller facade) : base(action, facade) { }

        protected override void Awake()
        {
            executeGuarder = new ActionGuarderMutable((
                    new ActionGuard(() => !isActive, AnimateBackToIdle, "Is Active", "Idle Anim")), 
                    new LazyActionGuard<LazyFuncLite<bool>>(facade.Blackboard.isMantled.SimpleReactions, () => facade.Blackboard.isMantled, "Is Mantled"));
            
        }

        protected override void OnSet() { }

        protected override void Execute(float dt)
        {
            facade.StartCoroutine(ShootImplementation());
            IEnumerator ShootImplementation()
            {
                facade.Blackboard.bulletSpawner.targetPosition = facade.Blackboard.mouseLook.core.contactPoint;
                if (facade.Blackboard.bulletSpawner.fireTimer.isRunning) yield break;
                facade.Blackboard.animController.animator.Play(facade.Blackboard.animController.shoot, layer: 1, normalizedTime: 0f);
                yield return null;
                facade.Blackboard.bulletSpawner.Spawn();
            }
        }
        
        void AnimateBackToIdle() 
            => facade.Blackboard.animController.animator.CrossFade(facade.Blackboard.animController.upperbodyidle, 0.1f, 1);
        
        
    }
    
    
    public class MoveModule : InputHeldModuleFacade<ActionGuarderMutable, TwoD_Controller> 
    {
        public MoveModule(PersistentAction<bool> action, TwoD_Controller facade) : base(action, facade) { }
        
        protected override void OnSet() { }

        protected override void Execute(float dt)
        {
            if (!facade.Blackboard.isRunning) Walk();
            else Run();
            Move(facade.Blackboard.input.movement);
            
            void Walk()
            {
                if(facade.Blackboard.speedAlpha < facade.Config.walkAlphaMax) facade.Blackboard.speedAlpha += facade.Blackboard.animController.speedStep;
                facade.Blackboard.speedAlpha = ToleranceSet(facade.Blackboard.speedAlpha, facade.Config.walkAlphaMax, facade.Blackboard.animController.moveJitterTolerance);
            }
            void Run()
            {
                if (facade.Blackboard.speedAlpha > facade.Config.runAlphaMax)
                    facade.Blackboard.speedAlpha = facade.Config.runAlphaMax;
                else if(facade.Blackboard.speedAlpha < facade.Config.runAlphaMax) 
                    facade.Blackboard.speedAlpha += facade.Blackboard.animController.speedStep;
            }
            
            void Move(Vector2 move)
            {
                if (move.x == 0) return;
                LookDir prevMoveDir = facade.Blackboard.moveDir;
                
                Vector3 dir = move.x < 0 ? Vector3.left : Vector3.right;
                facade.Blackboard.moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
                ApplyMoveForce(dir);
                
                if (prevMoveDir != facade.Blackboard.moveDir)
                {
                    facade.Blackboard.turnSlowdown.Restart();
                    if(facade.Blackboard.isMantled) facade.Input.UnMantleLedge?.Invoke();
                }
                
                
                
                void ApplyMoveForce(Vector3 dir)
                {
                    float runSpeedIncludingDecay = (facade.Blackboard.speedAlpha > facade.Config.walkAlphaMax ? facade.Blackboard.movement.maxSpeed : facade.Blackboard.movement.moveForce);
                    float actualSpeed = facade.Blackboard.isRunning ? runSpeedIncludingDecay : facade.Blackboard.movement.moveForce;
                    if (facade.Blackboard.turnSlowdown.isRunning) actualSpeed *= facade.Blackboard.turnSlowDown.Eval(facade.Blackboard.phys.isGrounded, facade.Blackboard.turnSlowdown.Progress);
                    if (!facade.Blackboard.phys.isGrounded) actualSpeed *= facade.Blackboard.phys.fallSettings.inAirMoveScalar;
                    facade.Blackboard.rb.AddForce(dir * actualSpeed, facade.Blackboard.movement.forceMode);
                }
            }
        }

    }
}