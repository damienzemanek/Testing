using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class MouseLookEX
    {
        const int SENSITIVITY_ADJUSTMENT = 5;
        
        [Serializable]
        public class MouseLookSettings
        {
            [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 look;
            [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 rot;
            
            [BoxGroup("References")] [SerializeField] [ShowIf("useBody")] Transform body;
            [BoxGroup("References")] [SerializeField] Transform head;
            [BoxGroup("References")] [SerializeField] IInputMouseLookReference input;
            
            [BoxGroup("Settings")] [SerializeField] public bool useBody = true;
            [BoxGroup("Settings")] [SerializeField] public bool clampXRotation = false;
            [BoxGroup("Settings")] [SerializeField] public bool clampYRotation = true;
            [BoxGroup("Settings")] [SerializeField] public bool updateMouseLook = true;
            [BoxGroup("Settings")] [SerializeField] Vector2 sensitivity = new Vector2(1, 1);
            [BoxGroup("Settings")] [SerializeField] [ShowIf("clampXRotation")] Vector2 clampX = new Vector2(-90f, 90f);
            [BoxGroup("Settings")] [SerializeField] [ShowIf("clampYRotation")] Vector2 clampY = new Vector2(-90f, 90f);

            public void UpdateMouseLook()
            {
                if (!updateMouseLook) return;
                
                // Grab the input
                Vector2 mouseInput = input.Value.mouse; //Debug.Log("Mouse Input: " + mouseInput);
                
                // Apply sensitivity
                look = mouseInput * sensitivity / SENSITIVITY_ADJUSTMENT;
                look.y *= -1;

                // Apply the rotation to the variable
                rot.x += look.x;
                rot.y += look.y;
                if(clampXRotation) rot.x = Mathf.Clamp(rot.x, clampX.x, clampX.y);
                if(clampYRotation) rot.y = Mathf.Clamp(rot.y, clampY.x, clampY.y);
                
                // Use the variable on the transforms
                if(useBody) body.localRotation = Quaternion.Euler(0, rot.x, 0);
                head.transform.localRotation = Quaternion.Euler(rot.y, rot.x, 0);
            }
        }


        [Serializable]
        public class RotateToMouseWorldSpace
        {
            [Serializable]
            public struct RotatingObject
            {
                public Transform transform;
                public bool flipX;
                public bool flipY;
                public bool flipZ;

                public bool clampX;
                public bool clampY;
                public bool clampZ;

                public Vector2 clampXrot;
                public Vector2 clampYrot;
                public Vector2 clampZrot;
            }
            
            public RotatingObject[] rotatingObjects;
            public Camera cam;
            public float maximumLength;
            
            private Ray rayMouse;
            private Vector3 direction;
            private Quaternion rotation;

            public void LateUpdateMouseLook()
            {
                RaycastHit hit;
                var mousePos = Input.mousePosition;
                rayMouse = cam.ScreenPointToRay(mousePos);
                Debug.DrawRay(rayMouse.origin, rayMouse.direction * maximumLength, Color.red);
                if(Physics.Raycast (rayMouse.origin, rayMouse.direction, out hit, maximumLength))
                {
                    RotateToMouseDirection(rotatingObjects, hit.point);
                }
                else
                {
                    var pos = rayMouse.GetPoint(maximumLength);
                    RotateToMouseDirection(rotatingObjects, pos);
                }
            }

            void RotateToMouseDirection (RotatingObject[] transform, Vector3 destination)
            {
                foreach (var ro in transform)
                {
                    direction = destination - ro.transform.position;

                    if (ro.flipX) direction.x *= -1;
                    if (ro.flipY) direction.y *= -1;
                    if (ro.flipZ) direction.z *= -1;

                    // Base rotation
                    rotation = Quaternion.LookRotation(direction, Vector3.up);

                    // Convert to euler
                    Vector3 euler = rotation.eulerAngles;

                    // Normalize for clamping
                    euler.x = NormalizeAngle(euler.x);
                    euler.y = NormalizeAngle(euler.y);
                    euler.z = NormalizeAngle(euler.z);

                    // Clamp
                    if (ro.clampX) euler.x = Mathf.Clamp(euler.x, ro.clampXrot.x, ro.clampXrot.y);
                    if (ro.clampY) euler.y = Mathf.Clamp(euler.y, ro.clampYrot.x, ro.clampYrot.y);
                    if (ro.clampZ) euler.z = Mathf.Clamp(euler.z, ro.clampZrot.x, ro.clampZrot.y);

                    // Rebuild quaternion
                    ro.transform.localRotation = Quaternion.Euler(euler);
                }
            }
            
            
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

    }
    
    

}