using System;
using System.Collections.Generic;
using UnityEngine;

namespace EMILtools_Private.Testing
{
    [Serializable]
    public abstract class Functionalities<TCoreFacade> : IInteriorElement<TCoreFacade>
        where TCoreFacade : ICoreFacade
    {
        public TCoreFacade facade { get; set; }
        
        [SerializeReference] List<FunctionalityModule> modules; 
        List<UPDATE> _update = new();
        List<FIXEDUPDATE> _fixed = new();
        List<LATEUPDATE> _late = new();
    
        public Functionalities() => modules = new List<FunctionalityModule>();

        public void Awake() { foreach (var t in modules)  t.AwakeTemplateCall(); }
        public void Bind() { foreach (var t in modules) t.Bind(); }
        public void Unbind() { foreach (var t in modules) t.Unbind(); }
    
    
        public void UpdateTick(float dt) { foreach (var t in _update) t.OnUpdateTick(dt); }

        public void FixedTick(float fdt)
        {
            Debug.Log("size of fixed : " + _fixed.Count);
            foreach (var t in _fixed)
            {
                Debug.Log("t : " + t + " fdt : " + fdt);
                t.OnFixedTick(fdt);
            }
        }
        public void LateTick(float dt) { foreach (var t in _late) t.LateTick(dt); }
    
    
        public void AddModule(FunctionalityModule module)
        {
            modules.Add(module);
            Debug.Log("added module " + module.GetType().Name + " new count is " + modules.Count);
            
            if (module is UPDATE u) _update.Add(u);
            if (module is FIXEDUPDATE f) _fixed.Add(f);
            if (module is LATEUPDATE l) _late.Add(l);
        }


        public void OnAwake()
        {
            AddModulesHere();
            foreach (var t in modules)  t.AwakeTemplateCall();
        }

        public abstract void AddModulesHere();
    }
}
