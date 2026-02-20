using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static PilotConfig;

public class FaceDirectionModule<TFacadeType> : InputHeldModuleFacade<LookDir, TFacadeType>, UPDATE
    where TFacadeType : class, IFacade
{
        
    public FaceDirectionModule(PersistentAction<LookDir, bool> action, TFacadeType facade) : base(action, facade, false) { }
        
    [ShowInInspector] LookDir dir;
        
    protected override void OnSet(LookDir args) => dir = args;

    protected override void Execute(float dt)
    {
        if (dir == LookDir.Right) facade.context.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        if (dir == LookDir.Left) facade.Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        facade.Blackboard.facingDir = dir;
    }

    public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
}