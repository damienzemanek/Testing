using System;
using System.Collections.Generic;
using KBCore.Refs;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IInputAuthority<TInputMap, TSubordinateEnum>
    where TSubordinateEnum : Enum
    where TInputMap : class, IInputMap, new()
{
    public TInputMap currentInputMap => subordinate != null ? subordinate.Input : null;
    public TSubordinateEnum currentSubordinate => subordinate != null ? subordinate.context.key : default;
    IInputSubordinate<TInputMap, TSubordinateEnum> subordinate { get; set; }
    
    public bool AcceptRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate)
    {
        this.subordinate = subordinate;
        ReceiveRequest(subordinate);
        return true;
    }
    protected abstract void ReceiveRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate);
    
}



