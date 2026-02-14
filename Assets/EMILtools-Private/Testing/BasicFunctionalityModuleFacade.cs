using System;
using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class BasicFunctionalityModuleFacade<T, TCoreFacade> : BasicFunctionalityModule<T>
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected BasicFunctionalityModuleFacade(PersistentAction<T> action, TCoreFacade facade) : base(action) 
        => this.facade = facade;
}

public abstract class BasicFunctionalityModuleFacade<TCoreFacade> : BasicFunctionalityModule
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected BasicFunctionalityModuleFacade(PersistentAction action, TCoreFacade facade) : base(action) 
        => this.facade = facade;
}
