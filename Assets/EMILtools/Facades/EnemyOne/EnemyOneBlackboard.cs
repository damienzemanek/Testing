using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

[Serializable]
public class EnemyOneBlackboard : Blackboard, ITwoD_Blackboard
{
    public VolleyProjectileSpawner volleySpawner;
    [ReadOnly] public bool canSeeTarget = false;
    public Vector3 aimOffset;
    [ReadOnly] public Transform trackingTarget;
    public Transform aimPivot;
    [field:SerializeField] public Transform facing { get; set; }
    [field: ReadOnly] [field:SerializeField] public LookDir facingDir { get; set; }
}