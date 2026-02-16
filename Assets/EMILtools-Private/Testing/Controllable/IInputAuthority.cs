using System;
using System.Collections.Generic;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IInputAuthority<TInputMap, TSubordinateEnum>
    where TSubordinateEnum : Enum
    where TInputMap : class, IInputMap, new()
{
    public Dictionary<int, Mapping> InputMappings { get; set; }
    
    public void RegisterSubordinateInstance(IInputSubordinate<TInputMap, TSubordinateEnum>.SubordinateContext context, Func<TInputMap> mapFactory = null)
    {
        int key = Convert.ToInt32(context.key);
        
        TInputMap mapInstance = mapFactory != null ? mapFactory() : new TInputMap();
        Mapping newMapping = new Mapping(context.Subordinate.Value, mapInstance);
        
        InputMappings[key] = newMapping;
        context.Subordinate.Value.Input = newMapping.inputMap; // Align subordinate's stored InputMap with the Mapping's Input map
        Debug.Log("Registered with key " + key + " with new Mapping w/ subordinate : " + context.Subordinate.Value);
    }

    public void UnregisterSubordinateInstance(int key)
    {
        InputMappings.Remove(key);
    }

    public void RequestDelegationOfAuthority(int key) => DelegateAuthorityToTemplateCall(key);
    
    protected void DelegateAuthorityToTemplateCall(int mapIndex)
    {
        if(!InputMappings.TryGetValue(mapIndex, out Mapping mapping)) Debug.LogError($"MapIndex {mapIndex} not found");
        
        DelegateAuthorityTo(mapIndex, mapping);
    }

    protected abstract void DelegateAuthorityTo(int mapIndex, Mapping mapping);
    
    public class Mapping
    {
        public TInputMap inputMap;
        public IInputSubordinate<TInputMap, TSubordinateEnum> subordinate;
        public IInitializable Initializable => subordinate as IInitializable;
        public Mapping(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate, TInputMap inputMap = null)
        {
            this.inputMap = inputMap;
            this.subordinate = subordinate;
        }
    }
}



