using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class InputPressedModule<SetGateFlow> : MonoFunctionalityModule
    where SetGateFlow : FlowOutChain, new()
{
    
    public InputPressedModule(PersistentAction action)
    {
        this.action = action;
        OnPressFlowOut = new();
    }
    
    bool initialized;
    PersistentAction action;
    [ShowInInspector] protected SetGateFlow OnPressFlowOut;
    
    
    public override void Bind() => action.Add(OnPressTemplateCall);
    public override void Unbind() => action.Remove(OnPressTemplateCall);
    
    public override void SetupModule()
    {
        if (initialized) return; initialized = true;
        Awake();
    }
    
    protected virtual void Awake() { }
    
    void OnPressTemplateCall()
    {
        if (OnPressFlowOut.TryEarlyExit()) return;
        OnPress();
    }
    protected abstract void OnPress();
    
}