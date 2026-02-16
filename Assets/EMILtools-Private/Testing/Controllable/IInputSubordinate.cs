using System;
using UnityEngine;

public interface IInputSubordinate<TInputMap, TSubordnateEnumType>
    where TInputMap : class, IInputMap, new()
    where TSubordnateEnumType : Enum
{
    [Serializable]
    public class SubordinateContext
    {
        [SerializeField]
        public InterfaceReference<IInputSubordinate<TInputMap, TSubordnateEnumType>, MonoBehaviour> Subordinate;
        
        
        [SerializeField] 
        public InterfaceReference<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
       
        
        [SerializeField] public TSubordnateEnumType key;

        public void RegisterWithAuthority() => Authority.Value.Register(this);
        public void RequestAuthority() => Authority.Value.RequestDelegationOfAuthority(Convert.ToInt32(key));

        public void FirstDelegationOfAuthority()
        {
            RegisterWithAuthority();
            RequestAuthority();
            Subordinate.Value.InitSubordinate();
            Debug.Log("Trying initialize Subordinate");
        }

        public IInputAuthority<TInputMap,TSubordnateEnumType>.Mapping GetMapping(int key) => Authority.Value.InputMappings[key];
    }
    
    public TInputMap Input { get; set; }
    public SubordinateContext subordinateContext { get; set; }
    public abstract void InitSubordinate();
}