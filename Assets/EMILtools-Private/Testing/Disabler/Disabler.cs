using System;
using UnityEngine;

namespace EMILtools_Private.Testing.Disabler
{
    [Serializable]
    public struct Enablable<T>
        where T: class
    {
        public T targ;
        public Enablable(T targ) => this.targ = targ;
        public bool GetEnable()
        {
            switch (targ)
            {
                case Behaviour b: return b.enabled;
                case Collider c: return c.enabled;
                case Renderer r: return r.enabled;
                case GameObject go: return go.activeSelf;
                default: return false;
            }
        }
        public void SetEnable(bool val)
        {
            switch (targ)
            {
                case Behaviour b: b.enabled = val; break;
                case Collider c: c.enabled = val; break;
                case Renderer r: r.enabled = val; break;
                case GameObject go: go.SetActive(val); break;
            }
        }
    }
}
