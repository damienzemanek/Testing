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


    public class ShipInputMap : InputMap, IInputMouseLook
    {
        [NonSerialized] public PersistentAction<bool> Thrust = new();
        [NonSerialized] public PersistentAction<Vector3, bool> Rotate = new();
        [NonSerialized] public PersistentAction<bool> Fire = new();
        [NonSerialized] public PersistentAction<bool> MouseLook = new();
        [NonSerialized] public PersistentAction SwitchCam = new();
        [NonSerialized] public PersistentAction Move = new();
        [NonSerialized] public Vector3 rotation;
        [field: NonSerialized] public Vector2 mouse { get; set; }
        
        public ShipInputMap() { }
        public ShipInputMap(string ownerName) : base(ownerName) { }
    }

    public class ShipActionMap : IActionMap
    {


    }
}
