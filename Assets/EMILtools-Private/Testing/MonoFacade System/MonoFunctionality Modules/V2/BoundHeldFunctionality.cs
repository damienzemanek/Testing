using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IModuleUsabableContext : IPipelineContext { }
public struct Empty : IModuleUsabableContext { }

public interface IExecutableTempalate
{
    public Delegate TemplateCall { get; }
}

/// <summary>
/// Only to be used with PersistentAction (No Args)
/// - Use TContext to pass around information
/// </summary>
/// <typeparam name="TFacade"></typeparam>
/// <typeparam name="TContext"></typeparam>
public abstract class BoundFunctionality<TFacade, TContext> : 
        UnboundFunctionality<TFacade, TContext>, 
        IBindable
    where TFacade : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext
{
    [NonSerialized] PersistentAction action = new();
    protected BoundFunctionality(PersistentAction action, TFacade facade) : base(facade)
     => this.action = action;
    
    /// <summary>
    /// Binds the EXECUTION PIPELINE to the BOUND ACTION
    /// </summary>
    public virtual void Bind() => action.Add(ExecuteTemplateCall);
    public virtual void Unbind() => action.Remove(ExecuteTemplateCall);
}



/// <summary>
/// Can be used with PersistentAction<bool, T2, T3...>
/// Tracks: isActive
/// Params: PersistentAction T2, T3...
/// </summary>
/// <typeparam name="TFacade"></typeparam>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="SettableTemplate"></typeparam>
public abstract class BoundSetFunctionality<
        TFacade,
        TContext,
        SettableTemplate> 
        : UnboundFunctionality<TFacade, TContext>, IBindable
    where TFacade : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext
    where SettableTemplate : class, ISettableTemplate<bool>, new()
{
    /// <summary>
    /// Alias for Settable.unnamedStoredValue1
    /// </summary>
    protected bool isActive => Settable._unnamedStoredValue1;
    protected SettableTemplate SetContext => Settable;
    [NonSerialized] [ShowInInspector] SettableTemplate Settable;
    protected BoundSetFunctionality(IPersistentDelegate _action, TFacade facade) : base(facade)
    {
        Settable = new SettableTemplate();
        Settable.action = _action;
        Debug.Log($"Settable action is : " + Settable.action + $" and template call is : " + Settable.TemplateCall + $" for functionality : " + GetType().Name);
    }

    public void Bind() => Settable.action.API_Add(Settable.TemplateCall);
    public void Unbind() => Settable.action.API_Remove(Settable.TemplateCall);
}


/// <summary>
/// Settables manage generic state using Template Method Pattern
/// </summary>
/// <typeparam name="T1"></typeparam>
public interface ISettableTemplate<T1>
{
    public IPersistentDelegate action { get; set; }
    public Delegate TemplateCall { get; }
    
    /// <summary>
    /// All SettableTMP's will have at least 1 unnamedStoredValue1, meaning it is accessible from the interface
    /// </summary>
    public T1 _unnamedStoredValue1 { get; set; }
}


/// <summary>
/// Ensure you set aliases for values they are generic storage only
/// </summary>
/// <typeparam name="T1"></typeparam>
public abstract class SettableTemplate<T1> : ISettableTemplate<T1>
{
    public T1 unnamedStoredValue1 => (this as ISettableTemplate<T1>)._unnamedStoredValue1;
    // Values
    [field: NonSerialized] T1 ISettableTemplate<T1>._unnamedStoredValue1 { get; set; }
    
    // Template Call 
    [NonSerialized] readonly Action<T1> _templateCall; 
    public Delegate TemplateCall => _templateCall;
    void _TemplateCall(T1 val)
    {
        Debug.Log($"SettableTemplate<T1>: CALLED SET TEMPLATE CALL, val is: : {val} for : {GetType().Name}");
        ((ISettableTemplate<T1>)this)._unnamedStoredValue1 = val;
        Set(val);
    }
    
    // Ctor
    public SettableTemplate() => _templateCall = new Action<T1>(_TemplateCall);
    
    // Action
    [NonSerialized] IPersistentAction<Action<T1>> _action;
    public IPersistentDelegate action
    {
        get => _action;
        set
        {
            Debug.Log($"SettableTemplate<T1>: Setting action to : {value} for : {GetType().Name}");
            _action = (IPersistentAction<Action<T1>>)value;
        }
    }

    protected virtual void Set(T1 val) { }
}

/// <summary>
/// Ensure you set aliases for values they are for generic storage only
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class SettableTemplate<T1, T2> : ISettableTemplate<T1>
{
    public T1 unnamedStoredValue1 => (this as ISettableTemplate<T1>)._unnamedStoredValue1;
    // Values
    [field: NonSerialized] T1 ISettableTemplate<T1>._unnamedStoredValue1 { get; set; }
    public T2 unnamedStoredValue2 { get; set; }
    
    // Template Call
    readonly Action<T1, T2> _templateCall;
    public Delegate TemplateCall => _templateCall;
    void _TemplateCall(T1 val1, T2 val2)
    {
        Debug.Log($"SettableTemplate<T1>: CALLED SET TEMPLATE CALL, val is: : {val1} and {val2} for : {GetType().Name}");
        ((ISettableTemplate<T1>)this)._unnamedStoredValue1 = val1;
        unnamedStoredValue2 = val2;
        Set(val1, val2);
    }
    
    // Ctor
    public SettableTemplate() => _templateCall = new Action<T1, T2>(_TemplateCall);
    
    // Action
    [NonSerialized] IPersistentAction<Action<T1, T2>> _action;
    public IPersistentDelegate action
    {
        get => _action; 
        set => _action = (IPersistentAction<Action<T1, T2>>)value;
    }

    protected virtual void Set(T1 val1, T2 val2) { }
}
