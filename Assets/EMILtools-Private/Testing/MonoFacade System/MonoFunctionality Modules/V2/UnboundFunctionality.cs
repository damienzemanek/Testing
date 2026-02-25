public abstract class UnboundFunctionality<TFacade, TContext> : MonoFunctionalityModule<TFacade, TContext>, 
    IExecutableTMP<TContext>, 
    IInjectablePipeline<TContext>
    where TFacade : class, IFacade
    where TContext : struct, IModuleUsabableContext
{
    protected UnboundFunctionality(TFacade facade) : base(facade) { }
    public abstract int injectAmountOfAddedSteps { get; }
    public abstract PipelineBuilder<TContext> InjectSteps(PipelineBuilder<TContext> builder);
    public Pipeline<TContext> executionPipeline { get; set; }
    public PipelineStepDelegate<TContext> InjectFinalStep() => new(Execute);
    
    protected IInjectablePipeline<TContext> injectablePipeline => this;
    protected IExecutableTMP<TContext> ExecutableTMP => this;
    
    public override void SetupModule()
    {
        injectablePipeline.Setup(setupWithFinalStep: false);
        Awake(context);
    }
    public void ExecuteTemplateCall() => context.TryTo(executionPipeline);
    public abstract bool Execute(TContext ctx);
    
}