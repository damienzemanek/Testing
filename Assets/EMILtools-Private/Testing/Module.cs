using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static FlowOutChain;

public interface IModule { }

public interface IModuleTick { }

public interface UPDATE : IModuleTick
{
    void OnUpdateTick(float dt);
}

public interface FIXEDUPDATE : IModuleTick
{
    void OnFixedTick(float dt);
}

public interface LATEUPDATE : IModuleTick
{
    void LateTick(float dt);
}


public abstract class MonoFunctionalityModule : IModule
{
    public abstract void SetupModule();
    protected virtual void ExecuteTemplateCall(float dt) { }
    public abstract void Bind();
    public abstract void Unbind();
}


public abstract class InputHeldModuleFacade<TPublisherArgs, SetGateFlow, TCoreFacade> : InputHeldModule<TPublisherArgs, SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
    => this.facade = facade;
}

public abstract class InputHeldModuleFacade<SetGateFlow, TCoreFacade> : InputHeldModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}

public abstract class InputPressedModuleFacade<SetGateFlow, TCoreFacade> : InputPressedModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputPressedModuleFacade(PersistentAction action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}



public abstract class InputHeldModule<TPublisherArgs, SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    public InputHeldModule(PersistentAction<TPublisherArgs, bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    bool initialized;
    PersistentAction<TPublisherArgs, bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected SetGateFlow SetGateFlowOut;
    [ShowInInspector] protected FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }

    protected abstract void Awake();
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
    }
    protected abstract void OnSetImplementation(TPublisherArgs args);

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Implementation(dt);
    }
    protected abstract void Implementation(float dt);
}

public abstract class InputHeldModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    public InputHeldModule(PersistentAction<bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    bool initialized;
    PersistentAction<bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected SetGateFlow SetGateFlowOut;
    [ShowInInspector] protected FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }
    
    protected virtual void Awake() { }
    
    protected void OnSetTemplateCall(bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSet();
    }
    protected abstract void OnSet();

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Execute(dt);
    }
    protected abstract void Execute(float dt);
}

public abstract class InputPressedModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    
    public InputPressedModule(PersistentAction action)
    {
        this.action = action;
        OnPressFlowOut = new();
    }
    
    bool initialized;
    PersistentAction action;
    [ShowInInspector] protected SetGateFlow OnPressFlowOut;
    
    
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
        if (OnPressFlowOut.TryEarlyExit()) return;
        OnPress();
    }
    protected abstract void OnPress();
    
}


