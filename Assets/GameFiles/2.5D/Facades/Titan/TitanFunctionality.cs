using System;
using System.Collections;
using EMILtools_Private.Testing;
using EMILtools.Core;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static ITwoD_Blackboard;
using static TitanFunctionality.MouseInputZonesModule;
using static TwoD_InputAuthority;
using static TwoD_SharedModules;

public class TitanFunctionality : Functionalities<TwoD_TitanController>
{
    protected override void AddModulesHere()
    {
        // Layer 1 -> Direct Input
        AddModule(new DismountModule(facade.Input.HoldInteract, facade));
        AddModule(new LocomotionModule(facade.Input.Move, facade));
        AddModule(new FaceDirectionModule<TwoD_TitanController>(facade.Input.FaceDirection, facade));
        AddModule(new ShootModule(facade.Input.Shoot, facade));
        
        // Layer 2 -> Actions
        AddModule(new CameraSystemModule(facade.Actions.Mount, facade));
        
        // Unbound        
        AddModule(new MountModule(facade));
        AddModule(new MouseInputZonesModule(facade));
        AddModule(new MouseLookModule(facade));
    }

    
    

    public class ShootModule : InputHeldModuleFacade<TwoD_TitanController>, UPDATE
    {
        public ShootModule(PersistentAction<bool> action, TwoD_TitanController facade) : base(action, facade, false) { }

        protected override void Awake()
            => executeGuarder = new ActionGuarderMutable(
                new ActionGuard(() => !isActive, AnimateBackToIdle, "Not Active", "Back To Idle"),
                           new ActionGuard(() => facade.Blackboard.bulletSpawner.fireTimer.isRunning, "Fire Rate On Cooldown"));
        
        protected override void Execute(float dt)
        {
            if (facade.Blackboard.bulletSpawner.fireTimer.isRunning) return;
            facade.Blackboard.bulletSpawner.targetPosition = facade.Blackboard.mouseLook.core.contactPoint;
            facade.Blackboard.bulletSpawner.Spawn();
            facade.Blackboard.anims.animator.Play(facade.Blackboard.anims.shoot, layer: 1, normalizedTime: 0f);
        }

        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
        
        void AnimateBackToIdle() 
            => facade.Blackboard.anims.animator.CrossFade(facade.Blackboard.anims.move, 0.1f, 0);
    }
    

    public class MouseLookModule : UnboundFunctionalityModuleFacade<TwoD_TitanController>, LATEUPDATE
    {
        public MouseLookModule(TwoD_TitanController facade) : base(facade, true) { }
    
        public override void Execute() => facade.Blackboard.mouseLook.Execute();
        public void LateTick(float dt) => ExecuteTemplateCall(dt);
    }
    
    public class MouseInputZonesModule : UnboundFunctionalityModuleFacade<TwoD_TitanController>, UPDATE, IAPI_Dependant<MouseInputZonesModule.MouseModuleContext>
    {
        public struct MouseModuleContext { public Camera cam; public MouseModuleContext(Camera cam) => this.cam = cam; }
        public MouseInputZonesModule(TwoD_TitanController facade) : base(facade, true) { }
        public override void Execute() => facade.Input.MouseInputZones.CheckAllZones(facade.Input.mouse);
        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
        void IAPI_Dependant<MouseModuleContext>.GrabDependancies(MouseModuleContext context) => facade.Blackboard.mouseLook.cam = context.cam;
    }
    

    
    public class LocomotionModule : InputHeldModuleFacade<Vector2, TwoD_TitanController>, UPDATE
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float speed { get; private set; }
            [field: SerializeField] public ForceMode forceMode { get; private set; }
            [field: SerializeField] public float runAlphaMax { get; private set; }
            [field: SerializeField] public Ref<float> decayScalar { get; private set; }
            [field: SerializeField] public float slowdownTime { get; private set; }
            [field: SerializeField] public float maxVelMagnitude { get; private set; }

        }
        
        public LocomotionModule(PersistentAction<Vector2, bool> action, TwoD_TitanController facade) : base(action, facade, true) { }

        
        [ShowInInspector] Vector2 moveVector;

        protected override void Awake()
        {
            facade.Blackboard.moveDecay = new DecayTimer(facade.Config.move.runAlphaMax, facade.Config.move.decayScalar);
            facade.Blackboard.turnSlowdown = new CountdownTimer(facade.Config.move.slowdownTime);

            facade.Blackboard.rb.maxLinearVelocity = facade.Config.move.maxVelMagnitude;
            facade.Blackboard.rb.maxAngularVelocity = facade.Config.move.maxVelMagnitude;
            
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.InitTimer(facade.Blackboard.turnSlowdown, true);
        }
        
        protected override void OnSet(Vector2 args) => moveVector = args;

        protected override void Execute(float dt)
        {
            Walk();
            Move(moveVector);
            
            
            void Walk()
            {
                if(facade.Blackboard.speedAlpha < 1f) facade.Blackboard.speedAlpha += 0.1f;
                facade.Blackboard.speedAlpha = NumEX.ToleranceSet(facade.Blackboard.speedAlpha, 1, 0.2f);
            }

            void Move(Vector2 move)
            {
                if (move.x == 0) return;
                LookDir prevMoveDir = facade.Blackboard.moveDir;

                Vector3 dir = move.x < 0 ? Vector3.left : Vector3.right;
                facade.Blackboard.moveDir = move.x < 0 ? LookDir.Right : LookDir.Left;
                ApplyMoveForce(dir);
            
            }
            
            void ApplyMoveForce(Vector3 dir)
            {
            
                // float runSpeedIncludingDecay = (speedAlpha > WALK_ALPHA_MAX ? movement.maxSpeed : movement.moveForce);
                // float actualSpeed = isRunning ? runSpeedIncludingDecay : movement.moveForce;
                // if (turnSlowdown.isRunning) actualSpeed *= turnSlowDown.Eval(phys.isGrounded, turnSlowdown.Progress);
                // if (!phys.isGrounded) actualSpeed *= phys.fallSettings.inAirMoveScalar;
                facade.Blackboard.rb.AddForce(dir * facade.Config.move.speed, facade.Config.move.forceMode);
            }
        }
        public void OnUpdateTick(float dt) => ExecuteTemplateCall(dt);
    }
    

    public class DismountModule : InputPressedModuleFacade<TwoD_TitanController>
    {
        public DismountModule(PersistentAction action, TwoD_TitanController facade) : base(action, facade) { }
        
        IInputSubordinate<TwoD_InputMap, Subordinates> thisTitan;
        IInputSubordinate<TwoD_InputMap, Subordinates> pilot;

        protected override void OnPress() => facade.StartCoroutine(DismountSequence());

        IEnumerator DismountSequence()
        {
            facade.Blackboard.anims.animator.Play(facade.Blackboard.anims.dismountAnim);
            
            yield return new WaitForSeconds(facade.Config.mount.duration);

            thisTitan = facade;
            pilot = facade.Blackboard.myPilot;
            
            bool successful = pilot.RequestAuthorityFrom(thisTitan);
            if(!successful) yield break;
            
            facade.Blackboard.hasMounted = false;
            facade.Blackboard.myPilot.gameObject.SetActive(true);
            facade.Blackboard.myPilot = null;
            Debug.Log("Titan Dismount Sequence Complete");
        }
    }

    public class CameraSystemModule : BasicFunctionalityModuleFacade<TwoD_TitanController>, IAPI_CameraSystem
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

    public class MountModule : UnboundFunctionalityModuleFacade<TwoD_TitanController>, IAPI_Mount
    {
        [Serializable]
        public struct Config
        {
            [field: SerializeField] public float duration { get; private set; }   
        }
        
        public MountModule(TwoD_TitanController facade) : base(facade, true) { }

        protected override void Awake()
            => executeGuarder.Add(new ActionGuard(() => !facade.Blackboard.canMount, "Cant Mount"));

        // Looks like alot of indirection, but Functionality Modules run through the Guards when they Execute;
        // Combined with the IAPI + IEnumerator it's just adding an extra layer of abstraction, +  the IEnumerator
        public void Mount() => ExecuteTemplateCall();
        public override void Execute() => facade.StartCoroutine(MountSequence());
        IEnumerator MountSequence()
        {
            Transform playerTransform = facade.Blackboard.myMountZone.playerTransform;
            Transform mountLoc = facade.Blackboard.mountLocation;
            var camContext = playerTransform.Get<TwoD_PilotController>().Blackboard.camContext;
            
            playerTransform.position = mountLoc.position; 
            playerTransform.parent = mountLoc;
            playerTransform.Get<Rigidbody>().isKinematic = true;
            playerTransform.Get<Collider>().enabled = false;
            facade.Get<AugmentPhysEX>().fallFaster = false;
            facade.Blackboard.myPilot = playerTransform.Get<TwoD_PilotController>();
            facade.GetFunctionality<IAPI_CameraSystem>().SendDependencies(camContext);
            facade.GetFunctionality<IAPI_Dependant<MouseModuleContext>>().SendDependencies(new MouseModuleContext(camContext.camera));
            // Later: Remake MouseModule for this stuff
            //facade.Blackboard.mouseZoneGuarder = new SimpleGuarderMutable(("Not Looking", () => !isLooking));
            // input._lookGuarder = new SimpleGuarderMutable();
            facade.InitTimer(facade.Blackboard.moveDecay, true);
            facade.Blackboard.anims.animator.Play(facade.Blackboard.anims.mountFrontAnim);


            
            yield return new WaitForSeconds(facade.Config.mount.duration);
            
            
            facade.Blackboard.moveDecay.Start();
            facade.Blackboard.hasMounted = true;
            if(facade.Blackboard.myPilot != null) facade.Blackboard.myPilot.gameObject.SetActive(false);
            facade.Actions.Mount.Invoke();
        }





    }
}
