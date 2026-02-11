using System;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static CamEX;


[Serializable]
public class ShipBlackboard : Blackboard, IFacadeCompositionElement<ShipController>
{
    public ShipController facade { get; set; }
    
    [field: BoxGroup("Movement")] [field:SerializeField] public Rigidbody rb { get; private set; }
    [field: BoxGroup("Cam")] [field:SerializeField] public Transform camTransform { get; private set; }
    [field: BoxGroup("Cam")] [field:SerializeField] public CinemachineCamera cam { get; private set; }
    [field: BoxGroup("Cam")] [field:SerializeField] public GameObject shipCameraObject { get; private set; }
    [field: BoxGroup("Cam")] [field:SerializeField] public Camera cannonCameraComponent { get; private set; }
    [BoxGroup("Cam")] [ReadOnly] public bool usingCannonCam = false;
    
    
    [field: BoxGroup("Thrust")] [field:SerializeField] public ParticleSystem vfx_Thrust { get; private set; }
    [BoxGroup("Thrust")] public CurveValue thrustFOV;
    
    
    [BoxGroup("Fire")] [SerializeField] public ProjectileSpawnManager cannonProjectileSpawner;
    [BoxGroup("Fire")] [SerializeField] public Animator gunAnimator;

    
    

}
