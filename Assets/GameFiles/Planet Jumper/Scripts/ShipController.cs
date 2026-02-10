using System;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using static CamEX;
using static CamEX.CurveValue;
using static Effectability;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Timers.TimerUtility;
using static LifecycleEX;
using static ShipFunctionality;

[Serializable]
public class ShipController : CoreFacade<ShipInputReader, ShipFunctionality, ShipConfig, ShipBlackboard, ShipController> , ITimerUser
{
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isRotating;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isFiring;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] float cachedFOV;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool usingCannonCam = false;
    
    
    
    [BoxGroup("Cannons")] [SerializeField] MouseLookSettings cannonMouseLook;
    [BoxGroup("Cannons")] [SerializeField] ProjectileSpawnManager cannonProjectileSpawner;
    
    
    private void Awake()
    {
        Init();   
        
    }

    void Start()
    {
        CursorEX.Set(false, CursorLockMode.Locked);
        cannonMouseLook.updateMouseLook = false;
        Blackboard.cannonCameraComponent.enabled = false;
    }

    private void OnEnable()
    {
        CursorEX.Set(false, CursorLockMode.Locked);


        Input.SwitchCam.Add(SwitchCam);
        Input.Fire.Add(Fire);
        Functionality.Bind();
    }

    private void OnDisable()
    {
        Input.SwitchCam.Remove(SwitchCam);
        Input.Fire.Remove(Fire);
        Functionality.Unbind();
    }

    protected override void Update()
    {
        base.Update();
        cannonMouseLook.UpdateMouseLook();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleFiring();
    }
    
    
    
    void SwitchCam()
    {
        usingCannonCam = !usingCannonCam;
        cannonMouseLook.updateMouseLook = usingCannonCam;
        Blackboard.shipCameraObject.SetActive(!usingCannonCam);
        Blackboard.cannonCameraComponent.enabled = usingCannonCam;
    }
    void Fire(bool active)
    {
        if (!usingCannonCam)
        {
            isFiring = false;
            return;
        }
        isFiring = active;
    }
    
    

    void HandleFiring()
    {
        if (!isFiring) return;
       cannonProjectileSpawner.Spawn();
    }
    


    private void OnDestroy()
    {
        this.ShutdownTimers();
        cannonProjectileSpawner.ShutdownTimers();
    }
}
