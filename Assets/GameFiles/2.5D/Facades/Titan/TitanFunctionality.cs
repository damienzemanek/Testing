using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using UnityEngine;
using static TwoD_InputAuthority;

public class TitanFunctionality : Functionalities<TwoD_TitanController>
{
    protected override void AddModulesHere()
    {
        // Layer 1 -> Direct Input
        AddModule(new DismountModule(facade.Input.HoldInteract, facade));
        
        // Layer 2 -> Actions
        AddModule(new CameraSystemModule(facade.Actions.Mount, facade));
        
        // Unbound        
        AddModule(new MountModule(facade));
        
        Debug.Log("Titan Functionality Modules Added");
    }


    public class DismountModule : InputPressedModuleFacade<TwoD_TitanController>
    {
        public DismountModule(PersistentAction action, TwoD_TitanController facade) : base(action, facade) { }

        protected override void OnPress()
        {
            Debug.Log("Titan Dismount Pressed");
            facade.StartCoroutine(DismountSequence());
        }

        IEnumerator DismountSequence()
        {
            Debug.Log("DISMOUNTING");
            
           // facade.Blackboard.anims.animator.Play(facade.Blackboard.anims.dismountAnim);
            
            yield return new WaitForSeconds(facade.Config.mount.duration);
            
            facade.Blackboard.hasMounted = false;
            facade.Blackboard.myPilot.gameObject.SetActive(true);
            facade.Blackboard.myPilot.context.RequestAuthority();
            
            Debug.Log("Titan Dismount Sequence Complete");
        }
    }

    public class CameraSystemModule : BasicFunctionalityModuleFacade<TwoD_TitanController, ActionGuarderMutable>, IAPI_CameraSystem
    {
        public CameraSystemModule(PersistentAction action, TwoD_TitanController facade) : base(action, facade, true) { }
        
        public override void Execute()
        {
            facade.Blackboard.camContext.CM.Target.TrackingTarget = facade.transform;
            facade.Blackboard.camContext.follow.FollowOffset = facade.Config.camSettings.followOffset;
            facade.Blackboard.camContext.rotComposer.TargetOffset = facade.Config.camSettings.targetOffset;
        }

        void IAPI_Dependant<CameraContext>.GrabDependancies(CameraContext context)
        {
            CameraContext myContext = facade.Blackboard.camContext;
            myContext.CM = context.CM;
            myContext.follow = context.follow;
            myContext.rotComposer = context.rotComposer;
            myContext.camera = context.camera;
        }

    }

    public class MountModule : UnboundFunctionalityModuleFacade<TwoD_TitanController, ActionGuarderMutable>, IAPI_Mount
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float duration { get; private set; }   
        }
        
        public MountModule(TwoD_TitanController facade) : base(facade, true) { }

        // Looks like alot of indirection, but Functionality Modules run through the Guards when they Execute;
        // Combined with the IAPI + IEnumerator it's just adding an extra layer of abstraction, +  the IEnumerator
        public void Mount() => ExecuteTemplateCall();
        public override void Execute() => facade.StartCoroutine(MountSequence());
        IEnumerator MountSequence()
        {
            Debug.Log("MOUNTING");
            
            Transform playerTransform = facade.Blackboard.myMountZone.playerTransform;
            Transform mountLoc = facade.Blackboard.mountLocation;
            
            playerTransform.position = mountLoc.position; 
            playerTransform.parent = mountLoc;
            playerTransform.Get<Rigidbody>().isKinematic = true;
            playerTransform.Get<Collider>().enabled = false;
            facade.Get<AugmentPhysEX>().fallFaster = false;
            facade.Blackboard.myPilot = playerTransform.Get<TwoD_PilotController>();
            facade.GetFunctionality<IAPI_CameraSystem>().SendDependencies(facade.Blackboard.myPilot.Blackboard.camContext);
            // Later: Remake MouseModule for this stuff
            //facade.Blackboard.mouseZoneGuarder = new SimpleGuarderMutable(("Not Looking", () => !isLooking));
            // input._lookGuarder = new SimpleGuarderMutable();
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.Blackboard.anims.animator.Play(facade.Blackboard.anims.mountFrontAnim);


            
            yield return new WaitForSeconds(facade.Config.mount.duration);
            
            facade.Blackboard.moveDecay.Start();
            facade.Blackboard.hasMounted = true;
            facade.Blackboard.myPilot.gameObject.SetActive(false);
            facade.Actions.Mount.Invoke();
        }





    }
}
