using UnityEngine;

public class ClimbHandler : MonoBehaviour
{
    [SerializeField] TwoD_Controller controller;

    public void CompleteClimb() => controller.GetFunctionality<IAPI_Climb>().CompleteClimb();
}
