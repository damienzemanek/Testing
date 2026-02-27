using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyOneConfig", menuName = "Configs/EnemyOne")]
public class EnemyOneConfig : Config
{
    public enum EnemyOneAnims { Shoot, Aim, Idle }
    public enum EnemyOneAnimBlendTreeVariabebles { }
    [field: SerializeField] public AnimHandle<EnemyOneAnims, EnemyOneAnimBlendTreeVariabebles> animHandle { get; set; }
    [field: FormerlySerializedAs("<aimOffset>k__BackingField")] [field: SerializeField] public Vector3 pilotAimOffset { get; set; }
    [field: SerializeField] public Vector3 titanAimOffset { get; set; }

}