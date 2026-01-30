using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class MouseLookEX
    {
        [Serializable]
        public class MouseLookSettings
        {
            [SerializeField] Transform body;
            [SerializeField] Transform head;
            [SerializeField] IInputMouseLookReference input;
            [SerializeField] Vector2 sensitivity;
            [SerializeField] Vector2 look;
            [SerializeField] Vector2 rot;
            [SerializeField] bool updateMouseLook = true;

            public void UpdateMouseLook()
            {
                if (!updateMouseLook) return;
                
                // Grab the input
                Vector2 mouseInput = input.Value.mouse; //Debug.Log("Mouse Input: " + mouseInput);
                
                // Apply sensitivity
                look = mouseInput * sensitivity / 5;
                look.y *= -1;

                // Apply the rotation to the variable
                rot.x += look.x;
                rot.y += look.y;
                rot.y = Mathf.Clamp(rot.y, -90f, 90f);
                
                // Use the variable on the transforms
                body.rotation = Quaternion.Euler(0, rot.x, 0);
                head.transform.rotation = Quaternion.Euler(rot.y, rot.x, 0);
            }
        }

    }

}