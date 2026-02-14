using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools_Private.Testing
{
    public abstract class Functionalities<TMonoFacade> : IFacadeCompositionElement<TMonoFacade>
        where TMonoFacade : IFacade
    {
        [field: NonSerialized] public TMonoFacade facade { get; set; }
        
        [ShowInInspector] List<MonoFunctionalityModule> modules; 
        List<UPDATE> _update = new();
        List<FIXEDUPDATE> _fixed = new();
        List<LATEUPDATE> _late = new();
    
        public Functionalities() => modules = new List<MonoFunctionalityModule>();


        public void OnAwakeCompositionalElement()
        {
            AddModulesHere();
            foreach (var t in modules)  t.SetupModule();
            Debug.Log("Functionality modules succesfully setup");
        }
        public void Bind() 
        { 
            foreach (var t in modules)
            {
                t.Bind();
            } 
            Debug.Log("Functionality modules succesfully Bound");
        }
        public void Unbind() { foreach (var t in modules) t.Unbind(); }
    
    
        public void UpdateTick(float dt) { foreach (var t in _update) t.OnUpdateTick(dt); }

        public void FixedTick(float fdt) { foreach (var t in _fixed) { t.OnFixedTick(fdt); } }
        public void LateTick(float dt) { foreach (var t in _late) t.LateTick(dt); }
    
    
        public void AddModule(MonoFunctionalityModule module)
        {
            modules.Add(module);
            Debug.Log("added module " + module.GetType().Name + " new count is " + modules.Count);
            
            if (module is UPDATE u) _update.Add(u);
            if (module is FIXEDUPDATE f) _fixed.Add(f);
            if (module is LATEUPDATE l) _late.Add(l);
        }


        public abstract void AddModulesHere();
    }
}
