using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static TwoD_IA;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, IPlayerActions
{
    TwoD_IA ia;
    
    public UnityAction<bool> Move = delegate { };
    public UnityAction<bool> Run = delegate { };
    public UnityAction<bool> Look = delegate { };

    
    public UnityAction Jump = delegate { };
    public UnityAction Shoot = delegate { };
    public UnityAction Interact = delegate { };

    public Vector2 movement;
    public Vector2 mouse;

    private void OnEnable()
    {
        if (ia == null) ia = new TwoD_IA();
        
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
    }


    private void OnDisable()
    {
        ia.Player.Disable();
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
        Shoot?.Invoke();
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
        Interact?.Invoke();
    }
}
