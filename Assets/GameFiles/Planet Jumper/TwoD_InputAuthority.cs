using System;
using EMILtools.Core;
using EMILtools.Extensions;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;
using static TwoD_Config;
using static TwoD_InputAuthority;

[Serializable]
public class TwoD_InputAuthority : InputAuthority<TwoD_InputReader, TwoD_InputMap>, IInputAuthority
{
    [Serializable]
    public struct Config
    {
        [field:SerializeField] public MouseCallbackZones MouseInputZones { get; private set; }
    }
    
    public struct Mapping
    {
        public TwoD_InputMap map;
        public InterfaceRef<IControllable<TwoD_InputMap>> controlled;
        
        public IInitializable Initializable => controlled.Value as IInitializable;
    }

    public int currentMapping = 0;
    [field: SerializeField] public Config cfg { get; set; }
    [NonSerialized, ShowInInspector, ReadOnly] public Mapping[] InputMappings = new Mapping[2];

    void Awake()
    {
        for (int i = 0; i < InputMappings.Length; i++) 
            InputMappings[i] = new Mapping();

        // 0 Index controlled (first one) is the default
        currentMapping = 0;
        var input = InputMappings[currentMapping];
        input.map = new TwoD_InputMap(cfg.MouseInputZones);
        input.controlled = Controlled;
        input.controlled.Value.Input = input.map;
        input.Initializable.Init();
        Reader.InputMap = input.map;
        Reader.Init();
    }

    [Serializable]
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
        public MouseCallbackZones MouseInputZones = new();
        
        public Vector2 movement;
        public Vector2 mouse;

        public TwoD_InputMap(MouseCallbackZones mouseInputZones) => this.MouseInputZones = mouseInputZones;
    }

    [Serializable]
    public class PilotActionMap : IActionMap
    {
        public PersistentAction UnMantleLedge = new();
        public PersistentAction MantleLedge = new();
        public PersistentAction DoubleJump = new();
        public PersistentAction ClimbLedge = new();
        public PersistentAction<bool> Land = new();
    }
}
