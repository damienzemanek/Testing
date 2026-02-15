using System;
using UnityEngine;

public interface IControllable<TInputMap> : IInitializable
    where TInputMap : class, IInputMap
{
    public TInputMap Input { get; set; }
    
}