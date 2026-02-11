using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;


// Func observing some variable 
// Func needs to know when variable changes
// Func then knows the Invoke next time its get
public class LazyFuncLite<T>
    where T : struct
{
    T storedFuncEvaluation;
    readonly Func<T> func;
    public T InvokeLazy => storedFuncEvaluation;

    public LazyFuncLite(PersistentAction observedOnChanged, Func<T> func)
    {
        this.func = func;  
        storedFuncEvaluation = func.Invoke();
        observedOnChanged.Add(Evaluate);
    }
    
    public void Dispose(PersistentAction observedOnChanged) => observedOnChanged.Remove(Evaluate);
    
    public static implicit operator T(LazyFuncLite<T> lazyFuncLite) => lazyFuncLite.storedFuncEvaluation;
    

    
    void Evaluate() => storedFuncEvaluation = func.Invoke();
}


// Func observing some variable 
// Func needs to know when variable changes
// Func then knows the Invoke next time its get
public class LazyFunc<T>
    where T : struct
{
    T storedFuncEvaluation;
    readonly Func<T> func;
    public T InvokeLazy => storedFuncEvaluation;
    [NonSerialized] PersistentAction observedOnChanged;

    public LazyFunc(PersistentAction observedOnChanged, Func<T> func)
    {
        this.func = func;  
        storedFuncEvaluation = func.Invoke();
        this.observedOnChanged = observedOnChanged;
        this.observedOnChanged.Add(Evaluate);
    }
    
    public void Dispose() => observedOnChanged.Remove(Evaluate);
    
    public static implicit operator T(LazyFunc<T> lazyFuncLite) => lazyFuncLite.storedFuncEvaluation;
    
    
    void Evaluate() => storedFuncEvaluation = func.Invoke();
}