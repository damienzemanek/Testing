using System;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static IInputSubordinate<TwoD_InputAuthority.TwoD_InputMap,TwoD_InputAuthority.Subordinates>;
using static PilotConfig;
using static TwoD_InputAuthority;

public class TwoD_TitanController : MonoFacade<
    TwoD_TitanController,
    TitanFunctionality, 
    TitanConfig,
    TitanBlackboard,
    TitanActionMap>,
    TimerUtility.ITimerUser,
    IInputSubordinate<TwoD_InputMap, Subordinates>
{
    [field: ShowInInspector] [field: NonSerialized] [field: ReadOnly]  public TwoD_InputMap Input { get; set; }
    [field: PropertyOrder(-1)] [field: ShowInInspector] [field: SerializeField] public SubordinateContext context { get; set; }
    public TwoD_InputMap InitSubordinate()
    {
        if(Input == null) Input = new TwoD_InputMap("Titan");
        InitializeFacade();
        return Input;
    }

    public void OnAuthorityReceived()
    {
        GetFunctionality<IAPI_Mount>().Mount();
        Functionality.Bind();
    }

    public void OnAuthorityLost()
    {
        Functionality.Unbind();

    }

    protected override void Update()
    {
        if (!Blackboard.hasMounted) return;
        
        base.Update();
        // if(Blackboard.anims.state == AnimState.Locomotion)
        //     Blackboard.anims.UpdateLocomotion(Blackboard.facingDir, Blackboard.moveDir, Blackboard.speedAlpha);
    }
    

    private void OnDisable()
    {
        Functionality.Unbind();
    }

}
