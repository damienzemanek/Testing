using System;
using EMILtools.Extensions;
using EMILtools.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using static RotateExtension;

public class RotateModule : MonoBehaviour
{
    public ConstantRotate ConstantRotateModule;
    public RandomRotation RandomRotationModule;

    private void Start()
    {
        if (RandomRotationModule.active && RandomRotationModule.rotateOnSpawn) RandomRotationModule.Rotate();
    }

    private void FixedUpdate()
    {
        if (ConstantRotateModule.active) ConstantRotateModule.Rotate();
    }
}

public static class RotateExtension
{

    [Serializable]
    public struct ConstantRotate
    {
        [SerializeField] public bool active;
        [ShowIf("active")] [SerializeField] Vector3 rotation;
        [ShowIf("active")][SerializeField] Transform transform;

        public void Rotate()
        {
            if (transform == null) this.Error("No transform set");
            transform.Rotate(rotation);
        }
    }

    [Serializable]
    public struct RandomRotation
    {
        [SerializeField] public bool active;
        [ShowIf("active")] public bool x, y, z;
        [ShowIf("active")] public bool affectAllChildren;
        [ShowIf("active")][SerializeField] Transform transform;
        [ShowIf("active")][ShowIf("x")] public Deviatable xRot;
        [ShowIf("active")][ShowIf("y")] public Deviatable yRot;
        [ShowIf("active")][ShowIf("z")] public Deviatable zRot;
        

        public bool rotateOnSpawn;

        public void Rotate()
        {
            if (affectAllChildren) { RotateChildren(); return; }
            Vector3 r = transform.eulerAngles;

            if (x) r.x = xRot.value;
            if (y) r.y = yRot.value;
            if (z) r.z = zRot.value;

            transform.rotation = Quaternion.Euler(r);
        }

        void RotateChildren()
        {
            foreach(Transform child in transform)
            {
                Vector3 r = child.eulerAngles;

                if (x) r.x = xRot.value;
                if (y) r.y = yRot.value;
                if (z) r.z = zRot.value;

                child.rotation = Quaternion.Euler(r);
            }

        }

    }

}
