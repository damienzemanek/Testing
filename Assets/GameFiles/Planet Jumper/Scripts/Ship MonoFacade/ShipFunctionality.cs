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

public class ShipFunctionality : Functionalities<ShipController>
{
    protected override void AddModulesHere()
    {
        // AddModule(new RotateModuleToggleSub(facade.Input.Rotate, facade));
        AddModule(new ThrustModuleSub(facade.Input.Thrust, facade));
        AddModule(new FireModule(facade.Input.Fire, facade));
        AddModule(new SwitchCamModule(facade.Input.SwitchCam, facade));
        AddModule(new CannonMouselookModule(facade.Input.MouseLook, facade));
        AddModule(new SteerModule(facade.Input.Thrust, facade));
    }



    public class CannonMouselookModule : InputHeldModuleFacade<ShipController>, UPDATE
    {
        public CannonMouselookModule(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }

        protected override void Awake()
        {
            executeGuarder.Add(new ActionGuard(() => !facade.Blackboard.usingCannonCam, "Not in Cannon Cam"));
            facade.Blackboard.cannonMouseLook.Input = facade.Input;
        }
        protected override void Execute(float dt) => facade.Blackboard.cannonMouseLook.UpdateMouseLook();
        public void UpdateTick(float dt) => ExecuteTemplateCall(dt);
    }
    

    public class SwitchCamModule : InputPressedModuleFacade<ShipController>
    {
        public SwitchCamModule(PersistentAction action, ShipController facade) : base(action, facade) { }

        
        protected override void OnPress()
        {
            facade.Blackboard.usingCannonCam = !facade.Blackboard.usingCannonCam;
            facade.Blackboard.cannonMouseLook.updateMouseLook = facade.Blackboard.usingCannonCam;
            facade.Blackboard.shipCameraObject.SetActive(!facade.Blackboard.usingCannonCam);
            facade.Blackboard.cannonCameraComponent.enabled = facade.Blackboard.usingCannonCam;
        }
        
    }

    

    public class FireModule : InputHeldModuleFacade<ShipController>, FIXEDUPDATE
    {

        static readonly int fireAnimNameLeft = Animator.StringToHash("fireLeft");
        static readonly int fireAnimNameRight = Animator.StringToHash("fireRight");
        bool shootDirToggle = true;
        
        public FireModule(PersistentAction<bool> action, ShipController facade) : base(action, facade, true) { }

        protected override void Awake()
        {
            facade.Blackboard.cannonProjectileSpawner.OnSpawn = new PersistentAction();
            facade.Blackboard.cannonProjectileSpawner.OnSpawn.Add(ShootAnim);
            executeGuarder.Add(new ActionGuard(() => !facade.Blackboard.usingCannonCam, "Not in Cannon Cam"));
        }
        
        protected override void OnSet() { }

        protected override void Execute(float dt)
        => facade.Blackboard.cannonProjectileSpawner.Spawn();
        
        public void FixedTick(float dt) => ExecuteTemplateCall(dt);



        void ShootAnim()
        {
            if (shootDirToggle)
                facade.Blackboard.gunAnimator.Play(fireAnimNameLeft, 0, 0f);
            else
                facade.Blackboard.gunAnimator.Play(fireAnimNameRight, 0, 0f);
            shootDirToggle = !shootDirToggle;
        }
    }
    

    public class ThrustModuleSub : InputHeldModuleFacade<ShipController>, UPDATE, FIXEDUPDATE
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

        Config config => facade.Config.thrust;
        
        public ThrustModuleSub(PersistentAction<bool> action, ShipController facade) : base(action, facade, true) { }
        

        protected override void Awake()
        {
            facade.Blackboard.thrustFOV.SetInitialTime(1f);
            facade.InitTimers((facade.Blackboard.thrustFOV, false));
            facade.Blackboard.rb.maxLinearVelocity = config.maxVelocity;
        }


        protected override void OnSet()
        {
            if (isActive)
            {
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Increase);
                facade.Blackboard.vfx_Thrust.Play();
            }
            else
            {
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Decrease);
                facade.Blackboard.vfx_Thrust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Slow();
            }
        }

        protected override void Execute(float dt)
        => facade.Blackboard.rb.AddForce(facade.transform.forward * config.thrustForce, config.thrustForceMode);

        void Slow() => facade.Blackboard.rb.linearVelocity *= facade.Config.thrust.notMovingSlowScalar;

        
        public void UpdateTick(float dt) => facade.Blackboard.cam.Lens.FieldOfView = facade.Blackboard.thrustFOV.Evaluate * config.defaultFOV;
        
        public void FixedTick(float dt) => ExecuteTemplateCall(dt);
    }


    public class SteerModule : InputHeldModuleFacade<ShipController>, FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float steerSmooth { get; private set; }
            [field: SerializeField] public Vector3 offset { get; private set; }
        }
        
        Config cfg => facade.Config.steer;
        
        public SteerModule(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }

        protected override void Awake()
        => executeGuarder = new (new ActionGuard(() => !isActive, StopSteering));

        protected override void Execute(float dt)
        {
            Quaternion target = Quaternion.Euler(facade.Blackboard.cam.transform.rotation.eulerAngles + cfg.offset);

            float t = Mathf.Clamp01(cfg.steerSmooth * dt);
            facade.transform.rotation = Quaternion.Lerp(facade.transform.rotation, target, t);
        }

        public void FixedTick(float dt) => ExecuteTemplateCall(dt);
        void StopSteering() => facade.Blackboard.rb.angularVelocity = Vector3.zero;
        
    }
    
    
    
    //
    // public class RotateModuleToggleSub : InputHeldModuleFacade<Vector3, ShipController>, FIXEDUPDATE
    // {
    //     [Serializable]
    //     public struct Config
    //     {
    //         [field: SerializeField] public float scalar { get; private set; }
    //     }
    //     
    //     [ShowInInspector, ReadOnly] Quaternion camOffset => facade != null 
    //                                                             ? facade.Blackboard.camTransform.rotation 
    //                                                             : Quaternion.identity;
    //     [ShowInInspector, ReadOnly] Vector3 rotationVector;
    //
    //
    //     public RotateModuleToggleSub(PersistentAction<Vector3, bool> action, ShipController facade) : base(action, facade, true) { }
    //
    //     protected override void Awake()
    //         => executeGuarder = new (new ActionGuard(() => !isActive, () => facade.Blackboard.rb.angularVelocity = Vector3.zero));
    //     
    //     protected override void OnSet(Vector3 rotation)
    //         => rotationVector = rotation;
    //
    //     protected override void Execute(float dt)
    //     {
    //         Quaternion deltaScaled = Quaternion.Euler(rotationVector * facade.Config.rotate.scalar);
    //         Quaternion newRot = camOffset * deltaScaled * Quaternion.Inverse(camOffset) * facade.transform.rotation;
    //
    //         facade.transform.rotation = Quaternion.Lerp(facade.transform.rotation, newRot, 0.1f); 
    //     }
    //
    //
    //     public void OnFixedTick(float dt)
    //     {
    //         ExecuteTemplateCall(dt);
    //     }
    // }

}
