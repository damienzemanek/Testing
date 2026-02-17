using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static Ship_IA;
using static ShipController;
using static ShipInputAuthority;

[Serializable]
[CreateAssetMenu(fileName = "ShipController", menuName = "ScriptableObjects/Ship Controller")]
public class ShipInputReader : ScriptableObject,
    IPlayerActions,
    IInputReaderSubordinate<ShipInputMap, Subordinates>
{
    public Ship_IA ia;
    public ShipInputMap Input => subordinate.Input;
    public IInputSubordinate<ShipInputMap, Subordinates> subordinate { get; set; }
    
    public void Init()
    {
        ia = new Ship_IA();
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
    }
    
    public void OnRotate(InputAction.CallbackContext context)
    {
        if (ia.Player.Rotate.IsPressed()) DoRotate();
        
        switch (context.phase) 
        {
            case InputActionPhase.Canceled: Input.Rotate?.Invoke(Vector3.zero, false); break; 
        }


        void DoRotate()
        {
            Vector2 v = context.ReadValue<Vector2>();
            Input.rotation = new Vector3(v.y, 0f, -v.x);
            Input.Rotate?.Invoke(Input.rotation, true);
        }
    }
    

    public void OnLook(InputAction.CallbackContext context)
    {
        if (ia.Player.Look.IsPressed())
        {
            Input.mouse = context.ReadValue<Vector2>();
            Input.MouseLook.Invoke(true);
        }
        else
        {
            Input.MouseLook.Invoke(false);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Input.Fire.Invoke(true); break;
            case InputActionPhase.Canceled: Input.Fire.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Input.Thrust?.Invoke(true); break;
            case InputActionPhase.Canceled: Input.Thrust?.Invoke(false); break;
        }
    }

    public void OnSwitchCam(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Input.SwitchCam?.Invoke();
    }
    
}
