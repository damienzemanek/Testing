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
using static ShipController;
using static ShipFunctionality;

[Serializable]
public class ShipController : MonoFacade<ShipController, ShipFunctionality, ShipConfig, ShipBlackboard, ShipActionMap> , ITimerUser
{
    [BoxGroup("Mouse")] [PropertyOrder(-1)] [SerializeField] public MouseLookSettings cannonMouseLook;
    
    
    void Awake()
    {
        InitializeFacade();   
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
        Functionality.Bind();
    }

    private void OnDisable()
    {
        Functionality.Unbind();
    }

    protected override void Update()
    {
        base.Update();
        cannonMouseLook.UpdateMouseLook();
    }
    
    
    private void OnDestroy()
    {
        this.ShutdownTimers();
        Blackboard.cannonProjectileSpawner.ShutdownTimers();
    }
    
    public class ShipActionMap : IActionMap { }
}
