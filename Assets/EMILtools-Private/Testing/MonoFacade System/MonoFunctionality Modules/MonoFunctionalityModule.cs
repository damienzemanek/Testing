using Sirenix.OdinInspector;
using UnityEngine;




/// <summary>
/// Optionally Context Dependant
/// Template Method Pattern
/// ValueType Execution Hook
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IExecutableTMP<TContext> 
    where TContext : struct
{
    public void ExecuteTemplateCall();
    public bool Execute(TContext ctx);
}

public interface IInjectablePipeline<TContext>
    where TContext : struct, IPipelineContext
{
    public Pipeline<TContext> executionPipeline { get; set; }
    
    /// <summary>
    /// Inject the size of the pipeline for runtime immutability and stability
    /// </summary>
    public abstract int injectAmountOfAddedSteps { get; }

    public virtual Pipeline<TContext> InjectStepsWithFinalStep(PipelineBuilder<TContext> builder)
        => throw new System.NotImplementedException();

    public virtual PipelineBuilder<TContext> InjectSteps(PipelineBuilder<TContext> builder)
        => throw new System.NotImplementedException();  

    public virtual PipelineStepDelegate<TContext> InjectFinalStep() 
        => throw new System.NotImplementedException(); 
    
    public void Setup(bool setupWithFinalStep)
    {
        // + 1 to accomodate for the final step
        var builder = new PipelineBuilder<TContext>(injectAmountOfAddedSteps + 1);
        if (setupWithFinalStep)
            executionPipeline = InjectStepsWithFinalStep(builder);
        else
            executionPipeline = InjectSteps(builder).FinalStep(InjectFinalStep());
    }
}


public interface IBindable
{
    public void Bind();
    public void Unbind();
}

public interface ISettableTMP<T>
{
    public void SetTemplateCall(T value);
    public void PostSetHook(T value);
}

    

public abstract class MonoFunctionalityModule<TFacade, TContext> 
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
{
    public TFacade facade { get; set; }
    public TContext context => (TContext)facade.Context;
    
    [Title("$Name"), PropertyOrder(-1)]
    [ShowInInspector] public string Name => "Module: " + this.GetType().Name;
    public abstract void SetupModule();
    protected virtual void Awake(TContext ctx) { }
    
    public MonoFunctionalityModule(TFacade facade) => this.facade = facade;

}