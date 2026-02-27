using System;
using System.Collections.Generic;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static CamEX;
using static CamEX.CurveValue;
using static EMILtools.Timers.TimerUtility;

public class ShipFunctionality : Functionalities<ShipController, ShipContext>
{
    protected override void AddModulesHere()
    {
        AddModule(new ThrustModuleSub(facade.Input.Thrust, facade));
        AddModule(new FireModule(facade.Input.Fire, facade));
        AddModule(new SwitchCamModule(facade.Input.SwitchCam, facade));
        AddModule(new CannonMouselookModule(facade.Input.MouseLook, facade));
        AddModule(new SteerModule(facade.Input.Thrust, facade));
    }

    public class CannonMouselookModule :
        BoundSetFunctionality<ShipController, ShipContext, CannonMouselookModule.Setter>,
        UPDATE
    {
        public class Setter : SettableTemplate<bool> { }
        public CannonMouselookModule(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.cannonMouseLook.Input = facade.Input;
        }
        public override PipelineBuilder<ShipContext> InjectSteps(PipelineBuilder<ShipContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.usingCannonCam);

        public override bool ExecutionImplementation(ShipContext ctx)
        {
            facade.Blackboard.cannonMouseLook.UpdateMouseLook();
            return true;
        }

        public void UpdateTick() => ExecuteTemplateCall();
    }

    public class SwitchCamModule : BoundFunctionality<ShipController, ShipContext>
    {
        public SwitchCamModule(PersistentAction action, ShipController facade) : base(action, facade) { }
        public override PipelineBuilder<ShipContext> InjectSteps(PipelineBuilder<ShipContext> builder) => builder;

        public override bool ExecutionImplementation(ShipContext ctx)
        {
            facade.Blackboard.usingCannonCam = !facade.Blackboard.usingCannonCam;
            facade.Blackboard.cannonMouseLook.updateMouseLook = facade.Blackboard.usingCannonCam;
            facade.Blackboard.shipCameraObject.SetActive(!facade.Blackboard.usingCannonCam);
            facade.Blackboard.cannonCameraComponent.enabled = facade.Blackboard.usingCannonCam;
            return true;
        }
    }

    public class FireModule :
        BoundSetFunctionality<ShipController, ShipContext, FireModule.Setter>,
        FIXEDUPDATE
    {
        public class Setter : SettableTemplate<bool> {  }
        static readonly int fireAnimNameLeft = Animator.StringToHash("fireLeft");
        static readonly int fireAnimNameRight = Animator.StringToHash("fireRight");
        bool shootDirToggle = true;

        public FireModule(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.cannonProjectileSpawner.OnSpawn ??= new PersistentAction();
            facade.Blackboard.cannonProjectileSpawner.OnSpawn.Add(ShootAnim);
        }
        
        public override PipelineBuilder<ShipContext> InjectSteps(PipelineBuilder<ShipContext> builder)
            => builder.ExitIf(_ => !facade.Blackboard.usingCannonCam || !isActive);

        public override bool ExecutionImplementation(ShipContext ctx)
        {
            facade.Blackboard.cannonProjectileSpawner.Spawn();
            return true;
        }

        public void FixedTick() => ExecuteTemplateCall();

        void ShootAnim()
        {
            if (shootDirToggle)
                facade.Blackboard.gunAnimator.Play(fireAnimNameLeft, 0, 0f);
            else
                facade.Blackboard.gunAnimator.Play(fireAnimNameRight, 0, 0f);
            shootDirToggle = !shootDirToggle;
        }
    }

    public class ThrustModuleSub : 
        BoundSetFunctionality<ShipController, ShipContext, ThrustModuleSub.Setter>,
        UPDATE, FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public ForceMode thrustForceMode { get; private set; }
            [field: SerializeField] public float thrustForce { get; private set; }
            [field: SerializeField] public float defaultFOV { get; private set; }
            [field: SerializeField] public float notMovingSlowScalar { get; private set; }
            [field: SerializeField] public float maxVelocity { get; private set; }
        }

        public class Setter : SettableTemplate<bool> { }

        Config config => facade.Config.thrust;

        public ThrustModuleSub(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }

        protected override void Awake()
        {
            facade.Blackboard.thrustFOV.SetInitialTime(1f);
            facade.InitTimers((facade.Blackboard.thrustFOV, false));
            facade.Blackboard.rb.maxLinearVelocity = config.maxVelocity;
        }
        public override PipelineBuilder<ShipContext> InjectSteps(PipelineBuilder<ShipContext> builder) => builder;

        public override bool ExecutionImplementation(ShipContext ctx)
        {
            if (isActive)
            {
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Increase);
                facade.Blackboard.vfx_Thrust.Play();
                facade.Blackboard.rb.AddForce(facade.transform.forward * config.thrustForce, config.thrustForceMode);
            }
            else
            {
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Decrease);
                facade.Blackboard.vfx_Thrust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Slow();
            }
            return true;
        }

        void Slow() => facade.Blackboard.rb.linearVelocity *= facade.Config.thrust.notMovingSlowScalar;

        public void UpdateTick() => facade.Blackboard.cam.Lens.FieldOfView = facade.Blackboard.thrustFOV.Evaluate * config.defaultFOV;
        public void FixedTick() => ExecuteTemplateCall();
    }

    public class SteerModule : 
        BoundSetFunctionality<ShipController, ShipContext, SteerModule.Setter>,
        FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float steerSmooth { get; private set; }
            [field: SerializeField] public Vector3 offset { get; private set; }
        }

        public class Setter : SettableTemplate<bool> { }

        Config cfg => facade.Config.steer;

        public SteerModule(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }
        public override PipelineBuilder<ShipContext> InjectSteps(PipelineBuilder<ShipContext> builder)
            => builder.ExitIf(_ => !isActive, new Callback(StopSteering));

        public override bool ExecutionImplementation(ShipContext ctx)
        {
            Quaternion target = Quaternion.Euler(facade.Blackboard.cam.transform.rotation.eulerAngles + cfg.offset);
            float t = Mathf.Clamp01(cfg.steerSmooth * Time.fixedDeltaTime);
            facade.transform.rotation = Quaternion.Lerp(facade.transform.rotation, target, t);
            return true;
        }

        public void FixedTick() => ExecuteTemplateCall();
        void StopSteering() => facade.Blackboard.rb.angularVelocity = Vector3.zero;
    }
}
