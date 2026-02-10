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
using static FlowOutChain;
using static EMILtools.Timers.TimerUtility;

[Serializable]
public class ShipFunctionality : Functionalities<ShipController>, IInteriorElement<ShipController>
{
    public override void AddModulesHere()
    {
        AddModule(new RotateModuleSub(facade.Input.Rotate, facade));
        AddModule(new ThrustModuleSub(facade.Input.Thrust, facade));
    }

    [Serializable]
    public class ThrustModuleSub : InputModuleSubInterior<FlowMutable, ShipController>, UPDATE, FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            [SerializeField] public ForceMode thrustForceMode;
            [SerializeField] public float thrustForce;
            [SerializeField] public float defaultFOV;
        }

        Config config => facade.Config.thrust;
        
        public ThrustModuleSub(PersistentAction<bool> action, ShipController facade) : base(action, facade) { }
        

        public override void Awake()
        {
            facade.Blackboard.thrustFOV.SetInitialTime(1f);
            facade.InitializeTimers((facade.Blackboard.thrustFOV, false));
            Debug.Log("inited thrust module");
        }


        public override void OnSetImplementation()
        {
            if (isActive)
            {
                Debug.Log("active");
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Increase);
                facade.Blackboard.vfx_Thrust.Play();
            }
            else
            {
                Debug.Log("inactive");
                facade.Blackboard.thrustFOV.DynamicStart(Operation.Decrease);
                facade.Blackboard.vfx_Thrust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public override void Implementation(float dt)
        => facade.Blackboard.rb.AddForce(facade.transform.up * config.thrustForce, config.thrustForceMode);

        
        public void OnUpdateTick(float dt) => facade.Blackboard.cam.Lens.FieldOfView = facade.Blackboard.thrustFOV.Evaluate * config.defaultFOV;
        
        public void OnFixedTick(float dt) => Execute(dt);
    }
    
    
    
    
    [Serializable]
    public class RotateModuleSub : InputModuleSubInterior<Vector3, FlowMutable, ShipController>, FIXEDUPDATE
    {
        [Serializable]
        public struct Config
        {
            [ShowInInspector] public float scalar;
        }
        
        [ShowInInspector, ReadOnly] Quaternion camOffset => facade != null 
                                                                ? facade.Blackboard.camTransform.rotation 
                                                                : Quaternion.identity;
        [ShowInInspector, ReadOnly] bool isRotating;
        [ShowInInspector, ReadOnly] Vector3 rotationVector;


        public RotateModuleSub(PersistentAction<Vector3, bool> action, ShipController facade) : base(action, facade) { }

        public override void Awake()
        {
            ExecuteGateFlowOut.Add(() => isRotating, () => facade.Blackboard.rb.angularVelocity = Vector3.zero);
            Debug.Log("inited rotate module");
        }
        public override void OnSetImplementation(Vector3 rotation)
            => rotationVector = rotation;

        public override void Implementation(float dt)
        {
            Quaternion deltaScaled = Quaternion.Euler(rotationVector * facade.Config.rotate.scalar);
            Quaternion newRot = camOffset * deltaScaled * Quaternion.Inverse(camOffset) * facade.transform.rotation;

            facade.transform.rotation = Quaternion.Lerp(facade.transform.rotation, newRot, 0.1f); 
        }


        public void OnFixedTick(float dt)
        {
            Execute(dt);
        }
    }

}
