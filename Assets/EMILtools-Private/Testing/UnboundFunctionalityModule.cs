using System;
using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class UnboundFunctionalityModule<TExecuteGuarder> : MonoFunctionalityModule
    where TExecuteGuarder : class, IActionGuarder, new()
{
    bool initialized;
    bool initGuarder;
    [ShowInInspector] protected TExecuteGuarder executeGuarder;

    public UnboundFunctionalityModule(bool initGuarder) => this.initGuarder = initGuarder;
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        if (initGuarder) executeGuarder = new TExecuteGuarder();
        Awake();
    }
    protected virtual void Awake() { }
    public override void Bind() { }
    public override void Unbind() { }
    public void ExecuteTemplateCall(float dt)
    {
        if (executeGuarder.TryEarlyExit()) return;
        Execute();
    }
    public abstract void Execute();
}

public abstract class UnboundFunctionalityModuleFacade<TCoreFacade, TExecuteGuarder> : UnboundFunctionalityModule<TExecuteGuarder>
    where TExecuteGuarder : class, IActionGuarder, new()
    where TCoreFacade : class, IFacade
{
    [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }


    public UnboundFunctionalityModuleFacade(TCoreFacade facade, bool initGuarder) : base(initGuarder) 
        => this.facade = facade;
    
}