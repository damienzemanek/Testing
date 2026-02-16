using System;
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
        [SerializeField]
        public InterfaceReference<IInputSubordinate<TInputMap, TSubordnateEnumType>, MonoBehaviour> Subordinate;
        
        
        [SerializeField] 
        public InterfaceReference<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
       
        
        [SerializeField] public TSubordnateEnumType key;

        /// <summary>
        /// Register with either a Param'd InputMap or a Completedly new InputMap
        /// </summary>
        /// <param name="inputMap"></param>
        public void RegisterWithAuthority(TInputMap inputMap = null)
        {
            if(inputMap == null) Authority.Value.RegisterSubordinateInstance(this, null);
            else Authority.Value.RegisterSubordinateInstance(this, () => inputMap);
        }
        
        /// <summary>
        /// Request delegation of authority from the Authority
        /// Available Later Implementation: Intercept and check if authority is valid
        /// </summary>
        public void RequestAuthority() => Authority.Value.RequestDelegationOfAuthority(Convert.ToInt32(key));

        /// <summary>
        /// First Delegation of Authority, Optional custom InputMap
        /// </summary>
        /// <param name="inputMap"></param>
        public void FirstDelegationOfAuthority(TInputMap inputMap = null)
        {
            RegisterWithAuthority(inputMap);
            RequestAuthority();
            Subordinate.Value.InitSubordinate();
            Debug.Log("Trying initialize Subordinate");
        }

        /// <summary>
        /// Gets the mapping
        /// (Loud Failure)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IInputAuthority<TInputMap,TSubordnateEnumType>.Mapping GetMapping(int key) => Authority.Value.InputMappings[key];
    }
    
    public TInputMap Input { get; set; }
    public SubordinateContext subordinateContext { get; set; }
    
    /// <summary>
    /// Initialize Subordinate here, don't use Awake (Is this a design smell?)
    /// </summary>
    public abstract void InitSubordinate();
}