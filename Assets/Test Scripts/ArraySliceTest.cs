using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ArraySliceTest : MonoBehaviour
{
    public int[] list;

    private void OnValidate()
    {
        list = new int[] { 1, 2, 3, 4, 5, 6 };

    }

    [Button, ExecuteAlways]
    void SliceDebug()
    {
        print(list[^1]); //Last index
        print(list[^3..]);
    }
}
