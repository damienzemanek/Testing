using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class InputHeldModule<TPublisherArgs, TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    public InputHeldModule(PersistentAction<TPublisherArgs, bool> action, string name = "Functionality Module", bool useIsActiveGuard = true)
    {
        this.action = action;
        this.useIsActiveGuard = useIsActiveGuard;
        setGuarder = new TSetActionGuarder();
    }
    
    bool initialized;
    bool useIsActiveGuard;
    PersistentAction<TPublisherArgs, bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected TSetActionGuarder setGuarder;
    [ShowInInspector] protected ActionGuarderMutable executeGuarder;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        if (useIsActiveGuard) executeGuarder = new(new ActionGuard(() => !isActive, "Not Active"));
        else executeGuarder = new ActionGuarderMutable();
        Awake();
    }

    protected virtual void Awake() { }
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        if (setGuarder != null && setGuarder.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
    }
    protected abstract void OnSetImplementation(TPublisherArgs args);

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (executeGuarder.TryEarlyExit()) return;
        Implementation(dt);
    }
    protected abstract void Implementation(float dt);
}

public abstract class InputHeldModule<TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    public InputHeldModule(PersistentAction<bool> action, bool useIsActiveGuard = true)
    {
        this.action = action;
        this.useIsActiveGuard = useIsActiveGuard;
        setGuarder = new();
    }
    
    bool initialized;
    bool useIsActiveGuard;
    [NonSerialized] PersistentAction<bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected TSetActionGuarder setGuarder;
    [ShowInInspector] protected ActionGuarderMutable executeGuarder;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        if (useIsActiveGuard) executeGuarder = new(new ActionGuard(() => !isActive, "Not Active"));
        else executeGuarder = new ActionGuarderMutable();
        Awake();
    }
    
    protected virtual void Awake() { }
    
    protected void OnSetTemplateCall(bool v)
    {
        if (setGuarder != null && setGuarder.TryEarlyExit()) return;
        isActive = v;
        OnSet();
    }
    protected virtual void OnSet() { }

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (executeGuarder.TryEarlyExit()) return;
        Implementation(dt);
    }
    protected abstract void Implementation(float dt);
}