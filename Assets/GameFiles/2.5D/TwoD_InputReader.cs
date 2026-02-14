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
using static TwoDimensionalController;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, IPlayerActions, IInputReader, IFacadeCompositionElement<TwoD_Controller>
{
    public TwoD_Controller facade { get; set; }

    
    TwoD_IA ia;

    // Layer 1 -> Invoked by Player Input
    public PersistentAction<bool> Move = new();
    public PersistentAction<bool> Run = new();
    public PersistentAction<bool> Look = new();
    public PersistentAction<bool> Shoot = new();
    public PersistentAction<LookDir, bool> FaceDirection = new();
    public PersistentAction Jump = new();
    public PersistentAction Interact = new();
    public PersistentAction CallInTitan = new();
    
    
    // Layer 2 -> Invoked in a Functionality Module
    public PersistentAction UnMantleLedge = new();
    public PersistentAction MantleLedge = new();
    public PersistentAction DoubleJump = new();
    public PersistentAction ClimbLedge = new();
    public PersistentAction<bool> Land = new();

    
    public Vector2 movement;
    public Vector2 mouse;
    [BoxGroup("Orientation")] public MouseCallbackZones mouseZones;

    public SimpleGuarderMutable _lookGuarder = new SimpleGuarderMutable();
    [ShowInInspector] public LazyGuarderMutable mouseZoneGuarder = new();
    
    

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
            (new Rect(0              , 0, halfScreenWidth, screenHeight), () => { FaceDirection.Invoke(LookDir.Right, true); Debug.Log("FaceDirection subscribers: " + FaceDirection.Count);
            }),
            (new Rect(halfScreenWidth, 0, halfScreenWidth, screenHeight), () => { FaceDirection.Invoke(LookDir.Left, true);  Debug.Log("FaceDirection subscribers: " + FaceDirection.Count);
            }));
    }


    private void OnDisable()
    {
        ia.Player.Disable();
        mouseZones.callbackZones = null;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (ia.Player.Move.IsPressed())
        {
            movement = context.ReadValue<Vector2>();
            Move?.Invoke(true); 
        }
        switch (context.phase)
        {
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
