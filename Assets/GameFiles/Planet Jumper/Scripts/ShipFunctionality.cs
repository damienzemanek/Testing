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

    public void MoreInit()
    {
        Debug.Log(facade.Input);
        Debug.Log(facade.Config);
        AddRotateModule();
        
    }


    [Button]
    public void AddRotateModule()
    {
        if (facade == null) return;
        Debug.Log(facade.Input.Rotate);
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


        public RotateModule(PersistentAction<Vector3, bool> action, ShipController facade) : base(action, facade)
        {
            Debug.Log(action);
            Debug.Log(facade.Config);
        }

        public override void InitImplementation()
            => ExecuteGateFlowOut.Add(() => isRotating, () => facade.Blackboard.rb.angularVelocity = Vector3.zero);

        public override void OnSetImplementation(Vector3 rotation)
            => rotationVector = rotation;

        public override void ExecuteImplementation(float dt)
        {
            Debug.Log(facade);
            Debug.Log(facade.Config);
            Debug.Log(facade.Config.rotate);
            Debug.Log(facade.Config.rotate.scalar);
            
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
