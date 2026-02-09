using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.PhysEX;

[Serializable]
public class AugmentPhysEX : MonoBehaviour
{
    public JumpSettings jumpSettings;
    public GroundedSettings groundedSettings;
    public FallSettings fallSettings;

    [BoxGroup("Rigidbody")] [SerializeField] Rigidbody rb;
    [BoxGroup("ReadOnly")] [ReadOnly] public ReactiveInterceptVT<bool> isGrounded;

    void FixedUpdate()
    {
        isGrounded.Value = transform.IsGrounded(ref groundedSettings);
        rb.FallFaster(fallSettings);
    }
}
