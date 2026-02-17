using UnityEngine;

public interface IFacadeCompositionElement<TMonoFacade>
    where TMonoFacade : IFacade
{
    public TMonoFacade facade { get; set; }

    public void ComposeElement(IFacade f)
    {
        if (f is TMonoFacade t) facade = t;
        else Debug.LogError($"Facade of type {f.GetType()} is not of type {typeof(TMonoFacade)}" );
        
        Debug.Log("awaking " + GetType().Name + " from facade " + facade.GetType().Name);
        OnAwakeCompositionalElement();
    }
    
    public virtual void OnAwakeCompositionalElement() { }
}