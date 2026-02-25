using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IModuleUsabableContext : IPipelineContext { }
public struct Empty : IModuleUsabableContext { }

public abstract class BoundFunctionality<TFacade, TContext, TPerisistentAction> : UnboundFunctionality<TFacade, TContext>, 
        IBindable
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
    where TPerisistentAction : class, IPersistentAction<Action, TPerisistentAction>, new()
{
    [NonSerialized] TPerisistentAction action = new();
    protected BoundFunctionality(TPerisistentAction action, TFacade facade) : base(facade)
        => this.action = action;
    
    public void Bind() => action.Add(ExecutableTMP.ExecuteTemplateCall);
    public void Unbind() => action.Remove(ExecutableTMP.ExecuteTemplateCall);
}


public abstract class BoundHeldFunctionality<
        TFacade,
        TContext,
        TSettableTMP> 
        : UnboundFunctionality<TFacade, TContext>, IBindable
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
    where TSettableTMP : class, ISettableTemplate<bool>, new()
{
    /// <summary>
    /// Alias for Settable.value1
    /// </summary>
    protected bool isActive => Settable.unnamedStoredValue1;
    [field: NonSerialized] public TSettableTMP Settable { get; protected set; }
    protected BoundHeldFunctionality(IPersistentDelegate _action, TFacade facade) : base(facade)
        => Settable = new TSettableTMP { action = _action };
    public void Bind() => Settable.action.API_Add(Settable.TemplateCall);
    public void Unbind() => Settable.action.API_Remove(Settable.TemplateCall);  
}


public interface ISettableTemplate<T1>
{
    public IPersistentDelegate action { get; set; }
    public Delegate TemplateCall { get; }
    
    /// <summary>
    /// All SettableTMP's will have at least 1 value1, meaning it is accessible from the interface
    /// </summary>
    protected internal T1 unnamedStoredValue1 { get; set; }
}


/// <summary>
/// Ensure you set aliases for values they are generic storage only
/// </summary>
/// <typeparam name="T1"></typeparam>
public abstract class SettableTemplate<T1> : ISettableTemplate<T1>
{
    // Values
    [field: NonSerialized] T1 ISettableTemplate<T1>.unnamedStoredValue1 { get; set; }
    
    // Template Call 
    [NonSerialized] readonly Action<T1> _templateCall; 
    public Delegate TemplateCall => _templateCall;
    void _TemplateCall(T1 val)
    {
        ((ISettableTemplate<T1>)this).unnamedStoredValue1 = val;
        Set(val);
    }
    
    // Ctor
    public SettableTemplate() => _templateCall = new Action<T1>(_TemplateCall);
    
    // Action
    [NonSerialized] public IPersistentAction<Action<T1>> _action;
    public IPersistentDelegate action
    {
        get => _action; 
        set => _action = (IPersistentAction<Action<T1>>)value;
    }

    public abstract void Set(T1 val);
}

/// <summary>
/// Ensure you set aliases for values they are for generic storage only
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class SettableTemplate<T1, T2> : ISettableTemplate<T1>
{
    // Values
    T1 ISettableTemplate<T1>.unnamedStoredValue1 { get; set; }
    protected T2 unnamedStoredValue2 { get; set; }
    
    // Template Call
    readonly Action<T1, T2> _templateCall;
    public Delegate TemplateCall => _templateCall;
    void _TemplateCall(T1 val1, T2 val2)
    {
        ((ISettableTemplate<T1>)this).unnamedStoredValue1 = val1;
        unnamedStoredValue2 = val2;
        Set(val1, val2);
    }
    
    // Ctor
    public SettableTemplate() => _templateCall = new Action<T1, T2>(_TemplateCall);
    
    // Action
    [NonSerialized] public IPersistentAction<Action<T1, T2>> _action;
    public IPersistentDelegate action
    {
        get => _action; 
        set => _action = (IPersistentAction<Action<T1, T2>>)value;
    }

    public abstract void Set(T1 val1, T2 val2);
}
