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
     where TInputReader : ScriptableObject, IInputReaderSubordinate<TInputMap, TSubordinateEnum>, IInitializable
     where TSubordinateEnum : Enum
{
     [SerializeField, Required] protected TInputReader Reader;
     [ShowInInspector, ReadOnly] public IInputSubordinate<TInputMap, TSubordinateEnum> subordinate { get; set; }
     [ShowInInspector, ReadOnly] bool initializedReader = false;
     
     [FoldoutGroup("Presetting & Initial Subordinate Settings")] [SerializeField] protected bool presetWithInitialSubordinate;
     [FoldoutGroup("Presetting & Initial Subordinate Settings")] [SerializeField] protected bool presetWithCustomInputMap;
     [FoldoutGroup("Presetting & Initial Subordinate Settings")] [ShowIf("presetWithCustomInputMap")] public TInputMap inputMapSettings;
     [FoldoutGroup("Presetting & Initial Subordinate Settings")] [ShowIf("presetWithInitialSubordinate")] public InterfaceReference<IInputSubordinate<TInputMap, TSubordinateEnum>, MonoBehaviour> InitialSubordinate;


      protected virtual void Awake()
      {
          if (presetWithInitialSubordinate) InitialSubordinate.Value.SetupFirstAuthority(inputMapSettings, this);
      }
      
      void IInputAuthority<TInputMap, TSubordinateEnum>.ReceiveRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate)
      {
          Reader.subordinate = subordinate;
          if(initializedReader) return;
          Reader.Init();
          initializedReader = true;
      }

      
      
    
}