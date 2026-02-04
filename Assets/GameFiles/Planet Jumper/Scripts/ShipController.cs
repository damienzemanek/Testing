using System;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static CamEX;
using static CamEX.CurveValue;
using static EMILtools.Timers.TimerUtility;
using static LifecycleEX;

public class ShipController : MonoBehaviour, ITimerUser
{
    [SerializeField] ShipInputReader input;
    [SerializeField] ForceMode fmode;
    [SerializeField] float thrustForce;
    [SerializeField] Rigidbody rb;
    [SerializeField] ForceMode rotateForceMode = ForceMode.Force;
    [SerializeField] float rotationScalar;
    [SerializeField] Transform camTransform;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] CurveValue thrustFOV;
    [SerializeField] private float defaultFOV = 70f;
    
    
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Vector3 rotationVector;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isRotating;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] bool isThrusting;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Quaternion camOffset => camTransform != null ? camTransform.rotation : Quaternion.identity;
    [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] float cachedFOV;


    private void Awake()
    {
        thrustFOV.SetInitialTime(1f);
        this.InitializeTimers((thrustFOV, false));
    }


    private void OnEnable()
    {
        input.Thrust += Thrust;
        input.Rotate += Rotate;
    }

    private void OnDisable()
    {
        input.Thrust -= Thrust;
        input.Rotate -= Rotate;

    }

    private void Update()
    {
        cam.Lens.FieldOfView = thrustFOV.Evaluate * defaultFOV;
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandleThrust();
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
        rb.AddForce(transform.up * thrustForce, fmode);
    }
    
    
    void Rotate(Vector3 rotation, bool active)
    {
        rotationVector = rotation;
        isRotating = active;
    }

    
    void Thrust(bool active)
    {
        isThrusting = active;

        if (active) thrustFOV.DynamicStart(Operation.Increase);
        else thrustFOV.DynamicStart(Operation.Decrease);
    }


    private void OnDestroy()
    {
        this.ShutdownTimers();
    }
}
