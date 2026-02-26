using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TitanConfig", menuName = "ScriptableObjects/Configs/Titan Config")]
public class TitanConfig : Config
{
    public enum TitanAnims { Falling, Land, Shoot, UpperBodyIdle, UpperBodyLanding, LocomotionFwd, LocomotionBackwd, MountFront, Dismount }
    [field: SerializeField] [field: ShowInInspector] public AnimHandle<TitanAnims> animHandle { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TwoD_InputAuthority.CameraSettings camSettings { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TitanFunctionality.MountModule.Config mount { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TitanFunctionality.LocomotionModule.Config move { get; private set; }

    [Button]
    public void Reinit()
    {
        animHandle = new AnimHandle<TitanAnims>();
    }

}
