using System;
using UnityEngine;

public interface IInputSubordinate<TInputMap, TSubordnateEnumType> : IInitializable
    where TInputMap : class, IInputMap, new()
    where TSubordnateEnumType : Enum
{
    [Serializable]
    public class SubordinateContext
    {
        [SerializeField]
        public InterfaceReference<IInputSubordinate<TInputMap, TSubordnateEnumType>, MonoBehaviour> Subordinate;
        
        
        [field: SerializeField] 
        public InterfaceReference<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
       
        
        [SerializeField] public TSubordnateEnumType key;

        public void RegisterWithAuthority()
        {
            Debug.Log("Registering with authority");
            Authority.Value.Register(this);
        }

        public void RequestAuthority()
        {
            Debug.Log(Authority);
            Debug.Log(Authority.Value);
            Authority.Value.RequestDelegationOfAuthority(Convert.ToInt32(key));
        }

        public IInputAuthority<TInputMap,TSubordnateEnumType>.Mapping GetMapping(int key) => Authority.Value.InputMappings[key];


    }
    
    public TInputMap Input { get; set; }
    public SubordinateContext subordinateContext { get; set; }
}