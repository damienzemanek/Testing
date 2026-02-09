using System;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;


[Serializable]
public class ShipBlackboard : Blackboard, IInterior<ShipController>
{
    public ShipController facade { get; set; }
    
    [field:SerializeField] public Rigidbody rb { get; private set; }
    [field:SerializeField] public Transform camTransform { get; private set; }
    [field:SerializeField] public Transform transform { get; private set; }
    [field:SerializeField] public CinemachineCamera cam { get; private set; }
    [field:SerializeField] public GameObject shipCameraObject { get; private set; }
    [field:SerializeField] public Camera cannonCameraComponent { get; private set; }
    

}
