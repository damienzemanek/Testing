using System;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UnboundFunctionality<TFacade, TContext> : MonoFunctionalityModule<TFacade, TContext>, 
    IExecuteTemplate<TContext>, 
    IInjectablePipeline<TContext>
    where TFacade : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext
{
    // Variables
    public Pipeline<TContext> executionPipeline { get; set; }
    
    // Ctor
    protected UnboundFunctionality(TFacade facade) : base(facade) { }
    
    // API Access
    protected IInjectablePipeline<TContext> injectablePipeline => this;
    protected IExecuteTemplate<TContext> ExecuteTemplate => this;
    
    // Methods
    public abstract PipelineBuilder<TContext> InjectSteps(PipelineBuilder<TContext> builder);
    public PipelineStepDelegate<TContext> InjectMainStep() => new(Execute);
    [Button] public void ExecuteTemplateCall() => context.TryTo(executionPipeline);
    public abstract bool Execute(TContext ctx);
    public override void SetupModule()
    {
        injectablePipeline.Setup(setupWithFinalStep: false);
        Awake();
    }
    



    
}