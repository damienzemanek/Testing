using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;


// Func observing some variable 
// Func needs to know when variable changes
// Func then knows the Invoke next time its get
public class LazyFunc<T>
    where T : struct
{
    T storedFuncEvaluation;
    readonly Func<T> func;
    public T InvokeLazy => storedFuncEvaluation;

    public LazyFunc(PersistentAction reactionReference, Func<T> func)
    {
        this.func = func;  
        storedFuncEvaluation = func.Invoke();
        reactionReference.Add(Evaluate);
    }
    

    
    void Evaluate() => storedFuncEvaluation = func.Invoke();
}