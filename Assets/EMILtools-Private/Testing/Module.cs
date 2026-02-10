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
    public abstract void AwakeTemplate();
    public abstract void Bind();
    public abstract void Unbind();
    public abstract void Execute(float dt);
}


[Serializable]
public abstract class InputModuleSubInterior<TPublisherArgs, SetGateFlow, TCoreFacade> : InputModule<TPublisherArgs, SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeField] public TCoreFacade facade { get; set; }

    protected InputModuleSubInterior(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
    => this.facade = facade;
}

[Serializable]
public abstract class InputModuleSubInterior<SetGateFlow, TCoreFacade> : InputModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeField] public TCoreFacade facade { get; set; }

    protected InputModuleSubInterior(PersistentAction<bool> action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}





[Serializable]
public abstract class InputModule<TPublisherArgs, SetGateFlow> : FunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<TPublisherArgs, bool> action;

    public InputModule(PersistentAction<TPublisherArgs, bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteGateFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void AwakeTemplate()
    {
        if (initialized) return; initialized = true;
        ExecuteGateFlowOut = new FlowMutable( ReturnIf("Not Active", () => !isActive) );
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

    public override void Execute(float dt) 
    {
        if (ExecuteGateFlowOut.TryEarlyExit()) return;
        Implementation(dt);
    }
    public abstract void Implementation(float dt);
}

[Serializable]
public abstract class InputModule<SetGateFlow> : FunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    bool initialized;

    [ShowInInspector, NonSerialized, ReadOnly] public PersistentAction<bool> action;

    public InputModule(PersistentAction<bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteGateFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void AwakeTemplate()
    {
        if (initialized) return; initialized = true;
        ExecuteGateFlowOut = new FlowMutable( ReturnIf("Not Active", () => !isActive) );
        Awake();
    }
    
    public virtual void Awake() { }
    
    public void OnSet(bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation();
    }
    public abstract void OnSetImplementation();

    public override void Execute(float dt) 
    {
        if (ExecuteGateFlowOut.TryEarlyExit()) return;
        Implementation(dt);
    }
    public abstract void Implementation(float dt);
}


