using System;
using System.Collections.Generic;

public class Pipeline<TContext>
    where TContext : struct, IPipelineContext
{
    readonly PipelineStep<TContext>[] steps;
    public int Size => steps?.Length ?? -1;
    public PipelineStep<TContext> this[int index] => steps[index];
    public Pipeline(PipelineStep<TContext>[] _steps) => steps = _steps;
}

public class PipelineBuilder<TContext> 
    where TContext : struct, IPipelineContext
{
    PipelineStep<TContext>[] steps;
    List<PipelineStep<TContext>> stepsList;
    bool usingList = false;
    int addedCount;

    public PipelineBuilder(int size)
    {
        steps = new PipelineStep<TContext>[size];
        usingList = false;
        addedCount = 0;
    }
    
    public PipelineBuilder()
    {
        stepsList = new List<PipelineStep<TContext>>();
        usingList = true;
    }


    public PipelineBuilder<TContext> ExitIf(PipelineStepDelegate<TContext> check, IResolveContext resolveCtx = null) { try 
    {
        
        if (usingList)
            stepsList.Add(new PipelineStep<TContext>(check, resolveCtx));
        else
            steps[addedCount++] = new PipelineStep<TContext>(check, resolveCtx);
        
        return this;
    }
    catch (Exception e) { throw new IndexOutOfRangeException(e.Message); } }
    
    public Pipeline<TContext> InjectMainMethod(PipelineStepDelegate<TContext> mainMethod) { try 
    {
        if (usingList)
            stepsList.Add(new PipelineStep<TContext>(mainMethod));
        else
            steps[addedCount++] = new PipelineStep<TContext>(mainMethod);
        
        return new Pipeline<TContext>(usingList ? stepsList.ToArray() : steps);
    }
    catch (Exception e) { throw new IndexOutOfRangeException(e.Message); } }
    
    

}


