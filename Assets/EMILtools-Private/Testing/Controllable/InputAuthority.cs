using System;
using EMILtools.Core;
using KBCore.Refs;
using UnityEngine;

[Serializable]
public abstract class InputAuthority<TInputReader, TInputMap> : ValidatedMonoBehaviour
     where TInputReader : ScriptableObject
     where TInputMap : class, IInputMap
{
     public InterfaceRef<IControllable<TInputMap>> Controlled;
     public TInputReader Reader;

     
}
