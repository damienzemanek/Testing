using System;
using System.Collections.Generic;
using UnityEngine;


public interface IPositionModule
{
    public void Execute();
}
public class PositionModule : MonoBehaviour
{
    
    [Serializable]
    public struct Constrain : IPositionModule
    {
        public Transform transform;
        public bool local;
        public bool x, y, z;
        
        public float constraintX;
        public float constraintY;
        public float constraintZ;

        public void Execute()
        {
            Vector3 position = transform.position;
            if (x) position.x = constraintX;
            if (y) position.y = constraintY;
            if (z) position.z = constraintZ;
            if(!local) transform.position = position;
            else transform.localPosition = position;
        }
    }

    public Constrain constrain;

    public void Update()
    {
        constrain.Execute();
    }
}
