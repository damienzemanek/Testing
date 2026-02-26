using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

public static class TwoD_SharedModules
{
    public class FaceDirectionModule<TFacadeType, TContext> : 
        BoundHeldFunctionality<TFacadeType, TContext, FaceDirectionModule<TFacadeType, TContext>.Setter>, 
        UPDATE
    where TFacadeType : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext, ITwoD_Context
    {
        public class Setter : SettableTemplate<bool, LookDir> { [ShowInInspector] public LookDir dir => unnamedStoredValue2; }
        ITwoD_Blackboard Blackboard;

        protected override void Awake()
         => Blackboard = facade.API_Blackboard<ITwoD_Blackboard>() ?? throw new System.ArgumentNullException(nameof(facade), "Facade cannot be null");


        public FaceDirectionModule(PersistentAction<bool, LookDir> _action, TFacadeType facade) : base(_action, facade) { }
        public override int injectAmountOfAddedSteps { get; }

        public override PipelineBuilder<TContext> AddPipelineStepsHere(PipelineBuilder<TContext> builder)
            => builder;

        public override bool Execute(TContext ctx)
        {
            if (InputContext.dir == LookDir.Right) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            if (InputContext.dir == LookDir.Left) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            ctx.dir = InputContext.dir;
            return true;
        }

        public void UpdateTick() => ExecuteTemplateCall();
    }
}
