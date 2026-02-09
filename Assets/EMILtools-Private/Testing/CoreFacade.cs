using System;
using EMILtools_Private.Testing;
using KBCore.Refs;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

public interface ICoreFacade { }

[Serializable]
public class CoreFacade<TInputReader, TFunctionality, TConfig, TBlackboard, TCoreFacade>: ValidatedMonoBehaviour, ICoreFacade
    where TInputReader : ScriptableObject, IInputReader,      IInterior<TCoreFacade>
    where TFunctionality : Functionalities,                   IInterior<TCoreFacade>
    where TBlackboard : Blackboard,                           IInterior<TCoreFacade>
    where TConfig : Config                                    // Config does not need to be an interior because it should not have a reference to the facade, it is just data
    where TCoreFacade : ICoreFacade       
{
    bool coreFacadeInitialized = false;
    
    [field: Title("Settings")]
    [field:SerializeField, Required] public TInputReader Input { get; private set; }
    [field:SerializeField, Required] public TConfig Config { get; private set; }
    [field: Title("Blackboard")]
    [field:SerializeField, Required] [field:HideLabel] public TBlackboard Blackboard { get; private set; }
    [field: Title("Functionality Modules")]
    [field:SerializeField, Required] [field:HideLabel] public TFunctionality Functionality { get; private set; }
    

    [Button, PropertyOrder(-1)]
    public void Init()
    {
        Debug.Assert(Input != null, $"{name}: Input not assigned");
        Debug.Assert(Config != null, $"{name}: Config not assigned");
        Debug.Assert(Blackboard != null, $"{name}: Blackboard not assigned");
        Debug.Assert(Functionality != null, $"{name}: Functionality not assigned");
        
        Input.Init(this);
        Blackboard.Init(this);    // move up
        
        // Functionality must be last because it depends on the Config and the Blackboard
        Functionality.Init(this);

        coreFacadeInitialized = true;
    }

    protected virtual void Update()
    {
        if (!coreFacadeInitialized) return;
        Functionality.Tick(Time.deltaTime);
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


public interface IInterior<TCoreFacade>
    where TCoreFacade : ICoreFacade
{
    public TCoreFacade facade { get; set; }

    public void Init(ICoreFacade f)
    {
        if (f is TCoreFacade t) facade = t;
        else Debug.LogError($"Facade of type {f.GetType()} is not of type {typeof(TCoreFacade)}" );
        InitImplementation();
    }
    
    public virtual void InitImplementation() { }
}
