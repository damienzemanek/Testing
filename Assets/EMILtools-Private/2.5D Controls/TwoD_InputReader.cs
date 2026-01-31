using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "2D Input Reader", menuName = "ScriptableObjects/2D Input Reader")]
public class TwoD_InputReader : ScriptableObject, TwoD_IA.IPlayerActions
{
    TwoD_IA ia;
    
    public UnityAction MoveStart = delegate { };
    public UnityAction MoveStop = delegate { };
    
    public UnityAction RunStart = delegate { };
    public UnityAction RunStop = delegate { };

    public UnityAction Look = delegate { };
    public UnityAction Shoot = delegate { };
    public UnityAction Jump = delegate { };
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
                MoveStart?.Invoke();
                break;
            case InputActionPhase.Canceled:
                MoveStop?.Invoke();
                break;
        }

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouse = context.ReadValue<Vector2>();
        Look?.Invoke();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        Shoot?.Invoke();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: RunStart?.Invoke(); break;
            case InputActionPhase.Canceled: RunStop?.Invoke(); break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Interact?.Invoke();
    }
}
