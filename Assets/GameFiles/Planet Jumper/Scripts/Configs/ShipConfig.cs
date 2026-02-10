using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipConfig", menuName = "ScriptableObjects/Configs/ShipConfig", order = 1)]
[Serializable]
public class ShipConfig : Config
{
    public ShipFunctionality.RotateModuleSub.Config rotate;
    public ShipFunctionality.ThrustModuleSub.Config thrust;
}
