using System;
using System.Collections;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static IInputSubordinate<TwoD_InputAuthority.TwoD_InputMap,TwoD_InputAuthority.Subordinates>;
using static TwoD_InputAuthority;



public class TwoD_TitanController : MonoFacade<
        TwoD_TitanController,
        TitanFunctionality, 
        TitanConfig,
        TitanBlackboard,
        TitanContext,
        TitanActionMap>,
    TimerUtility.ITimerUser,
    IInputSubordinate<TwoD_InputMap, Subordinates>
{
    
    
    [field: ShowInInspector] [field: NonSerialized] [field: ReadOnly]  public TwoD_InputMap Input { get; set; }
    [field: PropertyOrder(-1)] [field: ShowInInspector] [field: SerializeField] public SubordinateContext inputSubordinateContext { get; set; }
    public TwoD_InputMap InjectInputMap() => new("Titan");
    public void InitSubordinate() => InitializeFacade();

    private void Start() => OnSpawn();

    public void OnAuthorityReceived()
    {
        GetFunctionality<IAPI_Mount>().Mount();
        Functionality.Bind();
        if(!Blackboard.moveDecay.isRunning) Blackboard.moveDecay.Start();
    }

    public void OnAuthorityLost()
    {
        Functionality.Unbind();
        Input.Move.Invoke(false, Vector2.zero);
        Blackboard.rb.linearVelocity = Vector3.zero;
        Blackboard.myMountZone.mounted = Blackboard.hasMounted = Blackboard.isMountingOrDismounting = false;
    }

    protected override void Update()
    {
        if (!Blackboard.hasMounted || Blackboard.isMountingOrDismounting) return;
        base.Update();
        Config.animHandle.UpdateAnimBlendFloat(Blackboard.animator, TitanConfig.TitanAnimBlends.Speed, Blackboard.speedAlpha);
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
            Config.animHandle.Play(Blackboard.animator, TitanConfig.TitanAnims.Land);
            Blackboard.canMount = true;
        }
    }
    
}
