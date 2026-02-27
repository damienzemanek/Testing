using System;
using Sirenix.OdinInspector;
using UnityEngine;




/// <summary>
/// Optionally Context Dependant
/// Template Method Pattern
/// ValueType Execution Hook
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IExecuteTemplate<TContext> 
    where TContext : struct
{
    public void ExecuteTemplateCall();
    public bool Execute(TContext ctx);
}

public interface IBindable
{
    public void Bind();
    public void Unbind();
}





public abstract class MonoFunctionalityModule<TFacade, TContext> 
    where TFacade : class, IFacade<TContext>
    where TContext : struct, IModuleUsabableContext
{
    public TFacade facade { get; set; }
    public TContext context => facade.API_Context();
    [Title("$Name"), PropertyOrder(-1)]
    [ShowInInspector] public string Name => "Module: " + this.GetType().Name;
    public abstract void SetupModule();
    protected virtual void Awake() { }

    public MonoFunctionalityModule(TFacade facade) => this.facade = facade;

}