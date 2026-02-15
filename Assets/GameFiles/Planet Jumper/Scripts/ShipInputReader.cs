using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static Ship_IA;
using static ShipInputReader.ShipInputAuthority;

[Serializable]
[CreateAssetMenu(fileName = "ShipController", menuName = "ScriptableObjects/Ship Controller")]
public class ShipInputReader : ScriptableObject, IPlayerActions, IInputMouseLook, IInputReader<ShipInputReader.ShipInputAuthority.ShipInputMap>
{
    public class ShipInputAuthority : IInputAuthority<ShipInputAuthority.ShipInputMap, ShipInputAuthority.Subordinates>
    {
        public enum Subordinates
        {
            Ship = 0
        }
        
        public class ShipInputMap : IInputMap { }

        public Dictionary<int, IInputAuthority<ShipInputMap, Subordinates>.Mapping> InputMappings { get; set; }
        void IInputAuthority<ShipInputMap, Subordinates>.DelegateAuthorityTo(int mapIndex, IInputAuthority<ShipInputMap, Subordinates>.Mapping mapping)
        {
            throw new NotImplementedException();
        }
        
    }
    
    public Ship_IA ia;

    public PersistentAction<bool> Thrust = new PersistentAction<bool>();
    public PersistentAction<Vector3, bool> Rotate = new PersistentAction<Vector3, bool>();
    public PersistentAction<bool> Fire = new  PersistentAction<bool>();


    public PersistentAction Move;
    public PersistentAction SwitchCam;

    public Vector3 rotation;
    public Vector2 mouse { get; set; }

    private void OnEnable()
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
            case InputActionPhase.Canceled: Rotate?.Invoke(Vector3.zero, false); break; 
        }


        void DoRotate()
        {
            Vector2 v = context.ReadValue<Vector2>();
            rotation = new Vector3(v.y, 0f, -v.x);
            Rotate?.Invoke(rotation, true);
        }
    }
    

    public void OnLook(InputAction.CallbackContext context)
    {
        mouse = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Fire.Invoke(true); break;
            case InputActionPhase.Canceled: Fire.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Thrust?.Invoke(true); break;
            case InputActionPhase.Canceled: Thrust?.Invoke(false); break;
        }
    }

    public void OnSwitchCam(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) SwitchCam?.Invoke();
    }

    public ShipInputAuthority.ShipInputMap InputMap { get; set; }
}
