using System;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static CamEX;


[Serializable]
public class ShipBlackboard : Blackboard, IInteriorElement<ShipController>
{
    public ShipController facade { get; set; }
    
    [field:SerializeField] public Rigidbody rb { get; private set; }
    [field:SerializeField] public Transform camTransform { get; private set; }
    [field:SerializeField] public CinemachineCamera cam { get; private set; }
    [field:SerializeField] public GameObject shipCameraObject { get; private set; }
    [field:SerializeField] public Camera cannonCameraComponent { get; private set; }
    
    
    
    [field:SerializeField] public ParticleSystem vfx_Thrust { get; private set; }
    [SerializeField] public CurveValue thrustFOV;

    
    

}
