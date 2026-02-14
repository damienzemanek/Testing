public class TwoD_Config : Config, IFacadeCompositionElement<TwoD_Controller>
{
    public TwoD_Controller facade { get; set; }
    
    public enum LookDir { None, Left, Right }
    public enum AnimState { Locomotion, Jump, InAir, Land, Mantle, Climb }
    
    public float walkAlphaMax = 1f;
    public float runAlphaMax = 2.2f; // Should be greater than the greatest blend tree value to avoid jitter
}