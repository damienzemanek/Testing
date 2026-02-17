using System;
using EMILtools.Core;
using EMILtools.Extensions;
using KBCore.Refs;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using static PilotConfig;
using static ShipInputAuthority;

[Serializable]
public class ShipInputAuthority : InputAuthority<ShipInputReader, ShipInputMap, Subordinates>
{
    public enum Subordinates { Ship = 0 }


    public class ShipInputMap : InputMap
    {
        [NonSerialized] public PersistentAction<bool> Thrust = new();
        [NonSerialized] public PersistentAction<Vector3, bool> Rotate = new();
        [NonSerialized] public PersistentAction<bool> Fire = new();
        [NonSerialized] public PersistentAction Move = new();
        [NonSerialized] public PersistentAction SwitchCam = new();
        [NonSerialized] public Vector3 rotation;
        [NonSerialized] public Vector2 mouse;
        
        public ShipInputMap() { }
        public ShipInputMap(string ownerName) : base(ownerName) { }
    }

    public class ShipActionMap : IActionMap
    {


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
    }
}
