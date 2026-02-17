using System;
using EMILtools.Core;
using Sirenix.OdinInspector;



/// <summary>
/// Choice of TExecuteGuarder, 1 Args
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TCoreFacade"></typeparam>
/// <typeparam name="TExecuteGuarder"></typeparam>
public abstract class BasicFunctionalityModuleFacade<T, TCoreFacade, TExecuteGuarder> : BasicFunctionalityModule<T, TExecuteGuarder>
    where TCoreFacade : class, IFacade
    where TExecuteGuarder : class, IActionGuarder, new()
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected BasicFunctionalityModuleFacade(PersistentAction<T> action, TCoreFacade facade, bool initGuarder) : base(action, initGuarder) 
        => this.facade = facade;
}


/// <summary>
/// Choice of TExecuteGuarder, No Args
/// </summary>
/// <typeparam name="TCoreFacade"></typeparam>
/// <typeparam name="TExecuteGuarder"></typeparam>
public abstract class BasicFunctionalityModuleFacade<TCoreFacade, TExecuteGuarder> : BasicFunctionalityModule<TExecuteGuarder>
    where TCoreFacade : class, IFacade
    where TExecuteGuarder : class, IActionGuarder, new()

{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected BasicFunctionalityModuleFacade(PersistentAction action, TCoreFacade facade, bool initGuarder) : base(action, initGuarder) 
        => this.facade = facade;
}

/// <summary>
/// No Guarder, No Args
/// </summary>
/// <typeparam name="TCoreFacade"></typeparam>
public abstract class BasicFunctionalityModuleFacade<TCoreFacade> : BasicFunctionalityModule<ActionGuarderMutable>
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected BasicFunctionalityModuleFacade(PersistentAction action, TCoreFacade facade) : base(action, false) 
        => this.facade = facade;
}

