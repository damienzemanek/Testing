using System;
using System.Collections.Generic;
using UnityEngine;


namespace EMILtools.Core
{
    [Serializable]
    public struct ReactiveIntercept<T>
        where T : struct
    {
        public T baseValue;
        public T Value
        { 
            get => Value;
            set
            {
                if(EqualityComparer<T>.Default.Equals(baseValue, value)) return;
                baseValue = this.ApplyAllIntercepts(value);
                Reactions?.Invoke(baseValue);
            }
        }
        public List<Func<T, T>> Intercepts;
        public Action<T> Reactions;
        
        public ReactiveIntercept(T initial, List<Func<T, T>> intercepts = null, Action<T> reactions = null)
        {
            baseValue = initial;
            Intercepts = intercepts;
            Reactions = reactions;
        }
    }
}
