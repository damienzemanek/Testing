using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IModuleUsabableContext : IPipelineContext { }
public struct Empty : IModuleUsabableContext { }

public abstract class BoundFunctionality<TFacade, TContext> : UnboundFunctionality<TFacade, TContext>, 
        IBindable
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
{
    [NonSerialized] PersistentAction action = new();

    protected BoundFunctionality(TFacade facade) : base(facade) { }
    public void Bind() => action.Add(ExecutableTMP.ExecuteTemplateCall);
    public void Unbind() => action.Remove(ExecutableTMP.ExecuteTemplateCall);
}

public struct None { }



public abstract class BoundHeldFunctionality<TFacade, TContext, TPeristentActionType> : UnboundFunctionality<TFacade, TContext>,
    ISettableTMP<bool>,
    IBindable
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
    where TPeristentActionType : IPersistentAction
{
    [ShowInInspector] protected bool isActive = false;
    [NonSerialized] TPeristentActionType action = new();

    protected BoundHeldFunctionality(TPeristentActionType action, TFacade facade) : base(facade)
        => this.action = action;

    public void Bind() => action.Add(SetTemplateCall);
    public void Unbind() => action.Remove(SetTemplateCall);
    
    public void SetTemplateCall(bool value) { isActive = value; PostSetHook(isActive); }
    
    // I dont want to call this OnSet cause then it seems like an Action
    
    /// <summary>
    /// Optional mutation after setting the isActive state
    /// </summary>
    /// <param name="value"></param>
    public virtual void PostSetHook(bool value) { }
}
