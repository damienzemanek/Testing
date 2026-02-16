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
        
        
        [SerializeField] 
        public InterfaceReference<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
       
        
        [SerializeField] public TSubordnateEnumType key;
        
        
        /// <summary>a
        ///  Delegation of Authority
        /// </summary>
        /// <param name="inputMap"></param>
        public void RequestAuthority(bool setup = false)
        {
            //Retrive the Input map stored in the Subordinate
            TInputMap inputMap;
            if(setup) inputMap = Subordinate.Value.InitSubordinate();
            else inputMap = Subordinate.Value.InitSubordinateTemplateCall();
            
            // Register that InputMap with the Authority
            Authority.Value.AcceptRequest(Subordinate.Value);
            Subordinate.Value.OnAuthorityReceived();
            Debug.Log("Delegated Authority Complete");
        }
    }
    
    public IInputAuthority<TInputMap, TSubordnateEnumType> Authority => context.Authority.Value;
    
    public TInputMap Input { get; set; }
    public SubordinateContext context { get; set; }

    public TInputMap InitSubordinateTemplateCall()
    {
        if (context.Subordinate.Value.Input == null)
            InitSubordinate();
        return context.Subordinate.Value.Input;
    }
    
    /// <summary>
    /// Initialize Subordinate here, don't use Awake (Is this a design smell?)
    /// </summary>
    public abstract TInputMap InitSubordinate();
    public abstract void OnAuthorityReceived();
    public abstract void OnAuthorityLost();
    
    
    /// <summary>
    /// Used when you want to handoff Authority to another subordinate and keep your registration
    /// </summary>
    /// <param name="authority"></param>
    public void ReceiveAuthority(IInputAuthority<TInputMap, TSubordnateEnumType> authority)
    {
        context.Authority.Value = authority;
        context.RequestAuthority();
    }

}
