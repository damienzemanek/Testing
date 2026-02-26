using System;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;
using static IInputSubordinate<ShipInputAuthority.ShipInputMap,ShipInputAuthority.Subordinates>;
using static ShipInputAuthority;

[Serializable]
public class ShipController : MonoFacade<
        ShipController,
        ShipFunctionality, 
        ShipConfig, 
        ShipBlackboard,
        ShipContext,
        ShipActionMap>, 
    ITimerUser,
    IInputSubordinate<ShipInputMap, Subordinates>
{
    public ShipInputMap Input { get; set; }
    [field: SerializeField] [field: PropertyOrder(-1)] public SubordinateContext inputSubordinateContext { get; set; }
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
        Blackboard.cannonMouseLook.updateMouseLook = false;
        Blackboard.cannonCameraComponent.enabled = false;
    }
    
    
    private void OnDestroy()
    {
        this.ShutdownTimers();
        Blackboard.cannonProjectileSpawner.ShutdownTimers();
    }

}
