using System.Threading.Tasks;
using UnityEngine;

public static class PipelineSystem
{

    /// <summary>
    /// Frame-Agnostic Async Execution Pipeline
    /// </summary>
    public class PipelineExecutor
    {
        public async Task Execute<TContext>(Pipeline<TContext> pipeline, TContext ctx)
            where TContext : struct
        {
            for (int i = 0; i < pipeline.Size; i++)
            {
                bool hasCtx = pipeline[i].ResolveContext != null;
                bool blocked = hasCtx
                    ? pipeline[i].ResolveContext.Resolve(pipeline[i].Execute, ctx) 
                    : pipeline[i].Execute(ctx);

                if (hasCtx && pipeline[i].ResolveContext.canDelay) 
                    await pipeline[i].ResolveContext.WaitUntilResolved();
                
                if (blocked) return;
            }
        }
    }
    
    
    /// <summary>
    /// For repeated Pipeline users, cache your pipeline executor
    /// </summary>
    /// <param name="executor"></param>
    /// <param name="pipeline"></param>
    /// <param name="ctx"></param>
    /// <typeparam name="TContext"></typeparam>
    public static Task TryTo<TContext>(this PipelineExecutor executor, Pipeline<TContext> pipeline, in TContext ctx)
        where TContext : struct
        => executor.Execute(pipeline, ctx);
    
    /// <summary>
    /// For repeated Pipeline users, cache your pipeline executor
    /// Lazy-ish initialization
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="pipeline"></param>
    /// <param name="executor"></param>
    /// <typeparam name="TContext"></typeparam>
    public static Task TryTo<TContext>(this TContext ctx, Pipeline<TContext> pipeline, out PipelineExecutor executor)
        where TContext : struct
    {
        executor = new PipelineExecutor(); 
        return executor.Execute(pipeline, ctx);
    }
    
    /// <summary>
    /// Fire and Forget
    /// One off
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="pipeline"></param>
    /// <typeparam name="TContext"></typeparam>
    public static Task TryTo<TContext>(this TContext ctx, Pipeline<TContext> pipeline)
        where TContext : struct
    => new PipelineExecutor().Execute(pipeline, ctx);
    
    
}