using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

[Serializable]
public class EnemyOneBlackboard : Blackboard, ITwoD_Blackboard
{
    public Animator animator;
    public Transform weaponParent;
    public VolleyProjectileSpawner volleySpawner;
    [ReadOnly] public bool canSeeAndFire
    {
        get => volleySpawner.canFire;
        set => volleySpawner.canFire = value;
    }
    public Vector3 aimOffset;
    [ReadOnly] public Transform trackingTarget;
    public Transform aimPivot;
    public Vector3 lockedAimEueler;
    [field:SerializeField] public Transform facing { get; set; }
    [field: ReadOnly] [field:SerializeField] public LookDir facingDir { get; set; }
}