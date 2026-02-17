using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static EMILtools.Extensions.MouseLookEX;
using static PilotConfig;
using static TwoD_IA;
using static TwoD_InputAuthority;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, 
    IPlayerActions, 
    IInputReaderSubordinate<TwoD_InputMap, Subordinates>
{
    TwoD_IA ia;
    
    public TwoD_InputMap Input { get => subordinate.Input; }
    [ShowInInspector] public IInputSubordinate<TwoD_InputMap, Subordinates> subordinate { get; set; }
    
    
    public void Init()
    {
        if (ia == null) ia = new TwoD_IA();
        
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        if (Input.MouseInputZones == null)
        {
            Debug.LogWarning("MouseInputZones not initialized already... Initializing MouseCallbackZones for TwoD_InputReader");
            Input.MouseInputZones = ScriptableObject.CreateInstance<MouseCallbackZones>();
            Input.MouseInputZones.w = Screen.width;
            Input.MouseInputZones.h = Screen.height;
        }
        
        float halfScreenWidth = Input.MouseInputZones.w * 0.5f;
        float screenHeight = Input.MouseInputZones.h;
        Input.MouseInputZones.callbackZones = null;
        Input.MouseInputZones.AddInitalZones(
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { Input.FaceDirection.Invoke(LookDir.Left, true); Debug.Log("FaceDirection subscribers: " + Input.FaceDirection.Count);
            }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { Input.FaceDirection.Invoke(LookDir.Right, true);  Debug.Log("FaceDirection subscribers: " + Input.FaceDirection.Count);
            }));
    }

    private void OnDisable()
    {
        if(ia != null) ia.Player.Disable();
        subordinate = null;
    }
    
    

    public void OnMove(InputAction.CallbackContext context)
    {
        if (ia.Player.Move.IsPressed()) Input.Move?.Invoke(context.ReadValue<Vector2>(), true); 
        switch (context.phase)
        {
            case InputActionPhase.Canceled: Input.Move?.Invoke(Vector2.zero, false); break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: 
                Input.mouse = Mouse.current.position.ReadValue();
                Input.Look?.Invoke(true); break;
            case InputActionPhase.Canceled: 
                Input.Look?.Invoke(false); break;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Input.Shoot?.Invoke(true); break;
            case InputActionPhase.Canceled: Input.Shoot?.Invoke(false); break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Input.Run?.Invoke(true); break;
            case InputActionPhase.Canceled: Input.Run?.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Input.Jump?.Invoke();
    }

    public void OnInteractHeld(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("HELD INTERACT");
            Input.HoldInteract.PrintInvokeListNames();
            Input.HoldInteract?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Input.Interact?.Invoke();
    }

    public void OnCallInTitan(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Input.CallInTitan?.Invoke();
    }
    
}
