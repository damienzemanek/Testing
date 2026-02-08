using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Extensions
{
    [Serializable]
    [CreateAssetMenu(fileName = "MouseToWorldSpace", menuName = "ScriptableObjects/Mouse/MouseToWorldSpace")]
    public class MouseToWorldSpace : ScriptableObject
    {
        [BoxGroup("Settings")] [SerializeField] public float maximumLength;
        [BoxGroup("Settings")] [SerializeField] public bool lookAtCollisions = false;
        [BoxGroup("Settings")] [SerializeField] public bool lookAtPlane = false;
        [BoxGroup("Settings")] [SerializeField] [ShowIf("lookAtPlane")]  public LayerMask lookAtPlaneMask;
            
        [BoxGroup("ReadOnly")] [ReadOnly] [ShowIf("lookAtPlane")] public Vector3 contactPoint; 
        [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public Vector3 direction;
        public Ray rayMouse;
        
        public Vector3 GetHitPoint(Camera cam)
        {
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            rayMouse = cam.ScreenPointToRay(mousePos);
            Debug.DrawRay(rayMouse.origin, rayMouse.direction * maximumLength, Color.red);
            int layermask = 0;
            if(lookAtPlane) layermask = lookAtPlaneMask.value;
            if(Physics.Raycast (rayMouse.origin, rayMouse.direction, out hit, maximumLength, layermask))
            {
                if (lookAtCollisions || lookAtPlane)
                    return contactPoint = hit.point;
            }
            // Looks at the direction max length point by default
                
            var pos = rayMouse.GetPoint(maximumLength);
            contactPoint = Vector3.zero;
            return pos;
        }
    }

}

