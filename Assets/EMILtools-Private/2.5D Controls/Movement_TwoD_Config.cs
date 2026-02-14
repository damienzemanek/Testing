using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using EMILtools.Signals;
using static EMILtools.Signals.ModiferRouting;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Signals.StatTags;

[Serializable]
[CreateAssetMenu(fileName = "Movement 2D Config", menuName = "ScriptableObjects/Movement/2D Config", order = 0)]
public class Movement_TwoD_Config : ScriptableObject, IStatHolder
{
    [SerializeField] public float moveForce = 90;
    [SerializeField] public ForceMode forceMode = ForceMode.Force;
    [SerializeField] public Ref<float> decayScalar = 2.5f;
    [SerializeField] public float mantleXOffset = 1f;
    [SerializeField] public float mantleDelay = 1f;
    [SerializeField] public float maxVelMagnitude = 100f;
    [SerializeField] public float maxSpeed = 390; // run speed
    
    

    //runforce was 390
}

