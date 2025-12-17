using UnityEngine;
using KBCore.Refs;
using Unity.Cinemachine;
using System;

public class PlayerController : ValidatedMonoBehaviour
{
    [Header("References")]
    [SerializeField, Self] CharacterController controller;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineCamera freelookCam;
    [SerializeField, Anywhere] InputReader input;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float rotSpeed = 15f;
    [SerializeField] float smoothTime = 0.2f;

    Transform mainCam;

    const float ZeroF = 0f;

    float currentSpeed;
    float currentVelocity;

    private void Awake()
    {
        mainCam = Camera.main.transform;
        freelookCam.Follow = transform;
        freelookCam.LookAt = transform;
        
        freelookCam.OnTargetObjectWarped(
            transform, 
            transform.position - mainCam.position
        );
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        var moveDir = new Vector3(input.Direction.x, input.Direction.y).normalized;

        //Rotate movemente dir to match camera rotation
        var adjustedDir = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * moveDir;

        if (adjustedDir.magnitude > ZeroF)
        {
            //Adjust rotation to match movement direction
            var targetRot = Quaternion.LookRotation(adjustedDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            transform.LookAt(transform.position + adjustedDir);

            //Moving the player
            var adjustedMovement = adjustedDir * (moveSpeed * Time.deltaTime);
            controller.Move(adjustedMovement);

            currentSpeed = Mathf.SmoothDamp(currentSpeed, adjustedMovement.magnitude, ref currentVelocity, smoothTime);
        }
        else
            currentSpeed = Mathf.SmoothDamp(currentSpeed, ZeroF, ref currentVelocity, smoothTime);
        

    }
}
