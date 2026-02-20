using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

[Serializable]
public class EnemyOneBlackboard : Blackboard, ITwoD_Blackboard
{
    public VolleyProjectileSpawner volleySpawner;
    public bool canSeeTarget = false;
    [field:SerializeField] public Transform facing { get; set; }
    [field:SerializeField] public LookDir facingDir { get; set; }
}