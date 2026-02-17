using System;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static TwoD_InputAuthority;

public class MountZone : MonoBehaviour
{
    [Required] [SerializeField] InterfaceReference<IInputSubordinate<TwoD_InputMap, Subordinates>, MonoBehaviour> mountable;
    [ShowInInspector, ReadOnly] bool inZone;
    [ShowInInspector, ReadOnly] public bool mounted = false;

    [ReadOnly] public bool playerRequestedMount => playerTransform != null && 
                                                   playerTransform.Get<TwoD_PilotController>().Blackboard.hasRequestedMount;
    [ReadOnly] public Transform playerTransform;
    
    
    void OnTriggerStay(Collider other)
    {
        if (mounted) return;
        if (!other.CompareTag("Player")) return;
        var player = other.Get<TwoD_PilotController>();
        player.Blackboard.canMount = true;
        playerTransform = player.transform;
        inZone = true;

        if (!playerRequestedMount) return;
        
        IInputSubordinate<TwoD_InputMap, Subordinates> playerSubordinate = player;
        bool success = mountable.Value.RequestAuthorityFrom(playerSubordinate);
        
        if(!success) return;
        
        mounted = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.Get<TwoD_PilotController>().Blackboard.canMount = false;
        inZone = false;
        playerTransform = null;
    }
    
}
