public readonly struct PipelineStep<TContext>
    where TContext : struct
{
    
    public readonly PipelineStepDelegate<TContext> Execute;
    public readonly IResolveContext ResolveContext;

    public PipelineStep(PipelineStepDelegate<TContext> execute, IResolveContext resolveContext)
    {
        Execute = execute;
        ResolveContext = resolveContext;
    }
    
    public PipelineStep(PipelineStepDelegate<TContext> mainMethod)
    {
        Execute = mainMethod;
        ResolveContext = null;
    }
    
}

public delegate bool PipelineStepDelegate<in TContext>(TContext context);
