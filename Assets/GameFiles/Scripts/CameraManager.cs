using EMILtools.Extensions;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : ValidatedMonoBehaviour
{
    [Header(header: "Refs")]
    //[SerializeField, Anywhere] InputReader input;
    [SerializeField, Anywhere] CinemachineOrbitalFollow orbital;
    [SerializeField, Anywhere] CinemachineInputAxisController camInput;

    [Header(header: "Settings")]
    [SerializeField] bool requireRMBtoMoveCam;

    bool isRMBPressed;


    private void OnEnable()
    {
       // input.EnableMouseControlCamera += OnEnableMouseControlCamera;
       // input.DisableMouseControlCamera += OnDisableMouseControlCamera;
    }

    private void OnDisable()
    {
       // input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
       // input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
    }

    private void OnDisableMouseControlCamera()
    {
        if (requireRMBtoMoveCam)
        {
            isRMBPressed = camInput.enabled = false;
            CursorEX.Set(true, CursorLockMode.Confined);
        }
        else
        {
            camInput.enabled = true;
            CursorEX.Set(false, CursorLockMode.Locked);
        }
    }

    private void OnEnableMouseControlCamera()
    {
        if (requireRMBtoMoveCam)
        {
            isRMBPressed = camInput.enabled = true;
            CursorEX.Set(false, CursorLockMode.Locked);
        }
        else
        {
            camInput.enabled = true;
            CursorEX.Set(false, CursorLockMode.Locked);
        }

    }

}
