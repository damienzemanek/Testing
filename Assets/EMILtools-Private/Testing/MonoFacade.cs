using System;
using EMILtools_Private.Testing;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IFacade { }

[Serializable]
public class MonoFacade<TMonoFacade, TFunctionality, TConfig, TBlackboard>: ValidatedMonoBehaviour, IFacade
    where TMonoFacade : IFacade    
    where TConfig : Config                                 // Config does not need to be an interior because it should not have a reference to the facade, it is just data
    where TBlackboard : Blackboard                       
    where TFunctionality : Functionalities<TMonoFacade>,   IFacadeCompositionElement<TMonoFacade>, new()
{
    bool coreFacadeInitialized = false;
    
    [field: Title("Settings")]
    [field:SerializeField, Required] public TConfig Config { get; private set; }
    [field: Title("Blackboard")]
    [field:SerializeField, Required] [field:HideLabel] public TBlackboard Blackboard { get; private set; }
    [field: Title("Functionality Modules")]
    [field: ShowInInspector] [field:ReadOnly] [field:HideLabel] [field: NonSerialized] public TFunctionality Functionality { get; private set; }
    

    public virtual void InitializeFacade()
    {
        Functionality = new ();
        
        Debug.Assert(Config != null, $"{name}: Config not assigned");
        Debug.Assert(Blackboard != null, $"{name}: Blackboard not assigned");
        Debug.Assert(Functionality != null, $"{name}: Functionality did not initialize");
        
        Functionality.ComposeElement(this);   // Functionality must be last because it depends on the Config and the Blackboard
        
        coreFacadeInitialized = true;
    }
    

    protected virtual void Update()
    {
        if (!coreFacadeInitialized) return;
        Functionality.UpdateTick(Time.deltaTime);
    }
    
    protected virtual void FixedUpdate()
    {
        if (!coreFacadeInitialized) return;
        Functionality.FixedTick(Time.deltaTime);
    }
    
    protected virtual void LateUpdate()
    {
        if (!coreFacadeInitialized) return;
        Functionality.LateTick(Time.deltaTime);
    }
}