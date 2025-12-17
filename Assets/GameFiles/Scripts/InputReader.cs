using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
   
    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };
    public event UnityAction EnableMouseControlCamera = delegate { };
    public event UnityAction DisableMouseControlCamera = delegate { };

    static PlayerInputActions _inputActions;
    PlayerInputActions inputActions { get => _inputActions; set => _inputActions = value; }

    public Vector3 Direction = (Vector3)_inputActions.Player.Move.ReadValue<Vector2>();

    private void OnEnable()
    {
        if(inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
    }


    public void OnFire(InputAction.CallbackContext context)
    {
        Move.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
    }

    private bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";


    public void OnMouseControlCamera(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started: EnableMouseControlCamera.Invoke(); break;
            case InputActionPhase.Canceled: DisableMouseControlCamera.Invoke(); break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //noop
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        //noop
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //noop
    }

}
