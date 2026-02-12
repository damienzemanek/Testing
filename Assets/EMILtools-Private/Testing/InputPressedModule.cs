using EMILtools.Core;
using Sirenix.OdinInspector;

public abstract class InputPressedModule<TSetActionGuarder> : MonoFunctionalityModule
    where TSetActionGuarder : IActionGuarder, new()
{
    
    public InputPressedModule(PersistentAction action)
    {
        this.action = action;
        onPressGuarder = new();
    }
    
    bool initialized;
    PersistentAction action;
    [ShowInInspector] protected TSetActionGuarder onPressGuarder;
    
    
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
        if (onPressGuarder.TryEarlyExit()) return;
        OnPress();
    }
    protected abstract void OnPress();
    
}