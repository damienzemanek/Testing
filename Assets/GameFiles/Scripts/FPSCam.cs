using System;
using EMILtools.Extensions;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;

public class FPSCam : MonoBehaviour
{
    [SerializeField] InputReader input;
    [SerializeField] MouseLookSettings mouselook;

    private void Start()
    {
        input.EnablePlayerActions();
        CursorEX.Set(false, CursorLockMode.Locked);
    }

    private void Update()
    {
        mouselook.UpdateMouseLook();
    }

}
