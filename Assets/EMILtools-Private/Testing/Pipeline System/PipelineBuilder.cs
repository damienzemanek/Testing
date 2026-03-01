using System;
using System.Collections.Generic;

public class PipelineBuilder<TContext> 
    where TContext : struct, IPipelineContext
{
    List<PipelineStep<TContext>> steps;
    
    public PipelineBuilder() => steps = new List<PipelineStep<TContext>>();
    
    PipelineBuilder<TContext> AddStep(StepType stepType, PipelineStepDelegate<TContext> @if, params IResolveContext[] resolveCtx)  
    {
        steps.Add(new PipelineStep<TContext>(stepType, @if, resolveCtx)); return this;
    }
    
    public PipelineBuilder<TContext> Add_ShortCircuit(PipelineStepDelegate<TContext> @if, params IResolveContext[] resolveCtx) 
        => AddStep(StepType.ShortCircuit, @if, resolveCtx);
    public PipelineBuilder<TContext> Add_Middleware(PipelineStepDelegate<TContext> method, params IResolveContext[] resolveCtx)
        => AddStep(StepType.Middleware, method, resolveCtx);
    
    public Pipeline<TContext> InjectMainMethod(PipelineStepDelegate<TContext> mainMethod) 
    {
        steps.Add(new PipelineStep<TContext>( mainMethod));
        return new Pipeline<TContext>(steps.ToArray());
    }
}