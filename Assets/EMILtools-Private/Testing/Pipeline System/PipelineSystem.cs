using System.Threading.Tasks;
using UnityEngine;

public static class PipelineSystem
{

    /// <summary>
    /// Frame-Agnostic Async Execution Pipeline
    /// </summary>
    public class PipelineExecutor
    {
        public static PipelineExecutor Executor = new();
        public async Task Execute<TContext>(Pipeline<TContext> pipeline, TContext ctx)
            where TContext : struct, IPipelineContext
        {
            for (int i = 0; i < pipeline.Size; i++)
            {
                bool hasCtx = pipeline[i].ResolveContext != null;
                bool blocked = hasCtx
                    ? pipeline[i].ResolveContext.Resolve(pipeline[i].Execute, ctx) 
                    : pipeline[i].Execute(ctx);

                // To do: Fix raceing, block further exeutions if in flight
                if (hasCtx && pipeline[i].ResolveContext.canDelay) 
                    await pipeline[i].ResolveContext.WaitUntilResolved();
                
                if (blocked) return;
            }
        }
    }
    
    

    /// <summary>
    /// Slightly more performant option
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="ctx"></param>
    /// <typeparam name="TContext"></typeparam>
    /// <returns></returns>
    public static Task TryTo<TContext>(Pipeline<TContext> pipeline, in TContext ctx)
        where TContext : struct, IPipelineContext
    => PipelineExecutor.Executor.Execute(pipeline, ctx);
    

    /// <summary>
    /// Regular option
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="pipeline"></param>
    /// <typeparam name="TContext"></typeparam>
    /// <returns></returns>
    public static Task TryTo<TContext>(this TContext ctx, Pipeline<TContext> pipeline)
        where TContext : struct, IPipelineContext
    => PipelineExecutor.Executor.Execute(pipeline, ctx);
    
    
    
}