using System;
using EMILtools.Core;
using EMILtools.Extensions;
using Unity.Cinemachine;
using UnityEngine;
using static ITwoD_Blackboard;
using static TwoD_InputAuthority;

[Serializable]
public class TwoD_InputAuthority : InputAuthority< TwoD_InputReader, TwoD_InputMap, Subordinates>
{
    public enum Subordinates { Pilot = 0, Titan = 1 }

    /// <summary>
    /// Input Mappings
    /// - Serializable only because of presetting MouseInputZones (for First Delegation)
    /// </summary>
    [Serializable]
    public class TwoD_InputMap : InputMap
    {
        [NonSerialized] public PersistentAction<bool, Vector2> Move = new();
        [NonSerialized] public PersistentAction<bool> Run = new();
        [NonSerialized] public PersistentAction<bool> Look = new();
        [NonSerialized] public PersistentAction<bool> Shoot = new();
        [NonSerialized] public PersistentAction<bool, LookDir> FaceDirection = new();
        [NonSerialized] public PersistentAction Jump = new();
        [NonSerialized] public PersistentAction Interact = new();
        [NonSerialized] public PersistentAction HoldInteract = new();
        [NonSerialized] public PersistentAction CallInTitan = new();
        public MouseCallbackZones MouseInputZones;
        [NonSerialized] public Vector2 mouse;

        public TwoD_InputMap() : base("TwoD Input Map"){ }
        public TwoD_InputMap(string ownerName) : base(ownerName) { }
    }

    public class PilotActionMap : IActionMap
    {
        public PersistentAction UnMantleLedge = new();
        public PersistentAction MantleLedge = new();
        public PersistentAction DoubleJump = new();
        public PersistentAction ClimbLedge = new();
        public PersistentAction Land = new();
        public PersistentAction Dismount = new();

    }
    
    public class TitanActionMap : IActionMap
    {
        public PersistentAction Mount = new();

    }

    [Serializable]
    public struct CameraSettings
    {
        public Vector3 followOffset;
        public Vector3 targetOffset;
    }

    [Serializable]
    public class CameraContext : ICamContext
    {
        public Camera camera;
        public CinemachineCamera CM;
        public CinemachineFollow follow;
        public CinemachineRotationComposer rotComposer;
        public Transform mouseCenter;
    }
}
