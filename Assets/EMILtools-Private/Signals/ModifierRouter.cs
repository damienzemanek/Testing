using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EMILtools.Core;
using static EMILtools.Core.ExpressionBinder;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;

namespace EMILtools.Signals
{
    [Serializable]
    public readonly struct ModifierRouter
    {
        // Reference casting from Action<TMod> to object (basically free)
        // Since Action is a refernce, this doesn't box
        public readonly Dictionary<Type, object> MethodCache_AddModifier;
    }
    
    // Generic meta-data pattern (Type-Safe Enum)
    public static class StatLinker<T, TMod>
        where T : struct, IEquatable<T>
        where TMod : struct, IStatModStrategy<T>
    {
        //Action is specific to this exact TMod type
        // no boxing occurs bc the type is baked into the class
        public static Action<TMod> AddAction;
    }


    public static class ModiferRouting
    {
        public static void ModifyStatUser<T, TMod>(this IStatUser recipient, ref TMod strat)
            where T : struct, IEquatable<T>
            where TMod : struct, IStatModStrategy<T>
        {
            Debug.Log($"Retrieved Recicpient : {recipient}");

            if (recipient.router.MethodCache_AddModifier.TryGetValue(typeof(TMod), out var actionObject))
            {
                if (actionObject is not Action<TMod> AddModifier) return;
                AddModifier(strat);
            }
            
            // Type tmodtype = strat.GetType(); // Default to non-custom strategies
            // if (strat.GetType().IsAssignableFrom(typeof(IStatModStrategyCustom)))
            //     tmodtype = strat.GetType().GetGenericArguments()[1];


        }

        /// <summary>
        /// Call in Awake to cache the add modifiers for the IStatUser
        /// </summary>
        /// <param name="statuser"></param>
        public static void CacheStatFields(this IStatUser istatuser)
        {
            if (istatuser == null) return;

            Debug.Log($"[CacheStatFields] Starting cache for IStatUser: {istatuser.GetType().Name}");

            var fields = istatuser.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(IStat).IsAssignableFrom(f.FieldType))
                .ToList();

            Debug.Log($"[CacheStatFields] Found {fields.Count} Stat fields: {string.Join(", ", fields.Select(f => f.Name))}");

            foreach (var field in fields)
            {
                var instance = field.GetValue(istatuser);
                var method = field.FieldType.GetMethod("AddModifier");

                if (instance == null || method == null)
                {
                    Debug.LogWarning($"[CacheStatFields] Skipping field {field.Name}: Instance or AddModifier method not found.");
                    continue;
                }

                // Extract generic arguments (e.g., <float, SpeedModifier>)
                var genericArgs = field.FieldType.GetGenericArguments();
                var valueType = genericArgs[0];
                var stratType = genericArgs[1];

                Debug.Log($"[CacheStatFields] Processing field '{field.Name}' -> ValueType: {valueType.Name}, StratType: {stratType.Name}");

                // 1. Update the Static StatLinker (Global type-safe access)
                try 
                {
                    // Get a refernece to the generic meta data pattern provider
                    Type specificLinker = typeof(StatLinker<,>).MakeGenericType(valueType, stratType);
                    
                    // Get a reference to the GMDPP's AddAction method field
                    var addActionField = specificLinker.GetField("AddAction", BindingFlags.Public | BindingFlags.Static);

                    // Create a direct delegate to avoid boxing/ExpressionTree overhead
                    var cb = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(stratType), instance, method);
                    
                    // Set the Value of the action field to the correct callback
                    addActionField.SetValue(null, cb);
                    
                    
                    Debug.Log($"[CacheStatFields] Successfully populated StatLinker<{valueType.Name}, {stratType.Name}>.AddAction");

                    // 2. Update the Instance-specific Router (Multi-entity safety)
                    if (istatuser.router.MethodCache_AddModifier != null)
                    {
                        // We use the stratType (e.g. SpeedModifier) as the key
                        istatuser.router.MethodCache_AddModifier[stratType] = cb;
                        Debug.Log($"[CacheStatFields] Added {stratType.Name} callback to {istatuser.GetType().Name}.router.MethodCache");
                    }
                    else
                    {
                        Debug.LogError($"[CacheStatFields] Router Dictionary is NULL on {istatuser.GetType().Name}. Ensure it is initialized in the constructor or Awake.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CacheStatFields] Failed to bind field {field.Name}: {e.Message}");
                }
            }
            
            Debug.Log($"[CacheStatFields] Completed caching for {istatuser.GetType().Name}");
        }
    }
}
