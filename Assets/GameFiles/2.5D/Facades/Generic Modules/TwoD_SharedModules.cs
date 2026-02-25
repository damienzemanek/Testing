using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;

public static class TwoD_SharedModules
{
    public class FaceDirectionModule<TFacadeType> : InputHeldModuleFacade<LookDir, TFacadeType>, UPDATE
        where TFacadeType : class, IFacade
    {
        ITwoD_Blackboard Blackboard;
    
        public FaceDirectionModule(PersistentAction<LookDir, bool> action, TFacadeType facade) : base(action, facade, false) { }
        
        [ShowInInspector] LookDir dir;

        protected override void Awake() => Blackboard = facade.Blackboard<ITwoD_Blackboard>();

        protected override void OnSet(LookDir args) => dir = args;

        protected override void Execute(float dt)
        {
            if(Blackboard == null) Awake();
            if (dir == LookDir.Right) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            if (dir == LookDir.Left) Blackboard.facing.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Blackboard.facingDir = dir;
            
        }

        public void UpdateTick(float dt) => ExecuteTemplateCall(dt);
    }
}
