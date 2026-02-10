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

public interface NO_PUBLISHER_ARGS { }

[Serializable]
public abstract class FunctionalityModule : IModule
{
    public abstract void AwakeTemplateCall();
    public abstract void Bind();
    public abstract void Unbind();
    public virtual void ExecuteTemplateCall(float dt) { }
}


[Serializable]
public abstract class InputModuleHeldSubInterior<TPublisherArgs, SetGateFlow, TCoreFacade> : InputModuleHeld<TPublisherArgs, SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeField] public TCoreFacade facade { get; set; }

    protected InputModuleHeldSubInterior(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
    => this.facade = facade;
}

[Serializable]
public abstract class InputModuleHeldSubInterior<SetGateFlow, TCoreFacade> : InputModuleHeld<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeField] public TCoreFacade facade { get; set; }

    protected InputModuleHeldSubInterior(PersistentAction<bool> action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}

[Serializable]
public abstract class InputModulePressSubInterior<SetGateFlow, TCoreFacade> : InputModulePress<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeField] public TCoreFacade facade { get; set; }

    protected InputModulePressSubInterior(PersistentAction action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}



[Serializable]
public abstract class InputModuleHeld<TPublisherArgs, SetGateFlow> : FunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<TPublisherArgs, bool> action;

    public InputModuleHeld(PersistentAction<TPublisherArgs, bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void AwakeTemplateCall()
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
public abstract class InputModuleHeld<SetGateFlow> : FunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<bool> action;

    public InputModuleHeld(PersistentAction<bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void AwakeTemplateCall()
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
public abstract class InputModulePress<SetGateFlow> : FunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction action;

    public InputModulePress(PersistentAction action)
    {
        this.action = action;
        OnPressFlowOut = new();
    }
    
    public SetGateFlow OnPressFlowOut;


    public override void Bind() => action.Add(OnPressTemplateCall);
    public override void Unbind() => action.Remove(OnPressTemplateCall);
    
    public override void AwakeTemplateCall()
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


