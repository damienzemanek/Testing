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
    
    public void Register(IInputSubordinate<TInputMap, TSubordinateEnum>.SubordinateContext context)
    {
        int key = Convert.ToInt32(context.key);
        Mapping newMapping = new Mapping(context.Subordinate.Value);
        InputMappings[key] = newMapping;
        context.Subordinate.Value.Input = newMapping.inputMap;
        Debug.Log("Registered with key " + key + " with new Mapping w/ subordinate : " + context.Subordinate.Value);
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
        public Mapping(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate)
        {
            inputMap = new TInputMap();
            this.subordinate = subordinate;
        }
    }
}
