using System;
using System.Collections.Generic;
using UnityEngine;
using static PipelineSystem;

public class Pipeline<TContext>
    where TContext : struct
{
    readonly PipelineStep<TContext>[] steps;
    public int Size => steps?.Length ?? -1;
    public PipelineStep<TContext> this[int index] => steps[index];
    public Pipeline(PipelineStep<TContext>[] _steps) => steps = _steps;
}

public class PipelineBuilder<TContext> 
    where TContext : struct
{
    PipelineStep<TContext>[] steps;
    int addedCount;

    public PipelineBuilder(int size)
    {
        steps = new PipelineStep<TContext>[size];
        addedCount = 0;
    }
    

    public PipelineBuilder<TContext> AddStep(PipelineStepDelegate<TContext> check, IResolveContext resolveCtx = null) { try 
    {
        steps[addedCount++] = new PipelineStep<TContext>(check, resolveCtx);
        return this;
    }
    catch (Exception e) { throw new IndexOutOfRangeException(e.Message); } }
    
    public Pipeline<TContext> FinalStep(PipelineStepDelegate<TContext> mainMethod) { try 
    {
        steps[addedCount++] = new PipelineStep<TContext>(mainMethod);
        return new Pipeline<TContext>(steps);
    }
    catch (Exception e) { throw new IndexOutOfRangeException(e.Message); } }
}


public readonly struct PipelineStep<TContext>
    where TContext : struct
{
    
    public readonly PipelineStepDelegate<TContext> Execute;
    public readonly IResolveContext ResolveContext;

    public PipelineStep(PipelineStepDelegate<TContext> _execute, IResolveContext _resolveContext)
    {
        Execute = _execute;
        ResolveContext = _resolveContext;
    }
    
    public PipelineStep(PipelineStepDelegate<TContext> mainMethod)
    {
        Execute = mainMethod;
        ResolveContext = null;
    }
    
}



public delegate bool PipelineStepDelegate<TContext>(TContext context);


