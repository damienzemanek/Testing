using System;
using EMILtools.Core;
using Sirenix.OdinInspector;



public abstract class InputPressedModule : MonoFunctionalityModule
{
    public InputPressedModule(PersistentAction action)
     => this.action = action;
    
    bool initialized;
    [NonSerialized] PersistentAction action;
    
    
    public override void Bind() => action.Add(OnPress);
    public override void Unbind() => action.Remove(OnPress);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    
    protected virtual void Awake() { }
    protected abstract void OnPress();
    
}

public abstract class InputPressedModule<TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    
    public InputPressedModule(PersistentAction action)
    {
        this.action = action;
        onPressGuarder = new();
    }
    
    bool initialized;
    [NonSerialized] PersistentAction action;
    [ShowInInspector] protected TSetActionGuarder onPressGuarder;
    
    
    public override void Bind() => action.Add(OnPressTemplateCall);
    public override void Unbind() => action.Remove(OnPressTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    
    protected virtual void Awake() { }
    
    void OnPressTemplateCall()
    {
        if (onPressGuarder.TryEarlyExit()) return;
        OnPress();
    }
    protected abstract void OnPress();
    
}

public abstract class InputPressedModule<T, TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    
    public InputPressedModule(PersistentAction<T> action)
    {
        this.action = action;
        onPressGuarder = new();
    }
    
    bool initialized;
    [NonSerialized] PersistentAction<T> action;
    [ShowInInspector] protected TSetActionGuarder onPressGuarder;
    
    
    public override void Bind() => action.Add(OnPressTemplateCall);
    public override void Unbind() => action.Remove(OnPressTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    
    protected virtual void Awake() { }
    
    void OnPressTemplateCall(T val)
    {
        if (onPressGuarder.TryEarlyExit()) return;
        OnPress(val);
    }
    protected abstract void OnPress(T val);
    
}


