using System;
using EMILtools.Extensions;
using UnityEngine;

public class ClimbHandler : MonoBehaviour
{
    [SerializeField] TwoD_PilotController pilotController;

    public void CompleteClimb() => pilotController.GetFunctionality<IAPI_Climb>().CompleteClimb();

    void Awake()
    {
        pilotController.Ensure(this);
    }
}
