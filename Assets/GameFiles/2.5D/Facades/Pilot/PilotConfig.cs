
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Pilot Config", menuName = "ScriptableObjects/Configs/Pilot", order = 2)]
public class PilotConfig : Config
{
    public enum LookDir { None, Left, Right }
    public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb, MountFront, Dismount }
    
    [field: SerializeField] [field: ShowInInspector] public PilotFunctionality.MoveModule.Config move { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public PilotFunctionality.TitanCallInModule.Config titan { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public PilotFunctionality.JumpModule.Config jump { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TwoD_InputAuthority.CameraSettings camSettings { get; private set; }

}