using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

[Serializable]
public class EnemyOneBlackboard : Blackboard, ITwoD_Blackboard
{
    public enum AnimState { NONE_ASSIGN_STATE, Idle, Aim, Shoot }
    public Animator animator;
    public Transform weaponParent;
    [ReadOnly] public AnimState animState;
    public AnimHandle<AnimState, NoBlends> anims = new();
    public VolleyProjectileSpawner volleySpawner;
    [ReadOnly] public bool canSeeTarget = false;
    public Vector3 aimOffset;
    [ReadOnly] public Transform trackingTarget;
    public Transform aimPivot;
    public Vector3 lockedAimEueler;
    [field:SerializeField] public Transform facing { get; set; }
    [field: ReadOnly] [field:SerializeField] public LookDir facingDir { get; set; }
}