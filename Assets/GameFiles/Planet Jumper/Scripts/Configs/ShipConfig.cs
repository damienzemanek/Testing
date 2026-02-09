using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipConfig", menuName = "ScriptableObjects/Configs/ShipConfig", order = 1)]
[Serializable]
public class ShipConfig : Config, IInterior<ShipController>
{
    [field:SerializeField] public ShipController facade { get; set; }
    public ShipFunctionality.RotateModule.Config rotate;
}
