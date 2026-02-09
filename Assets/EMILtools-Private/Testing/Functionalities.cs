using System;
using System.Collections.Generic;
using UnityEngine;

namespace EMILtools_Private.Testing
{
    [Serializable]
    public abstract class Functionalities
    {
        [SerializeReference] List<FunctionalityModule> modules;
        [SerializeReference, HideInInspector] List<UPDATE> _update = new();
        [SerializeReference, HideInInspector] List<FIXEDUPDATE> _fixed = new();
        [SerializeReference, HideInInspector] List<LATEUPDATE> _late = new();
    
        public Functionalities() => modules = new List<FunctionalityModule>();

        public void Init() { foreach (var t in modules)  t.Init(); }
        public void Bind() { foreach (var t in modules) t.Bind(); }
        public void Unbind() { foreach (var t in modules) t.Unbind(); }
    
    
        public void Tick(float dt) { foreach (var t in _update) t.Tick(dt); }
        public void FixedTick(float fdt) { foreach (var t in _fixed) t.FixedTick(fdt); }
        public void LateTick(float dt) { foreach (var t in _late) t.LateTick(dt); }
    
    
        public void AddModule(FunctionalityModule module)
        {
            modules.Add(module);
            Debug.Log("added module " + module.GetType().Name + " new count is " + modules.Count);
            
            if (module is UPDATE u) _update.Add(u);
            if (module is FIXEDUPDATE f) _fixed.Add(f);
            if (module is LATEUPDATE l) _late.Add(l);
        }

    }
}
