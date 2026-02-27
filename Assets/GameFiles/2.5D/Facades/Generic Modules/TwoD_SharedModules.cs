using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

public static class TwoD_SharedModules
{
    public class FaceDirectionModule<TFacadeType, TContext> : 
        BoundSetFunctionality<TFacadeType, TContext, FaceDirectionModule<TFacadeType, TContext>.Setter>, 
        UPDATE
    where TFacadeType : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext, ITwoD_Context
    {
        public class Setter : SettableTemplate<bool, LookDir> 
            { [ShowInInspector] public LookDir newFacingDirection => unnamedStoredValue2; }
        
        ITwoD_Blackboard Blackboard;

        protected override void Awake()
         => Blackboard = facade.API_Blackboard<ITwoD_Blackboard>() ?? throw new System.ArgumentNullException(nameof(facade), "Facade cannot be null");
        
        public FaceDirectionModule(PersistentAction<bool, LookDir> _action, TFacadeType facade) : base(_action, facade) { }
        public override PipelineBuilder<TContext> InjectSteps(PipelineBuilder<TContext> builder)
            => builder;

        public override bool ExecutionImplementation(TContext ctx)
        {
            if (SetContext.newFacingDirection == LookDir.Right) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            if (SetContext.newFacingDirection == LookDir.Left) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Blackboard.facingDir = SetContext.newFacingDirection;
            return true;
        }

        public void UpdateTick() => Execute();
    }
}
