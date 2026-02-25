using System;
using EMILtools_Private.Testing;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public abstract class MonoFacade<TMonoFacade, 
        TFunctionality,  // Systems and functionality
        TConfig,         // Immutable Configuration
        TBlackboard,     // References
        TContext,        // Transient state 
        TActionMap>:     // Internal Action bindings
        MonoBehaviour,
        IFacade
    where TMonoFacade : class, IFacade    
    where TConfig : Config                              
    where TBlackboard : Blackboard                       
    where TContext : struct, IModuleUsabableContext               
    where TFunctionality : Functionalities<TMonoFacade, TContext>, new()
    where TActionMap : class, IActionMap, new()
{
    public TContext context { get; set; }
    bool initialized = false;
    [field: Title("Action Mappings")]
    [field: ShowInInspector] [field:ReadOnly] [field:HideLabel] [field: NonSerialized] public TActionMap Actions { get; protected set; }
    [field: Title("Settings")]
    [field:SerializeField, Required] public TConfig Config { get; private set; }
    [field: Title("Blackboard")]
    [field:SerializeField, Required] [field:HideLabel] public TBlackboard Blackboard { get; private set; }
    [field: Title("Functionality Modules")]
    [field: ShowInInspector] [field:ReadOnly] [field:HideLabel] [field: NonSerialized] public TFunctionality Functionality { get; private set; }

    public T GetFunctionality<T>() where T : class, IAPI_Module
    {
        if (Functionality.APIs().TryGetValue(typeof(T), out var module))
            return module as T;
        if(module == null) Debug.LogWarning("Did not find module of type " + typeof(T));
        return null;
    }

    protected void InitializeFacade()
    {
        if (initialized) return;
        
        Actions = new();
        Functionality = new ();
        
        Debug.Assert(Config != null, $"{name}: Config not assigned");
        Debug.Assert(Blackboard != null, $"{name}: Blackboard not assigned");
        Debug.Assert(Functionality != null, $"{name}: Functionality did not initialize");

        Functionality.InjectFacadeReference(this);
        Functionality.SetupModules();   // Functionality must be last because it depends on the Config and the Blackboard
        
        initialized = true;
    }
    

    protected virtual void Update()
    {
        if (!initialized) return;
        Functionality.UpdateTick(Time.deltaTime);
    }
    
    protected virtual void FixedUpdate()
    {
        if (!initialized) return;
        Functionality.FixedTick(Time.deltaTime);
    }
    
    protected virtual void LateUpdate()
    {
        if (!initialized) return;
        Functionality.LateTick(Time.deltaTime);
    }
    
}