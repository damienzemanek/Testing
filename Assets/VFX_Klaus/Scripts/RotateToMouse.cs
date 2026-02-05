using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    public Camera cam;
    public float maximumLength;

    public bool flipX;
    public bool flipY;
    public bool flipZ;

    private Ray rayMouse;
    private Vector3 pos;
    private Vector3 direction;
    private Quaternion rotation;

    void LateUpdate()
    {
        if(cam != null)
        {
            
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            rayMouse = cam.ScreenPointToRay(mousePos);
            Debug.DrawRay(rayMouse.origin, rayMouse.direction * maximumLength, Color.red);
            if(Physics.Raycast (rayMouse.origin, rayMouse.direction, out hit, maximumLength))
            {
                RotateToMouseDirection(gameObject, hit.point);
            }
            else
            {
                var pos = rayMouse.GetPoint(maximumLength);
                RotateToMouseDirection(gameObject, pos);
            }
        }
        else
        {
            Debug.Log("No Camera");
        }
    }

    void RotateToMouseDirection (GameObject obj, Vector3 destination)
    {
        direction = destination - obj.transform.position;
        if(flipX) direction.x *= -1;
        if (flipY) direction.y *= -1; 
        if(flipZ) direction.z *= -1;
        rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }

    public Quaternion GetRotation()
    {
        return rotation;
    }
}
