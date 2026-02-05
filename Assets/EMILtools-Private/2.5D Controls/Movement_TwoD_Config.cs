using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Movement 2D Config", menuName = "ScriptableObjects/Movement/2D Config", order = 0)]
public class Movement_TwoD_Config : ScriptableObject
{
    [SerializeField] public float walkForce = 230f; // value based on mass 1
    [SerializeField] public Ref<float> runForce = 390f; // value based on mass 1
    [SerializeField] public ForceMode forceMode = ForceMode.Force;
    [SerializeField] public Ref<float> decayScalar = 2.5f;
    [SerializeField] public float mantleXOffset = 1f;
    [SerializeField] public float mantleDelay = 1f;
    [SerializeField] public float maxVelMagnitude = 100f;
}

