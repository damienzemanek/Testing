using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.MouseLookEX.MouseCallbackZones;

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
        public class MouseCallbackZones
        { 
            public float w = Screen.width;
            public float h = Screen.height;
            
            [Serializable]
            public class CallbackZone
            {
                public Rect zone;
                [ShowInInspector, ReadOnly] bool wasInside;
                public PersistentAction callback;
                
                public void CheckZone(Vector2 mousePos)
                {
                    bool inside = zone.Contains(mousePos);
                    if(inside && !wasInside) callback.Invoke();
                    wasInside = inside;
                }

                public CallbackZone(Rect zone)
                {
                    this.zone = zone;
                    wasInside = false;
                    callback = new PersistentAction();
                }
            }

            public void CheckAllZones(Vector2 mousePos)
            {
                if(callbackZones == null) Debug.LogError("No callback zones found, make sure to add some with AddInitialZones or AddZone");
                foreach (var zone in callbackZones)
                    zone.CheckZone(mousePos);
            }
            
            [BoxGroup("References")] public List<CallbackZone> callbackZones;
        }
        
        /// <summary>
        /// Add all zones needed in the beginning
        /// </summary>
        /// <param name="zones"></param>
        /// <param name="zonesToAdd"></param>
        public static void AddInitalZones(this MouseCallbackZones zones, params (Rect rect, Action method)[] zonesToAdd)
        {
            if (zones.callbackZones == null) zones.callbackZones = new List<CallbackZone>();
            
            for(int i = 0; i < zonesToAdd.Length; i++)
                zones.callbackZones.AddGet(new CallbackZone(zonesToAdd[i].rect))
                    .callback.Add(zonesToAdd[i].method);

        }
        
         /// <summary>
         /// Add a zone to the list of zones, can be used at runtime
         /// </summary>
         /// <param name="zones"></param>
         /// <param name="rect"></param>
         /// <param name="method"></param>
        public static void AddZone(this MouseCallbackZones zones, Rect rect, Action method)
        {
            if (zones.callbackZones == null) zones.callbackZones = new List<CallbackZone>();
            zones.callbackZones.AddGet(new CallbackZone(rect)).callback.Add(method);
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

                [ShowIf("clampX")] public Vector2 clampXrot;
                [ShowIf("clampY")] public Vector2 clampYrot;
                [ShowIf("clampZ")] public Vector2 clampZrot;
            }
            
            
            [BoxGroup("References")] public RotatingObject[] rotatingObjects;
            [BoxGroup("References")] public Camera cam;
            
            [BoxGroup("Settings")] [SerializeField] float maximumLength;
            [BoxGroup("Settings")] [SerializeField] public bool lookAtCollisions = false;
            [BoxGroup("Settings")] [SerializeField] public bool lookAtDirection = true;
            [BoxGroup("Settings")] [SerializeField] public bool lookAtPlane = false;
            [BoxGroup("Settings")] [SerializeField] [ShowIf("lookAtPlane")]  public LayerMask lookAtPlaneMask;
            
            [BoxGroup("ReadOnly")] [ReadOnly] [ShowIf("lookAtPlane")] public Vector3 contactPoint; 
            [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Vector3 direction;
            [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] Quaternion rotation;
            Ray rayMouse;


            public void LateUpdateMouseLook()
            {
                RaycastHit hit;
                var mousePos = Input.mousePosition;
                rayMouse = cam.ScreenPointToRay(mousePos);
                Debug.DrawRay(rayMouse.origin, rayMouse.direction * maximumLength, Color.red);
                int layermask = 0;
                if(lookAtPlane) layermask = lookAtPlaneMask.value;
                if(Physics.Raycast (rayMouse.origin, rayMouse.direction, out hit, maximumLength, layermask))
                {
                    if(lookAtCollisions || lookAtPlane) RotateToMouseDirection(rotatingObjects,
                        contactPoint = hit.point);
                }
                else if(lookAtDirection)
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