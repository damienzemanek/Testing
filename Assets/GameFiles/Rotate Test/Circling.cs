using System;
using UnityEngine;

public class Circling : MonoBehaviour
{
    public Vector3 vector;
    private void Update()
    {
        transform.Rotate(vector);
    }
}
