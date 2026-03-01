using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a step in a pipeline, defining the type of step, its execution logic, and its associated resolution contexts.
/// - ReadOnly, to be created ONCE in initialization via PipelineBuilder
/// </summary>
/// <typeparam name="TContext">
/// The type of context used in the pipeline. Must be a struct.
/// </typeparam>
public readonly struct PipelineStep<TContext>
    where TContext : struct
{
    static readonly bool ResolveSuccessfull = true;

    public readonly PipelineStepDelegate<TContext> Execute;

    public readonly IResolveContext[] resolveContexts;
    public readonly StepType StepType;

    public PipelineStep(StepType stepType, PipelineStepDelegate<TContext> execute, IResolveContext[] resolveContexts)
    {
        this.Execute = execute;
        this.resolveContexts = resolveContexts ?? Array.Empty<IResolveContext>();
        StepType = stepType;
    }
    
    public PipelineStep(PipelineStepDelegate<TContext> mainMethod)
    {
        Execute = mainMethod;
        resolveContexts = Array.Empty<IResolveContext>();
        StepType = StepType.MainMethod;
    }
    
    public async Task<bool> ResolveContext(TContext context)
    {
        foreach (var resolve in resolveContexts)
        {
            if (resolve.Resolve(context) != ResolveSuccessfull) return !ResolveSuccessfull;
            if (resolve is IResolveWaitable canWait && !canWait.waiting) await canWait.WaitUntilResolved();
        }
        return ResolveSuccessfull;
    }
    
}

public enum StepType
{
    /// <summary>
    /// Default type, will execute and allow the pipeline to continue if not blocked by the ResolveContext
    /// </summary>
    Middleware, 
    ShortCircuit, 
    MainMethod
}





public delegate bool PipelineStepDelegate<in TContext>(TContext context);



