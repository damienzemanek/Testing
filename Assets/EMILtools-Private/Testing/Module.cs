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


[Serializable]
public abstract class MonoFunctionalityModule : IModule
{
    public abstract void SetupModule();
    public virtual void ExecuteTemplateCall(float dt) { }
    public abstract void Bind();
    public abstract void Unbind();
}


[Serializable]
public abstract class InputHeldModuleFacade<TPublisherArgs, SetGateFlow, TCoreFacade> : InputHeldModule<TPublisherArgs, SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
    => this.facade = facade;
}

[Serializable]
public abstract class InputHeldModuleFacade<SetGateFlow, TCoreFacade> : InputHeldModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}

[Serializable]
public abstract class InputPressedModuleFacade<SetGateFlow, TCoreFacade> : InputPressedModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] public TCoreFacade facade { get; set; }

    protected InputPressedModuleFacade(PersistentAction action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}



[Serializable]
public abstract class InputHeldModule<TPublisherArgs, SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<TPublisherArgs, bool> action;

    public InputHeldModule(PersistentAction<TPublisherArgs, bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }

    public abstract void Awake();
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
    }
    public abstract void OnSetImplementation(TPublisherArgs args);

    public override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Implementation(dt);
    }
    public abstract void Implementation(float dt);
}

[Serializable]
public abstract class InputHeldModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<bool> action;

    public InputHeldModule(PersistentAction<bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }
    
    public virtual void Awake() { }
    
    public void OnSetTemplateCall(bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSet();
    }
    public abstract void OnSet();

    public override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Execute(dt);
    }
    public abstract void Execute(float dt);
}

[Serializable]
public abstract class InputPressedModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction action;

    public InputPressedModule(PersistentAction action)
    {
        this.action = action;
        OnPressFlowOut = new();
    }
    
    public SetGateFlow OnPressFlowOut;


    public override void Bind() => action.Add(OnPressTemplateCall);
    public override void Unbind() => action.Remove(OnPressTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    
    public virtual void Awake() { }
    
    public void OnPressTemplateCall()
    {
        if (OnPressFlowOut.TryEarlyExit()) return;
        OnPress();
    }
    public abstract void OnPress();
    
}


