using System;
using System.Threading.Tasks;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;


/// <summary>
/// Context for ResolveContexts, used to pass data and control the flow of the pipeline
/// </summary>
public interface IResolveContext
{
    public virtual void Reset() { }
    public bool Resolve<TContext>(in TContext ctx) where TContext : struct;
}

/// <summary>
/// Represents an interface for waitable resolve operations in the pipeline.
/// This interface enables components to await asynchronous resolution
/// while maintaining the non-blocking nature of the resolve process.
/// </summary>
public interface IResolveWaitable
{
    bool waiting { get; set; }
    public Task WaitUntilResolved(bool reenacting = false)
    {
        Debug.Log("WaitUntilResolved Called");
        if (reenacting) waiting = true;
        return cachedWaitTask;
    }
    public Task cachedWaitTask { get; set; }
}

public class Callback : IResolveContext
{
    static readonly bool ContinueResolving = true;
    public readonly Action Action;
    public Callback(Action _action) => Action = _action;

    public bool Resolve<TContext>(in TContext ctx) where TContext : struct
    {
        Debug.Log("CALLBACK");
        Action?.Invoke();
        return ContinueResolving;
    }
}

public class Timed : IResolveContext, ITimerUser
{
    bool ShortCircuitIfNotFinished => false; // Is not intended to be read as FALSe short circuit, just for readibiliy in the Resolve()
    bool ContinueResolving => true;
    public CountdownTimer Timer => timer;
    CountdownTimer timer;
    public Timed(float sec)
    {
        timer = new CountdownTimer(sec);
        this.InitTimer(timer, isFixed: true);
    }
    public bool Resolve<TContext>(in TContext ctx) where TContext : struct
    {
        if(!timer.isRunning && !timer.isFinished()) timer.Start();
        return timer.isFinished() ? ContinueResolving : ShortCircuitIfNotFinished;
    }
}

public class Wait : IResolveContext, ITimerUser, IResolveWaitable
{
    // --- static ----
    static bool ContinueResolving = true;
    
    // --- Privates ----
    CountdownTimer timer;
    TaskCompletionSource<bool> tcs;
    
    // --- API ----
    public bool waiting { get; set; } = false;
    public Task cachedWaitTask { get; set; }
    public CountdownTimer Timer => timer;
    
    // --- Ctor ----
    public Wait(float sec)
    {
        timer = new CountdownTimer(sec);
        this.InitTimer(timer, isFixed: true);
        tcs = new();
        cachedWaitTask = tcs.Task;
        timer.OnTimerStop.Add(TimerStopped);
        Debug.Log("Wait Timer Initialized");
    }
    
    void TimerStopped()
    {
        tcs.TrySetResult(true);
        waiting = false;
        Debug.Log("Wait Timer Finished");
    }
    
    public void Reset()
    {
        tcs = new();
        cachedWaitTask = tcs.Task;
        timer.Reset();
    }
    
    public bool Resolve<TContext>(in TContext ctx) where TContext : struct
    {
        if (!timer.isRunning && !timer.isFinished())
        {
            timer.Start();
            Debug.Log("Started Wait Timer");
        }
        return ContinueResolving;
    }
}

