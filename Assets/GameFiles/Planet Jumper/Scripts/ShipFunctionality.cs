using System;
using System.Collections.Generic;
using EMILtools_Private.Testing;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static FlowOutChain;

[Serializable]
public class ShipFunctionality : Functionalities, IInterior<ShipController>
{
    public ShipController facade { get; set; }

    public void InitImplementation()
    {
        if (facade == null) Debug.LogError("missing facade");
        
        
        AddModule(new RotateModule(facade.Input.Rotate, facade));
    }

    
    
    [Serializable]
    public class RotateModule : InputModuleInterior<Vector3, FlowMutable, ShipController>, FIXEDUPDATE
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


        public RotateModule(PersistentAction<Vector3, bool> action, ShipController facade) : base(action, facade) { }

        public override void InitImplementation()
            => ExecuteGateFlowOut.Add(() => isRotating, () => facade.Blackboard.rb.angularVelocity = Vector3.zero);

        public override void OnSetImplementation(Vector3 rotation)
            => rotationVector = rotation;

        public override void ExecuteImplementation(float dt)
        {
            Quaternion deltaScaled = Quaternion.Euler(rotationVector * facade.Config.rotate.scalar);
            Quaternion newRot = camOffset * deltaScaled * Quaternion.Inverse(camOffset) * facade.Blackboard.transform.rotation;

            facade.Blackboard.transform.rotation = Quaternion.Lerp(facade.Blackboard.transform.rotation, newRot, 0.1f); 
        }


        public void FixedTick(float dt)
        {
            Execute(dt);
        }
    }

}
