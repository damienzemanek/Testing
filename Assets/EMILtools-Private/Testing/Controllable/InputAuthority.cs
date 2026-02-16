using System;
using System.Collections.Generic;
using EMILtools.Core;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class InputAuthority<TInputReader, TInputMap, TSubordinateEnum> : ValidatedMonoBehaviour, 
     IInputAuthority<TInputMap, TSubordinateEnum>
     where TInputMap : class, IInputMap, new()
     where TInputReader : ScriptableObject, IInputReader<TInputMap>, IInitializable
     where TSubordinateEnum : Enum
{
     [SerializeField] protected TInputReader Reader;
     [SerializeField] protected int mappingCount;
     [ShowInInspector, ReadOnly] protected int currentMapping;
     
     [FoldoutGroup("Presetting & First Delegation Settings")] [SerializeField] protected bool presetWithInitialSubordinate;
     [FoldoutGroup("Presetting & First Delegation Settings")] [SerializeField] protected bool presetWithCustomInputMap;
     [FoldoutGroup("Presetting & First Delegation Settings")] [ShowIf("presetWithCustomInputMap")] public TInputMap inputMapSettings;
     [FoldoutGroup("Presetting & First Delegation Settings")] [ShowIf("presetWithInitialSubordinate")] public InterfaceReference<IInputSubordinate<TInputMap, TSubordinateEnum>, MonoBehaviour> InitialSubordinate;
     
      [ShowInInspector] public Dictionary<int, IInputAuthority<TInputMap, TSubordinateEnum>.Mapping> InputMappings { get; set; }

      protected virtual void Awake()
      {
          InitializeMappingsList(mappingCount);
          if(presetWithInitialSubordinate) InitialSubordinate.Value.subordinateContext.
              FirstDelegationOfAuthority(presetWithCustomInputMap ? inputMapSettings : null);
      }

      void IInputAuthority<TInputMap, TSubordinateEnum>.DelegateAuthorityTo(int mapIndex, IInputAuthority<TInputMap, TSubordinateEnum>.Mapping mapping)
      {
          currentMapping = mapIndex;
          Reader.InputMap = mapping.inputMap;
          Reader.Init();
      }
     
     
     protected void InitializeMappingsList(int amountOfMappings)
          => InputMappings = new Dictionary<int, IInputAuthority<TInputMap, TSubordinateEnum>.Mapping>(amountOfMappings);
    
}