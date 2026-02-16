using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IInputSubordinate<TInputMap, TSubordnateEnumType>
    where TInputMap : class, IInputMap, new()
    where TSubordnateEnumType : Enum
{
    /// <summary>
    /// Used to store a subordinate's (1) Authority and (2) Current Delegate Enum State
    /// </summary>
    [Serializable]
    public class SubordinateContext
    {
        [SerializeField, Required]
        public InterfaceReference<IInputSubordinate<TInputMap, TSubordnateEnumType>, MonoBehaviour> Subordinate;
        [SerializeField, ReadOnly] 
        public InterfaceReference<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
        [SerializeField] public TSubordnateEnumType key;
    }
    
    public IInputAuthority<TInputMap, TSubordnateEnumType> Authority => context.Authority.Value;
    
    public TInputMap Input { get; set; }
    public SubordinateContext context { get; set; }
    public abstract TInputMap InjectInputMap();
    public abstract void InitSubordinate();
    public abstract void OnAuthorityReceived();
    public abstract void OnAuthorityLost();
    
    
    /// <summary>a
    ///  Delegation of Authority
    /// </summary>
    /// <param name="inputMap"></param>
    bool SendRequest()
    {
        //Retrive the Input map stored in the Subordinate
        if (context.Subordinate.Value.Input == null)
        {
            context.Subordinate.Value.Input = context.Subordinate.Value.InjectInputMap();
            context.Subordinate.Value.InitSubordinate();
        }
            
        // Register that InputMap with the Authority
        bool accepted = context.Authority.Value.ConsiderRequest(context.Subordinate.Value);
        if (accepted) context.Subordinate.Value.OnAuthorityReceived();
        return accepted;
    }
    
    public void SetupFirstAuthority(TInputMap inputMap, IInputAuthority<TInputMap, TSubordnateEnumType> authority)
    {
        context.Authority.Value = authority;
        context.Subordinate.Value.Input = inputMap;
        context.Subordinate.Value.InitSubordinate();
        context.Authority.Value.ConsiderRequest(context.Subordinate.Value);
        context.Subordinate.Value.OnAuthorityReceived();
    }
    
    public bool RequestAuthorityFrom(IInputSubordinate<TInputMap, TSubordnateEnumType> former)
    {
        IInputAuthority<TInputMap, TSubordnateEnumType> formerAuthority = former.Authority;
        context.Authority.Value = former.Authority;
        bool successful = SendRequest();
        if(successful) former.OnAuthorityLost();
        else context.Authority.Value = formerAuthority;
        return successful;
    }

}
