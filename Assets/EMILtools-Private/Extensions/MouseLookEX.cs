using System;
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
            [BoxGroup("ReadOnly")] [SerializeField, ReadOnly]  Vector2 rot;
            
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

    }

}