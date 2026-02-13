using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static EMILtools.Extensions.MouseLookEX;
using static TwoD_IA;
using static TwoDimensionalController;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, IPlayerActions
{
    TwoD_IA ia;
    
    public UnityAction<bool> Move = delegate { };
    public UnityAction<bool> Run = delegate { };
    public UnityAction<bool> Look = delegate { };
    public UnityAction<bool> Shoot = delegate { };

    
    public UnityAction<LookDir> FaceDirection = delegate { };

    
    public UnityAction Jump = delegate { };
    public UnityAction Interact = delegate { };
    public UnityAction CallInTitan = delegate { };

    public Vector2 movement;
    public Vector2 mouse;
    [BoxGroup("Orientation")] public MouseCallbackZones mouseZones;

    public SimpleGuarderMutable _lookGuarder = new SimpleGuarderMutable();
    [ShowInInspector] public SimpleGuarderMutable _mouseZoneGuarder;
    
    

    private void OnEnable()
    {
        if (ia == null) ia = new TwoD_IA();
        
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
        
        // Looking at the player from the front, reverses the directions (like a mirror)
        float halfScreenWidth = mouseZones.w * 0.5f;
        float screenHeight = mouseZones.h;
        mouseZones.callbackZones = null;
        mouseZones.AddInitalZones(
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { FaceDirection(LookDir.Right); Debug.Log("FaceDirection subscribers: " + FaceDirection?.GetInvocationList().Length);
            }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { FaceDirection(LookDir.Left);  Debug.Log("FaceDirection subscribers: " + FaceDirection?.GetInvocationList().Length);
            }));
    }


    private void OnDisable()
    {
        ia.Player.Disable();
        mouseZones.callbackZones = null;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: 
                movement = context.ReadValue<Vector2>();
                Move?.Invoke(true); break;
            case InputActionPhase.Canceled: 
                Move?.Invoke(false); break;
        }

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: 
                mouse = Mouse.current.position.ReadValue();
                Look?.Invoke(true); break;
            case InputActionPhase.Canceled: 
                Look?.Invoke(false); break;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Shoot?.Invoke(true); break;
            case InputActionPhase.Canceled: Shoot?.Invoke(false); break;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: Run?.Invoke(true); break;
            case InputActionPhase.Canceled: Run?.Invoke(false); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Jump?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) Interact?.Invoke();
    }

    public void OnCallInTitan(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed) CallInTitan?.Invoke();
    }
}
