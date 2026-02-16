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
        public void RequestAuthority()
        {
            //Retrive the Input map stored in the Subordinate
            if (Subordinate.Value.Input == null)
            {
                Subordinate.Value.Input = Subordinate.Value.InjectInputMap();
                Subordinate.Value.InitSubordinate();
            }
            
            // Register that InputMap with the Authority
            Authority.Value.AcceptRequest(Subordinate.Value);
            Subordinate.Value.OnAuthorityReceived();
        }

        public void SetupFirstAuthority(TInputMap inputMap)
        {
            Subordinate.Value.Input = inputMap;
            Subordinate.Value.InitSubordinate();
            Authority.Value.AcceptRequest(Subordinate.Value);
            Subordinate.Value.OnAuthorityReceived();
        }
    }
    
    public IInputAuthority<TInputMap, TSubordnateEnumType> Authority => context.Authority.Value;
    
    public TInputMap Input { get; set; }
    public SubordinateContext context { get; set; }
    

    public abstract TInputMap InjectInputMap();
    /// <summary>
    /// Initialize Subordinate here, don't use Awake (Is this a design smell?)
    /// </summary>
    public abstract void InitSubordinate();
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
