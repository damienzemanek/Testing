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
using static ShipInputAuthority;

[Serializable]
public class ShipController : MonoFacade<ShipController, ShipFunctionality, ShipConfig, ShipBlackboard, ShipActionMap>, 
    ITimerUser,
    IInputSubordinate<ShipInputMap, Subordinates>
{
    [BoxGroup("Mouse")] [PropertyOrder(-1)] [SerializeField] public MouseLookSettings cannonMouseLook;
    public ShipInputMap Input { get; set; }
    public IInputSubordinate<ShipInputMap, Subordinates>.SubordinateContext context { get; set; }
    public ShipInputMap InjectInputMap() => new ("Ship Input Map");
    
    public void InitSubordinate()
    {
        InitializeFacade();   
    }

    public void OnAuthorityReceived()
    {
        CursorEX.Set(false, CursorLockMode.Locked);
        Functionality.Bind();
    }

    public void OnAuthorityLost()
    {
        Functionality.Unbind();
    }
    
    void Start()
    {
        CursorEX.Set(false, CursorLockMode.Locked);
        cannonMouseLook.updateMouseLook = false;
        Blackboard.cannonCameraComponent.enabled = false;
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

}
