using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.MouseCallbackZones;
using static EMILtools.Extensions.MouseLookEX;

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
        public class PositionToMouseWorldSpace
        {
            public Camera cam;
            public MouseToWorldSpace core;
            
            bool isLerp => moveOptions == MoveOptions.Lerp;
            
            [BoxGroup("References")] public Transform objectToMove;
            public enum MoveOptions { Teleport, Lerp }

            public Vector3 offset = Vector3.zero;
            public bool lockX;
            public bool lockY;
            public bool lockZ;
            [ShowIf("lockX")] public float lockPosX;
            [ShowIf("lockY")]public float lockPosY;
            [ShowIf("lockZ")]public float lockPosZ;
            [ShowIf("isLerp")] public float lerpSpeed = 5f;

            public MoveOptions moveOptions;
            
            public void Execute()
            {
                switch (moveOptions)
                {
                    case MoveOptions.Teleport: objectToMove.position = PositionalInteroggative(); break;
                    case MoveOptions.Lerp: objectToMove.position = Vector3.Lerp(objectToMove.position, PositionalInteroggative() + offset, Time.deltaTime * lerpSpeed); break;
                }
            }

            Vector3 PositionalInteroggative()
            {
                Vector3 ret = core.GetHitPoint(cam) + offset;
                if(lockX) ret.x = lockPosX;
                if(lockY) ret.y = lockPosY;
                if(lockZ) ret.z = lockPosZ;
                return ret;
            }
        }


        [Serializable]
        public class RotateToMouseWorldSpace
        {
            
            [Serializable]
            public struct RotatingObject
            {
                public Transform transform;
                
                public bool rotateX;
                public bool rotateY;
                public bool rotateZ;
                
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

            public Camera cam;
            public MouseToWorldSpace core;
            [BoxGroup("References")] public RotatingObject[] rotatingObjects;
            [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public Quaternion rotation;



            public void Execute()
            => RotateToMouseDirection(rotatingObjects, core.GetHitPoint(cam));

            void RotateToMouseDirection (RotatingObject[] transform, Vector3 destination)
            {
                if (rotatingObjects == null) return;
                
                foreach (var ro in transform)
                {
                    core.direction = destination - ro.transform.position;

                    if (ro.flipX) core.direction.x *= -1;
                    if (ro.flipY) core.direction.y *= -1;
                    if (ro.flipZ) core.direction.z *= -1;

                    // Base rotation
                    rotation = Quaternion.LookRotation(core.direction, Vector3.up);

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
                    
                    
                    // Rotate Angle toggles
                    Vector3 current = ro.transform.localEulerAngles;
                    current.x = NormalizeAngle(current.x);
                    current.y = NormalizeAngle(current.y);
                    current.z = NormalizeAngle(current.z);
                    if(!ro.rotateX) euler.x = current.x;
                    if(!ro.rotateY) euler.y = current.y;
                    if(!ro.rotateZ) euler.z = current.z;

                    // Rebuild quaternion & apply
                    ro.transform.localRotation = Quaternion.Euler(euler);
                }
            }
            
            public static implicit operator MouseToWorldSpace(RotateToMouseWorldSpace self) => self.core;
            
            
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

    }
    
    

}