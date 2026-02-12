using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class InputHeldModule<TPublisherArgs, TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    public InputHeldModule(PersistentAction<TPublisherArgs, bool> action, string name = "Functionality Module")
    {
        this.action = action;
        setGuarder = new TSetActionGuarder();
    }
    
    bool initialized;
    PersistentAction<TPublisherArgs, bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected TSetActionGuarder setGuarder;
    [ShowInInspector] protected ActionGuarderMutable executeGuarder;


    public override void Bind() => action.Add(OnSet);
    public override void Unbind() => action.Remove(OnSet);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        executeGuarder = new (new ActionGuard(() => !isActive, "Not Active"));
        Awake();
    }

    protected abstract void Awake();
    
    public void OnSet(TPublisherArgs args, bool v)
    {
        if (setGuarder != null && setGuarder.TryEarlyExit()) return;
        isActive = v;
        OnSetImplementation(args);
    }
    protected abstract void OnSetImplementation(TPublisherArgs args);

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (executeGuarder.TryEarlyExit()) return;
        Implementation(dt);
    }
    protected abstract void Implementation(float dt);
}

public abstract class InputHeldModule<TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    public InputHeldModule(PersistentAction<bool> action)
    {
        this.action = action;
        setGuarder = new();
    }
    
    bool initialized;
    PersistentAction<bool> action;
    [ShowInInspector] protected bool isActive;
    [ShowInInspector] protected TSetActionGuarder setGuarder;
    [ShowInInspector] protected ActionGuarderMutable executeGuarder;


    public override void Bind() => action.Add(OnSetTemplateCall);
    public override void Unbind() => action.Remove(OnSetTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        executeGuarder = new (new ActionGuard(() => !isActive, "Not Active"));
        Awake();
    }
    
    protected virtual void Awake() { }
    
    protected void OnSetTemplateCall(bool v)
    {
        if (setGuarder != null && setGuarder.TryEarlyExit()) return;
        isActive = v;
        OnSet();
    }
    protected abstract void OnSet();

    protected override void ExecuteTemplateCall(float dt) 
    {
        if (executeGuarder.TryEarlyExit()) return;
        Execute(dt);
    }
    protected abstract void Execute(float dt);
}