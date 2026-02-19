using System;
using UnityEngine;

public class RotateTest : MonoBehaviour
{
    public Transform lookAt;

    
    
    private void LateUpdate()
    {
        Vector3 offset = new Vector3(0, 0, 0);
        transform.eulerAngles = Quaternion.LookRotation(lookAt.position - transform.position).eulerAngles;
    }
}
