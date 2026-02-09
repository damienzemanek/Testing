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
    void Tick(float dt);
}

public interface FIXEDUPDATE : IModuleTick
{
    void FixedTick(float dt);
}

public interface LATEUPDATE : IModuleTick
{
    void LateTick(float dt);
}


[Serializable]
public abstract class FunctionalityModule : IModule
{
    public abstract void Init();
    public abstract void Bind();
    public abstract void Unbind();
    public abstract void Execute(float dt);
}


[Serializable]
public abstract class InputModuleInterior<TPublisherArgs, SetGateFlow, TCoreFacade> : InputModule<TPublisherArgs, SetGateFlow>, IInterior<TCoreFacade>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, ICoreFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:Required] [field:SerializeReference] public TCoreFacade facade { get; set; }

    protected InputModuleInterior(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
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
        Debug.Log("recieved " + action);
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
        Debug.Log("new action is " + this.action);

    }
    
    public bool isActive;

    public SetGateFlow SetGateFlowOut;
    public FlowMutable ExecuteGateFlowOut;


    [Button]
    public override void Bind()
    {
        Debug.Log(action);
        Debug.Log(action.Count);
        action.Add(OnSet);
        Debug.Log(action.Count);

    }
    public override void Unbind() => action.Remove(OnSet);
    
    public override void Init()
    {
        if (initialized) return; initialized = true;
        ExecuteGateFlowOut = new FlowMutable( ReturnIf("Not Active", () => !isActive) );
        InitImplementation();
    }
    
    public virtual void InitImplementation() { }
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        Debug.Log("b");
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
        Debug.Log("a");
    }
    public abstract void OnSetImplementation(TPublisherArgs args);

    public override void Execute(float dt) 
    {
        if (ExecuteGateFlowOut.TryEarlyExit()) return;
        ExecuteImplementation(dt);
    }
    public abstract void ExecuteImplementation(float dt);
}


