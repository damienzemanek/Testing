using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static EMILtools.Extensions.MouseLookEX;
using static TwoD_Config;
using static TwoD_IA;
using static TwoD_InputAuthority;
using static TwoDimensionalController;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, IPlayerActions, IInputReader<TwoD_InputMap>, IInitializable
{
    TwoD_IA ia;
    [field: NonSerialized] [field: ShowInInspector] [field: ReadOnly] public TwoD_InputMap InputMap { get; set; }
    
    public void Init()
    {
        if (ia == null) ia = new TwoD_IA();
        
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        if (InputMap.MouseInputZones == null)
        {
            InputMap.MouseInputZones = ScriptableObject.CreateInstance<MouseCallbackZones>();
            InputMap.MouseInputZones.w = Screen.width;
            InputMap.MouseInputZones.h = Screen.height;
            float halfScreenWidth = InputMap.MouseInputZones.w * 0.5f;
            float screenHeight = InputMap.MouseInputZones.h;
            InputMap.MouseInputZones.callbackZones = null;
            InputMap.MouseInputZones.AddInitalZones(
                (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { InputMap.FaceDirection.Invoke(LookDir.Left, true); Debug.Log("FaceDirection subscribers: " + InputMap.FaceDirection.Count);
                }),
                (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { InputMap.FaceDirection.Invoke(LookDir.Right, true);  Debug.Log("FaceDirection subscribers: " + InputMap.FaceDirection.Count);
                }));
        }

    }

    private void OnDisable()
    {
        if(ia != null) ia.Player.Disable();
        if(InputMap != null && InputMap.MouseInputZones != null) InputMap.MouseInputZones.callbackZones = null;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (ia.Player.Move.IsPressed())
        {
            InputMap.movement = context.ReadValue<Vector2>();
            InputMap.Move?.Invoke(true); 
        }
        switch (context.phase)
        {
            case InputActionPhase.Canceled: 
                InputMap.Move?.Invoke(false); break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: 
                InputMap.mouse = Mouse.current.position.ReadValue();
                InputMap.Look?.Invoke(true); break;
            case InputActionPhase.Canceled: 
                InputMap.Look?.Invoke(false); break;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: InputMap.Shoot?.Invoke(true); break;
            case InputActionPhase.Canceled: InputMap.Shoot?.Invoke(false); break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: InputMap.Run?.Invoke(true); break;
            case InputActionPhase.Canceled: InputMap.Run?.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) InputMap.Jump?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) InputMap.Interact?.Invoke();
    }

    public void OnCallInTitan(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) InputMap.CallInTitan?.Invoke();
    }
    
}
