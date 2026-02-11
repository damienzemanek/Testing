using System;
using EMILtools_Private.Testing;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

[Serializable]
public class ControlledMonoFacade<TMonoFacade, TFunctionality, TConfig, TBlackboard, TInputReader>: MonoFacade<TMonoFacade, TFunctionality, TConfig, TBlackboard>
    where TMonoFacade : IFacade    
    where TConfig : Config                                    // Config does not need to be an interior because it should not have a reference to the facade, it is just data
    where TBlackboard : Blackboard,                           IFacadeCompositionElement<TMonoFacade>
    where TFunctionality : Functionalities<TMonoFacade>,      IFacadeCompositionElement<TMonoFacade>, new()
    where TInputReader : ScriptableObject, IInputReader,      IFacadeCompositionElement<TMonoFacade>
{
    [field: Title("Input")] [field: PropertyOrder(-1)]
    [field:SerializeField, Required] public TInputReader Input { get; private set; }

    
    public override void InitializeFacade()
    {
        Debug.Assert(Input != null, $"{name}: Input not assigned");
        Input.ComposeElement(this);
        base.InitializeFacade();
    }
}


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


