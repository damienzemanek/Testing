using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon")]
public class WeaponManager : ScriptableObject
{
    public Ref<float> fireRate;
}
