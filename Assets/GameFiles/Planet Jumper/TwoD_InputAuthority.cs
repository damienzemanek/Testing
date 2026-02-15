using System;
using EMILtools.Core;
using EMILtools.Extensions;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;
using static TwoD_Config;
using static TwoD_InputAuthority;

[Serializable]
public class TwoD_InputAuthority : InputAuthority< TwoD_InputReader, TwoD_InputMap, Subordinates>
{
    public static TwoD_InputAuthority Instance { get; private set; }
    
    public enum Subordinates
    {
        Pilot = 0,
        Titan = 1
    }
    
    [Serializable]
    public struct Config : IConfig
    {
        [field:SerializeField] public MouseCallbackZones MouseInputZones { get; private set; }
    }
    
    [field: SerializeField] public Config cfg { get; set; }
    public Subordinates currentSubordinate = 0;

    void Awake()
    {
        Instance = this;
        InitializeMappingsList(mappingCount);
    }

    public class TwoD_InputMap : IInputMap
    {
        public PersistentAction<bool> Move = new();
        public PersistentAction<bool> Run = new();
        public PersistentAction<bool> Look = new();
        public PersistentAction<bool> Shoot = new();
        public PersistentAction<LookDir, bool> FaceDirection = new();
        public PersistentAction Jump = new();
        public PersistentAction Interact = new();
        public PersistentAction CallInTitan = new();
        public MouseCallbackZones MouseInputZones;
        
        public Vector2 movement;
        public Vector2 mouse;

        public TwoD_InputMap() { }
        public TwoD_InputMap(MouseCallbackZones mouseInputZones) => this.MouseInputZones = mouseInputZones;
    }

    public class PilotActionMap : IActionMap
    {
        public PersistentAction UnMantleLedge = new();
        public PersistentAction MantleLedge = new();
        public PersistentAction DoubleJump = new();
        public PersistentAction ClimbLedge = new();
        public PersistentAction<bool> Land = new();
    }
    
    [Serializable]
    public class TitanActionMap : IActionMap
    {

    }
}
