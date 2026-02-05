using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Data", menuName = "ScriptableObjects/Projectile Data")]
public class ProjectileData : EntityData
{
    public float forceScalar;
    public ForceMode forceMode;
    public string tag;
    public GameObject hitEffectPrefab;
}
