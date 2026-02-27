using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static ITwoD_Blackboard;

[Serializable]
public class EnemyOneBlackboard : Blackboard, ITwoD_Blackboard
{

    public VolleyProjectileSpawner volleySpawner;
    // ----- ReadOnly ----- //
    [BoxGroup("ReadOnly")] [ReadOnly] public bool canSeeAndFire
    {
        get => volleySpawner.canFire;
        set => volleySpawner.canFire = value;
    }
    [field: BoxGroup("ReadOnly")][field: ReadOnly] [field:SerializeField] public LookDir facingDir { get; set; }
    [BoxGroup("ReadOnly")] [ReadOnly] public Transform trackingTarget;
    
    
    // ------ References ---- //
    [BoxGroup("References")] [Required] public Transform aimPivot;
    [BoxGroup("References")] [Required] public Animator animator;
    [BoxGroup("References")] [Required] public Transform weaponParent;
    [field: BoxGroup("References")][field:Required] [field: FormerlySerializedAs("<facing>k__BackingField")] [field:SerializeField] public Transform facingTransformObject { get; set; }
}