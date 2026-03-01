public class Pipeline<TContext>
    where TContext : struct, IPipelineContext
{
    readonly PipelineStep<TContext>[] steps;
    public int Size => steps?.Length ?? throw new System.NullReferenceException();
    public PipelineStep<TContext> this[int index] => steps[index];
    public Pipeline(PipelineStep<TContext>[] _steps) => steps = _steps;
}