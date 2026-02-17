using System;
using EMILtools.Extensions;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX;

public class FPSCam : MonoBehaviour
{
    [SerializeField] MouseLookSettings mouselook;
    
    private void Update()
    {
        mouselook.UpdateMouseLook();
    }

}
