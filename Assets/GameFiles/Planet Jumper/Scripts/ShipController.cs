using System;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using static CamEX;
using static CamEX.CurveValue;
using static Effectability;
using static EMILtools.Extensions.MouseLookEX;
using static EMILtools.Timers.TimerUtility;
using static LifecycleEX;

public class ShipController : MonoBehaviour, ITimerUser
{
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Vector3 rotationVector;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isRotating;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isThrusting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isFiring;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Quaternion camOffset => camTransform != null ? camTransform.rotation : Quaternion.identity;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] float cachedFOV;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool usingCannonCam = false;

    
    [BoxGroup("References")] [SerializeField] ShipInputReader input;
    [BoxGroup("References")] [SerializeField] Rigidbody rb;
    [BoxGroup("References")] [SerializeField] Transform camTransform;
    [BoxGroup("References")] [SerializeField] CinemachineCamera cam;
    [BoxGroup("References")] [SerializeField] GameObject shipCameraObject;
    [BoxGroup("References")] [SerializeField] Camera cannonCameraComponent;

    
    [BoxGroup("Thrust")] [SerializeField] ForceMode thrustForceMode = ForceMode.Force;
    [BoxGroup("Thrust")] [SerializeField] float thrustForce;
    [BoxGroup("Thrust")] [SerializeField] CurveValue thrustFOV;
    [BoxGroup("Thrust")] [SerializeField] float defaultFOV = 70f;
    [BoxGroup("Thrust")] [SerializeField] ParticleSystem vfx_Thrust;
    
    [BoxGroup("Rotation")] [SerializeField] ForceMode rotateForceMode = ForceMode.Force;
    [BoxGroup("Rotation")] [SerializeField] float rotationScalar;
    
    [BoxGroup("Cannons")] [SerializeField] MouseLookSettings cannonMouseLook;
    [BoxGroup("Cannons")] [SerializeField] ProjectileSpawnManager cannonProjectileSpawner;


    private void Awake()
    {
        thrustFOV.SetInitialTime(1f);
        this.InitializeTimers((thrustFOV, false));
    }

    void Start()
    {
        CursorEX.Set(false, CursorLockMode.Locked);
        cannonMouseLook.updateMouseLook = false;
        cannonCameraComponent.enabled = false;
    }

    private void OnEnable()
    {
        CursorEX.Set(false, CursorLockMode.Locked);
        input.Thrust += Thrust;
        input.Rotate += Rotate;
        input.SwitchCam += SwitchCam;
        input.Fire += Fire;
    }

    private void OnDisable()
    {
        input.Thrust -= Thrust;
        input.Rotate -= Rotate;
        input.SwitchCam -= SwitchCam;
        input.Fire -= Fire;
    }

    private void Update()
    {
        cam.Lens.FieldOfView = thrustFOV.Evaluate * defaultFOV;
        cannonMouseLook.UpdateMouseLook();
    }

    private void FixedUpdate()
    {
        HandleRotation();
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
    void Rotate(Vector3 rotation, bool active)
    {
        if (usingCannonCam) return;
        
        rotationVector = rotation;
        isRotating = active;
    }
    void SwitchCam()
    {
        usingCannonCam = !usingCannonCam;
        cannonMouseLook.updateMouseLook = usingCannonCam;
        shipCameraObject.SetActive(!usingCannonCam);
        cannonCameraComponent.enabled = usingCannonCam;
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
    
    

    void HandleRotation()
    {
        if (!isRotating)
        {
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Quaternion deltaScaled = Quaternion.Euler(rotationVector * rotationScalar);
        Quaternion newRot = camOffset * deltaScaled * Quaternion.Inverse(camOffset) * transform.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 0.1f);
    }
    void HandleThrust()
    {
        if (!isThrusting) return;
        rb.AddForce(transform.up * thrustForce, thrustForceMode);
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
