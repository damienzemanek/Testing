using System;
using UnityEngine;

public abstract class UnboundFunctionality<TFacade, TContext> : MonoFunctionalityModule<TFacade, TContext>, 
    IExecuteTemplate<TContext>, 
    IInjectablePipeline<TContext>
    where TFacade : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext
{
    protected UnboundFunctionality(TFacade facade) : base(facade) { }
    public abstract int injectAmountOfAddedSteps { get; }
    public abstract PipelineBuilder<TContext> AddPipelineStepsHere(PipelineBuilder<TContext> builder);
    public Pipeline<TContext> executionPipeline { get; set; }
    public PipelineStepDelegate<TContext> InjectFinalStep() => new(Execute);
    
    protected IInjectablePipeline<TContext> injectablePipeline => this;
    protected IExecuteTemplate<TContext> ExecuteTemplate => this;
    public override void SetupModule()
    {
        injectablePipeline.Setup(setupWithFinalStep: false);
        Awake();
    }

    public void ExecuteTemplateCall()
    {
        Debug.Log("Trying to ExecuteTemplateCall " + GetType().Name);
        context.TryTo(executionPipeline);
        Debug.Log("COMPLETED ExecuteTemplateCall " + GetType().Name);

    }
    public abstract bool Execute(TContext ctx);
    
}