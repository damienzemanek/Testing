using System;
using System.Threading.Tasks;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;

public interface IResolveContext
{
    public virtual void Reset() { }
    public bool Resolve<TContext>(PipelineStepDelegate<TContext> del, in TContext ctx) where TContext : struct;
    public virtual Task WaitUntilResolved() => Task.CompletedTask;
    public virtual bool canDelay => false;
}

public class Callback : IResolveContext
{
    public readonly Action Action;
    public Callback(Action _action) => Action = _action;

    public bool Resolve<TContext>(PipelineStepDelegate<TContext> del, in TContext ctx) where TContext : struct
    {
        Action?.Invoke();
        return del.Invoke(ctx);
    }
}

public class Timed : IResolveContext, ITimerUser
{
    CountdownTimer timer;
    
    public Timed(float sec)
    {
        timer = new CountdownTimer(sec);
        this.InitTimer(timer, isFixed: true);
    }
    public void Pause() => timer.Pause();
    public void Resume() => timer.Resume();
    public void Reset() => timer.Reset();
    
    public bool Resolve<TContext>(PipelineStepDelegate<TContext> del, in TContext ctx) where TContext : struct
    {
        if(!timer.isRunning && !timer.isFinished()) timer.Start();
        return !timer.isFinished(); // Blocked if not finished
    }
}

public class Wait : IResolveContext, ITimerUser
{
    CountdownTimer timer;
    public Task WaitUntilResolved() => tcs.Task;
    TaskCompletionSource<bool> tcs = new();
    public bool canDelay => true;
    
    public Wait(float sec)
    {
        timer = new CountdownTimer(sec);
        this.InitTimer(timer, isFixed: true);
        timer.OnTimerStop.Add(() => tcs.TrySetResult(true));
    }
    public void Pause() => timer.Pause();
    public void Resume() => timer.Resume();

    public void Reset()
    {
        tcs.TrySetResult(false);
        timer.Reset();
    }
    
    public bool Resolve<TContext>(PipelineStepDelegate<TContext> del, in TContext ctx) where TContext : struct
    {
        if(!timer.isRunning && !timer.isFinished()) timer.Start();
        return false;
    }
}

