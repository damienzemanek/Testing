using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public abstract class InputHeldModuleFacade<TPublisherArgs, TSetActionGuarder, TCoreFacade> : InputHeldModule<TPublisherArgs, TSetActionGuarder>
    where TSetActionGuarder : IActionGuarder, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade, bool useIsActiveGuard)
        : base(action, useIsActiveGuard: useIsActiveGuard)
    => this.facade = facade;
}

public abstract class InputHeldModuleFacade<TSetActionGuarder, TCoreFacade> : InputHeldModule<TSetActionGuarder>
    where TSetActionGuarder : IActionGuarder, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade, bool useIsActiveGuard) : base(action, useIsActiveGuard)
        => this.facade = facade;
}

public abstract class InputPressedModuleFacade<TSetActionGuarder, TCoreFacade> : InputPressedModule<TSetActionGuarder>
    where TSetActionGuarder : IActionGuarder, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputPressedModuleFacade(PersistentAction action, TCoreFacade facade)
        : base(action)
    => this.facade = facade;
}

public abstract class InputPressedModuleFacade<T, TSetActionGuarder, TCoreFacade> : InputPressedModule<T, TSetActionGuarder>
    where TSetActionGuarder : IActionGuarder, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputPressedModuleFacade(PersistentAction<T> action, TCoreFacade facade)
        : base(action)
        => this.facade = facade;
}


