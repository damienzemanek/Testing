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
using static TwoDimensionalController;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, IPlayerActions, IInputReader<TwoD_InputMap, Subordinates>, IInitializable
{
    TwoD_IA ia;

    [ShowInInspector] public IInputSubordinate<TwoD_InputMap, Subordinates> subordinate { get; set; }
    
    
    public void Init()
    {
        if (ia == null) ia = new TwoD_IA();
        
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        if (subordinate.Input.MouseInputZones == null)
        {
            Debug.LogWarning("MouseInputZones not initialized already... Initializing MouseCallbackZones for TwoD_InputReader");
            subordinate.Input.MouseInputZones = ScriptableObject.CreateInstance<MouseCallbackZones>();
            subordinate.Input.MouseInputZones.w = Screen.width;
            subordinate.Input.MouseInputZones.h = Screen.height;
        }
        
        float halfScreenWidth = subordinate.Input.MouseInputZones.w * 0.5f;
        float screenHeight = subordinate.Input.MouseInputZones.h;
        subordinate.Input.MouseInputZones.callbackZones = null;
        subordinate.Input.MouseInputZones.AddInitalZones(
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { subordinate.Input.FaceDirection.Invoke(LookDir.Left, true); Debug.Log("FaceDirection subscribers: " + subordinate.Input.FaceDirection.Count);
            }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { subordinate.Input.FaceDirection.Invoke(LookDir.Right, true);  Debug.Log("FaceDirection subscribers: " + subordinate.Input.FaceDirection.Count);
            }));
    }

    private void OnDisable()
    {
        if(ia != null) ia.Player.Disable();
        subordinate = null;
    }
    
    

    public void OnMove(InputAction.CallbackContext context)
    {
        if (ia.Player.Move.IsPressed())
        {
            subordinate.Input.Move?.Invoke(context.ReadValue<Vector2>(), true); 
            Debug.Log("Move Inpuutedd : " + context.ReadValue<Vector2>());
        }
        switch (context.phase)
        {
            case InputActionPhase.Canceled: 
                subordinate.Input.Move?.Invoke(Vector2.zero, false); break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: 
                subordinate.Input.mouse = Mouse.current.position.ReadValue();
                subordinate.Input.Look?.Invoke(true); break;
            case InputActionPhase.Canceled: 
                subordinate.Input.Look?.Invoke(false); break;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        Debug.Log("shooting");
        
        Debug.Log(subordinate);
        Debug.Log(subordinate.Input);
        Debug.Log(subordinate.Input.Shoot);
        subordinate.Input.Shoot.PrintInvokeListNames();
        switch (context.phase)
        {
            case InputActionPhase.Started: subordinate.Input.Shoot?.Invoke(true); break;
            case InputActionPhase.Canceled: subordinate.Input.Shoot?.Invoke(false); break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: subordinate.Input.Run?.Invoke(true); break;
            case InputActionPhase.Canceled: subordinate.Input.Run?.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) subordinate.Input.Jump?.Invoke();
    }

    public void OnInteractHeld(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("HELD INTERACT");
            subordinate.Input.HoldInteract.PrintInvokeListNames();
            subordinate.Input.HoldInteract?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) subordinate.Input.Interact?.Invoke();
    }

    public void OnCallInTitan(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) subordinate.Input.CallInTitan?.Invoke();
    }
    
}
