using UnityEngine;

public class Pipeline<TContext> : MonoBehaviour
    where TContext : struct
{
    readonly PipelineStep<TContext>[] steps;

}

public struct PipelineStep<TContext>
{
    public PipelineDelegate<TContext> Execute;
}

public interface IResolveContext
{
    
}





public delegate void PipelineDelegate<TContext>(TContext context);