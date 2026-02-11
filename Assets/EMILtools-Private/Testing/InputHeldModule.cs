using EMILtools.Core;
using Sirenix.OdinInspector;
using static FlowOutChain;

public abstract class InputHeldModule<TPublisherArgs, SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    public InputHeldModule(PersistentAction<TPublisherArgs, bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    bool initialized;
    PersistentAction<TPublisherArgs, bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected SetGateFlow SetGateFlowOut;
    [ShowInInspector] protected FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }

    protected abstract void Awake();
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
    }
    protected abstract void OnSetImplementation(TPublisherArgs args);

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Implementation(dt);
    }
    protected abstract void Implementation(float dt);
}

public abstract class InputHeldModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    public InputHeldModule(PersistentAction<bool> action)
    {
        this.action = action;
        SetGateFlowOut = new SetGateFlow();
    }
    
    bool initialized;
    PersistentAction<bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected SetGateFlow SetGateFlowOut;
    [ShowInInspector] protected FlowMutable ExecuteFlowOut;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        ExecuteFlowOut = new FlowMutable( Return("Not Active", () => !isActive) );
        Awake();
    }
    
    protected virtual void Awake() { }
    
    protected void OnSetTemplateCall(bool v)
    {
        if (SetGateFlowOut != null && SetGateFlowOut.TryEarlyExit()) return;
        isActive = v;
        OnSet();
    }
    protected abstract void OnSet();

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (ExecuteFlowOut.TryEarlyExit()) return;
        Execute(dt);
    }
    protected abstract void Execute(float dt);
}