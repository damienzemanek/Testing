using System;
using EMILtools.Core;
using Sirenix.OdinInspector;


public abstract class BasicFunctionalityModule : MonoFunctionalityModule
{
    bool initialized;
    [NonSerialized] PersistentAction action;

    public BasicFunctionalityModule(PersistentAction action)
        => this.action = action;
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    protected virtual void Awake() { }
    
    public override void Bind() => action.Add(Execute);
    public override void Unbind() => action.Remove(Execute);

    public abstract void Execute();
}

public abstract class BasicFunctionalityModule<T> : MonoFunctionalityModule
{
    bool initialized;
    [NonSerialized] PersistentAction<T> action;
    
    public BasicFunctionalityModule(PersistentAction<T> action) 
        => this.action = action;
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    protected virtual void Awake() { }
    
    public override void Bind() => action.Add(Execute);
    public override void Unbind() => action.Remove(Execute);

    public abstract void Execute(T val);
}