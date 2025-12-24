using System;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.SignalUtility;
using static EMILtools.Extensions.NumEX;

namespace EMILtools.Extensions
{
    public static class PhysEX
    {
        [Serializable]
        public struct MoveSettings
        {
            public ReferenceModifiable<float> speed;
            public ReferenceModifiable<float> maxVel;

            public MoveSettings(ReferenceModifiable<float> speed, ReferenceModifiable<float> maxVel)
            {
                this.speed = speed;
                this.maxVel = maxVel;
            }
        }
        [Serializable]
        public struct GroundedSettings
        {
            public Transform feetPoint;
            public float checkDist;
            public LayerMask mask;
        }

        [Serializable]
        public struct FallSettings
        {
            public ForceMode forceMode;
            public Reference<float> mult;
            public Vector3 dir;
        }

        [Serializable]
        public struct JumpSettings
        {
            public ForceMode forceMode;
            public Vector3 direction;
            public Reference<float> mult;
            public bool complexJump;
            public Reference<float> inputMaxDuration;
            public Reference<float> cooldown;
            [ShowInInspector, InlineProperty, ShowIf("complexJump")]
            public AnimationCurve forceCurve;
        }

        [Serializable]
        public struct Explode
        {
            public Rigidbody[] rbs;
            public float strength;
            public float radius;
            public bool individualOrigin;
            bool NotIndividualOrigin => !individualOrigin;
            [ShowIf("individualOrigin")] public Transform origin;
            [ShowIf("NotIndividualOrigin")] public Transform[] origins;

            public ForceMode forceMode;
        }

        public static void Blast(this Explode explode)
        {
            Transform origin = null;

            for (int i = 0; i < explode.rbs.Length; i++)
            {
                Rigidbody rb = explode.rbs[i];
                if (explode.individualOrigin) origin = explode.origin;
                else origin = explode.origins.Rand();

                rb.AddExplosionForce(explode.strength, origin.position, explode.radius);
            }
            origin.Log("Exploding");
            
        }

        static void GroundDefaultCheck(this Transform t, ref GroundedSettings ground)
        {
            if (!ground.feetPoint)
            {
                var newFeetPoint = new GameObject("Feet Point Auto-Generated");
                newFeetPoint.transform.parent = t;
                newFeetPoint.transform.localPosition = t.position.With(y: t.position.y + 0.02f);
                ground.feetPoint = newFeetPoint.transform;
            }

            if (ground.checkDist == 0) ground.checkDist = 0.08f;
        }

        public static bool IsGrounded(this Transform transform, ref GroundedSettings ground)
        {
            transform.GroundDefaultCheck(ref ground);

            bool isGrounded = Physics.Raycast(ground.feetPoint.position,
                                        -transform.up,
                                        out RaycastHit hit,
                                        ground.checkDist,
                                        ground.mask);

            return isGrounded;
        }

        public static void FallFaster(this Rigidbody rb, FallSettings fall)
        {
            rb.AddForce(fall.dir * fall.mult.Value, fall.forceMode);
        }

        public static void Jump(this Rigidbody rb, JumpSettings jump, float prog)
        {
            //rb.Log($"jumping, prog: {prog}, isComplex {jump.complexJump}");

            Vector3 dir = Vector3.zero;
            float mult = ZEROF;
            prog = Mathf.Clamp01(prog);

            if (!jump.complexJump) mult = jump.mult.Value;
            else mult = jump.mult.Value * jump.forceCurve.Evaluate(prog);
           
            dir += jump.direction * mult;
            rb.AddForce(dir, jump.forceMode);
        }


        public static void InputDirectionalMove(this Rigidbody rb, Vector2 moveInput, MoveSettings move)
        {
            if (moveInput == Vector2.zero) return;

            float speedMult = move.speed;

            // if diagnally inputting
            if (Mathf.Abs(moveInput.x) > 0.5f && Mathf.Abs(moveInput.y) > 0.5f) 
                speedMult = speedMult * 0.7071f;

            if (moveInput.x != 0)
            {
                if (moveInput.x > 0.5)
                    rb.AddForce(rb.transform.right * (speedMult * 100), ForceMode.Force);
                if (moveInput.x < -0.5)
                    rb.AddForce(-rb.transform.right * (speedMult * 100), ForceMode.Force);
            }

            if (moveInput.y != 0)
            {
                if (moveInput.y > 0.5)
                    rb.AddForce(rb.transform.forward * (speedMult * 100), ForceMode.Force);
                if (moveInput.y < -0.5)
                    rb.AddForce(-rb.transform.forward * (speedMult * 100), ForceMode.Force);
            }
        }

    }
}
