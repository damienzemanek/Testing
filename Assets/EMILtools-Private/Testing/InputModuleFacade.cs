using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static FlowOutChain;



public abstract class InputHeldModuleFacade<TPublisherArgs, SetGateFlow, TCoreFacade> : InputHeldModule<TPublisherArgs, SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade) :
        base(action)
    => this.facade = facade;
}

public abstract class InputHeldModuleFacade<SetGateFlow, TCoreFacade> : InputHeldModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}

public abstract class InputPressedModuleFacade<SetGateFlow, TCoreFacade> : InputPressedModule<SetGateFlow>
    where SetGateFlow : FlowOutChain, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }

    protected InputPressedModuleFacade(PersistentAction action, TCoreFacade facade) : base(action)
        => this.facade = facade;
    
}


