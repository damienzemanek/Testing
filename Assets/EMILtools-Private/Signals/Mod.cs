using System;
using System.Collections.Generic;
using EMILtools.Signals;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Signals.StatTags;

namespace EMILtools.Signals
{
    
    public interface ModWrapper { }
    
    [Serializable]
    public struct Mod<T, TMod, TTag>
        where T : struct
        where TMod : struct, IStatModStrategy<T>
        where TTag : struct, IStatTag
    {
        [ShowInInspector] public readonly string tagName => typeof(TTag).Name;
        [ShowInInspector] public readonly string TModName => mod.GetType().Name;
        
        public readonly TMod mod;
        [ShowInInspector] public readonly List<StatModDecorator<T, TMod, TTag>> decorators;
        
        public Mod(TMod mod)
        {
            this.mod = mod;
            decorators = new List<StatModDecorator<T, TMod, TTag>>();
        }
        
        public static implicit operator TMod(Mod<T, TMod, TTag> mod) => mod.mod;
    }
    
    
}

