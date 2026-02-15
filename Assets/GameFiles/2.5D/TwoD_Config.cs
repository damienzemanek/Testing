
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TwoD_Config", menuName = "ScriptableObjects/TwoD_Config", order = 2)]
public class TwoD_Config : Config, IFacadeCompositionElement<TwoD_Controller>
{
    public TwoD_Controller facade { get; set; }
    
    public enum LookDir { None, Left, Right }
    public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb }
    
    [field: SerializeField] [field: ShowInInspector] public TwoD_Functionality.MoveModule.Config move { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TwoD_Functionality.TitanCallInModule.Config titan { get; private set; }
    [field: SerializeField] [field: ShowInInspector] public TwoD_Functionality.JumpModule.Config jump { get; private set; }

}