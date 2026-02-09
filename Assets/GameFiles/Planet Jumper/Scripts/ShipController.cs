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
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isThrusting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isFiring;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] float cachedFOV;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool usingCannonCam = false;
    
    
    [BoxGroup("Thrust")] [SerializeField] ForceMode thrustForceMode = ForceMode.Force;
    [BoxGroup("Thrust")] [SerializeField] float thrustForce;
    [BoxGroup("Thrust")] [SerializeField] CurveValue thrustFOV;
    [BoxGroup("Thrust")] [SerializeField] float defaultFOV = 70f;
    [BoxGroup("Thrust")] [SerializeField] ParticleSystem vfx_Thrust;
    
    [BoxGroup("Cannons")] [SerializeField] MouseLookSettings cannonMouseLook;
    [BoxGroup("Cannons")] [SerializeField] ProjectileSpawnManager cannonProjectileSpawner;
    
    
    private void Awake()
    {
        Init();   
        
        thrustFOV.SetInitialTime(1f);
        this.InitializeTimers((thrustFOV, false));
        
        
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


        Input.Thrust.Add(Thrust);
        Input.SwitchCam.Add(SwitchCam);
        Input.Fire.Add(Fire);
        Functionality.Bind();
    }

    private void OnDisable()
    {
        Input.Thrust.Remove(Thrust);
        Input.SwitchCam.Remove(SwitchCam);
        Input.Fire.Remove(Fire);
        Functionality.Unbind();
    }

    protected override void Update()
    {
        base.Update();
        Blackboard.cam.Lens.FieldOfView = thrustFOV.Evaluate * defaultFOV;
        cannonMouseLook.UpdateMouseLook();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleThrust();
        HandleFiring();
    }
    
    
    void Thrust(bool active)
    {
        isThrusting = active;

        if (active)
        {
            thrustFOV.DynamicStart(Operation.Increase);
            vfx_Thrust.Play();
        }
        else
        {
            thrustFOV.DynamicStart(Operation.Decrease);
            vfx_Thrust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
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
    
    
    void HandleThrust()
    {
        if (!isThrusting) return;
        Blackboard.rb.AddForce(transform.up * thrustForce, thrustForceMode);
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
