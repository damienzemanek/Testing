using System;
using System.Collections;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static IInputSubordinate<TwoD_InputAuthority.TwoD_InputMap,TwoD_InputAuthority.Subordinates>;
using static ITwoD_Blackboard;
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
    public TwoD_InputMap InjectInputMap() => new("Titan");
    public void InitSubordinate() => InitializeFacade();

    private void Start()
    {
        Blackboard.moveDecay.Start();
        OnSpawn();
    }

    public void OnAuthorityReceived()
    {
        GetFunctionality<IAPI_Mount>().Mount();
        Functionality.Bind();
    }

    public void OnAuthorityLost()
    {
        Functionality.Unbind();
        Blackboard.myMountZone.mounted = false;
    }

    protected override void Update()
    {
        if (!Blackboard.hasMounted) return;
        
        base.Update();
        if(Blackboard.anims.state == AnimState.Locomotion)
            Blackboard.anims.UpdateLocomotion(Blackboard.facingDir, Blackboard.moveDir, Blackboard.speedAlpha);
    }
    

    private void OnDisable()
    {
        Functionality.Unbind();
    }

    void OnSpawn()
    {
        Blackboard.canMount = false;
        StartCoroutine(WaitUntilLanded());
        
        IEnumerator WaitUntilLanded()
        {
            while(!Blackboard.phys.isGrounded) yield return null;
            Blackboard.anims.Play(Blackboard.anims.land);
            Blackboard.canMount = true;
        }
    }
    
}
