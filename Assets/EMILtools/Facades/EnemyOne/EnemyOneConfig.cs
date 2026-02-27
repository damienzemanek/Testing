using UnityEngine;

[CreateAssetMenu(fileName = "EnemyOneConfig", menuName = "Configs/EnemyOne")]
public class EnemyOneConfig : Config
{
    public enum EnemyOneAnims { Shoot, Aim, Idle }
    public enum EnemyOneAnimBlendTreeVariabebles { }
    [field: SerializeField] public AnimHandle<EnemyOneAnims, EnemyOneAnimBlendTreeVariabebles> animHandle { get; set; }
}