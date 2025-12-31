using System;
using System.Collections.Generic;
using EMILtools.Signals;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;

public static class ModifierExtensions
{
    public static T ApplyAll<T, TMod>(this List<TMod> modifiers, T val)
        where T : struct
        where TMod : struct, IStatModStrategy<T>
    {
        foreach (var mod in modifiers)
            val = mod.Apply(val);

        return val;
    }
    
    
    // Avoids long generic strings for StatModTimed to help with type inference through ModifierRouting.ModifyStatUser
    public static void ModifyStatUser<T, TMod>(this IStatUser recipient, ref StatModTimed<T, TMod> timedStrat)
        where T : struct, IEquatable<T>
        where TMod : struct, IStatModStrategy<T>
    {
        recipient.ModifyStatUser<T, StatModTimed<T, TMod>>(ref timedStrat);
    }

    // Float extension for timed
    public static StatModTimed<float, TMod> WithTimed<TMod>(this TMod modifier, float duration)
        where TMod : struct, IStatModStrategy<float>
    {
        return new StatModTimed<float, TMod>(in modifier, duration);
    }
    
    // Generic for timed
    public static StatModTimed<T, TMod> WithTimed<T, TMod>(this TMod modifier, float duration)
        where T : struct, IEquatable<T>
        where TMod : struct, IStatModStrategy<T>
    {
        return new StatModTimed<T, TMod>(in modifier, duration);
    }
    
    
    // Float ext for regular
    public static void ModifyStatUser<TMod>(this IStatUser recipient, ref TMod strat)
        where TMod : struct, IStatModStrategy<float>
    {
        // Redirects to the main method with float explicitly set
        recipient.ModifyStatUser<float, TMod>(ref strat);
    }
}
