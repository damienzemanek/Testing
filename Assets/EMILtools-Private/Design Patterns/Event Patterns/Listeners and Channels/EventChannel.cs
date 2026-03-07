using System.Collections.Generic;
using UnityEngine;

namespace EMILtools.Core {
    
    public abstract class EventChannel<T> : ScriptableObject
    {
        readonly HashSet<EventListener<T>> observers = new();

        public void Invoke(T val)
        {
            foreach (var observer in observers)
                observer.Raise(val);
        }
        
        public void Register(EventListener<T> observer) => observers.Add(observer);
        public void Unregister(EventListener<T> observer) => observers.Remove(observer);
    }

    public readonly struct Empty{}
    
    [CreateAssetMenu(fileName = "EventChannel", menuName = "EMILtools/EventChannel")]
    public class EventChannel : EventChannel<Empty> {}
}
