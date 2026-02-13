using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;


namespace EMILtools.Extensions
{
    [CreateAssetMenu(fileName = "Mouse Callback Zones", menuName = "ScriptableObjects/Mouse/Callback Zones")]
    public class MouseCallbackZones : ScriptableObject
    { 
        public int w = 1920;
        public int h = 1080;
            
        [Serializable]
        public class CallbackZone
        {
            public Rect zone;
            [ShowInInspector, ReadOnly] bool wasInside;
            [NonSerialized] public PersistentAction callback;
            
            void EnsureInit() => callback ??= new PersistentAction();
            
            public void CheckZone(Vector2 mousePos)
            {
                EnsureInit();
                bool inside = zone.Contains(mousePos);
                if (inside && !wasInside)
                {
                    callback.Invoke();
                    
                    Debug.Log("Invoking " + callback.Count);
                    callback.PrintInvokeListNames();
                }
                wasInside = inside;
            }

            public CallbackZone(Rect zone)
            {
                this.zone = zone;
                wasInside = false;
                callback = new PersistentAction();
            }
        }

        public void CheckAllZones(Vector2 mousePos)
        {
            Debug.Log("checking zoneszx");
            if(callbackZones == null) Debug.LogError("No callback zones found, make sure to add some with AddInitialZones or AddZone");
            Debug.Log(callbackZones.Count);

            foreach (var zone in callbackZones)
            {
                Debug.Log(zone);
                zone.CheckZone(mousePos);
            }
        }
            
        [BoxGroup("References")] public List<CallbackZone> callbackZones;
    }
    
}

