using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TitanConfig", menuName = "ScriptableObjects/Configs/Titan Config")]
public class TitanConfig : Config
{
    [field: SerializeField] [field: ShowInInspector] public TwoD_InputAuthority.CameraSettings camSettings { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TitanFunctionality.MountModule.Config mount { get; private set; }
}
