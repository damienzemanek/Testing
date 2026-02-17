using System;
using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class BasicFunctionalityModule<TExecuteGuarder> : MonoFunctionalityModule
    where TExecuteGuarder : class, IActionGuarder, new()
{
    bool initialized;
    bool initGuarder;
    [NonSerialized] PersistentAction action;
    [ShowInInspector] protected TExecuteGuarder executeGuarder;

    public BasicFunctionalityModule(PersistentAction action, bool initGuarder)
    {
        this.action = action;
        this.initGuarder = initGuarder;
    }
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        if(initGuarder) executeGuarder = new TExecuteGuarder();
        Awake();
    }
    protected virtual void Awake() { }
    
    public override void Bind() => action.Add(ExecuteTemplateCall);
    public override void Unbind() => action.Remove(ExecuteTemplateCall);

    public void ExecuteTemplateCall()
    {
        if (initGuarder && executeGuarder.TryEarlyExit()) return;
        Execute();
    }
    
    public abstract void Execute();
}

public abstract class BasicFunctionalityModule<T, TExecuteGuarder> : MonoFunctionalityModule
    where TExecuteGuarder : class, IActionGuarder, new()
{
    bool initialized;
    bool initGuarder;
    [NonSerialized] PersistentAction<T> action;
    [ShowInInspector] protected TExecuteGuarder executeGuarder;

    public BasicFunctionalityModule(PersistentAction<T> action, bool initGuarder)
    {
        this.action = action;
        this.initGuarder = initGuarder;
    }
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        if(initGuarder)  executeGuarder = new TExecuteGuarder();
        Awake();
    }
    protected virtual void Awake() { }
    
    public override void Bind() => action.Add(ExecuteTemplateCall);
    public override void Unbind() => action.Remove(ExecuteTemplateCall);
    
    public void ExecuteTemplateCall(T val)
    {
        if (initGuarder && executeGuarder.TryEarlyExit()) return;
        Execute(val);
    }

    public abstract void Execute(T val);
}

