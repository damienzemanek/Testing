using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Signals.StatTags;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Signals
{
    public interface IStatModDecorator<T, TTag>
        where T : struct
        where TTag : struct, IStatTag
    {
        public bool removable { get; set; }
        public Type tagtype => typeof(TTag);
        public Stat<T, TTag> stat { get; set; }
        public T ApplyThruDecoratorFirst(T input);
        public Action OnAdd { get; set; }
        public Action OnRemove{ get; set; }
    }
    
    public abstract class StatModDecorator<T, TTag> : IStatModDecorator<T, TTag>
        where T : struct
        where TTag : struct, IStatTag
    {
        public bool removable { get; set; }
        public ulong hash { get; set; }
        public Stat<T, TTag> stat { get; set; }
        public abstract T ApplyThruDecoratorFirst(T input);
        public Action OnAdd { get; set; } = delegate { };
        public Action OnRemove { get; set; } = delegate { };

        public StatModDecorator(ulong hash)
        {
            removable = false;
            this.hash = hash;
        }

        public void RemoveModifier()
        {
            removable = true;
            stat.RemoveModifier(hash);
        }

    }
    
    public class StatModDecTimed<T, TTag> : StatModDecorator<T, TTag>, ITimerUser
        where T : struct
        where TTag : struct, IStatTag
    {
        public CountdownTimer timer;
        public override T ApplyThruDecoratorFirst(T input) => input;
        
        public StatModDecTimed(ulong hash, CountdownTimer timer, Action[] OnDecorAddCBs = null, Action[] OnDecorRemoveCBs = null) : base(hash)
        {
            removable = false;
            this.timer = timer;
            this.InitializeTimers((timer, false));
            this.Sub(timer.OnTimerStop, RemoveModifier);
            
            OnAdd += timer.Start;
            OnRemove += this.ShutdownTimers;
            
            if(OnDecorAddCBs != null && OnDecorAddCBs.Length > 0)
                foreach(var cb in OnDecorAddCBs)
                    OnAdd += cb;
            
            if(OnDecorRemoveCBs != null && OnDecorRemoveCBs.Length > 0)
                foreach(var cb in OnDecorRemoveCBs)
                    OnRemove += cb;
        }
        

        public void ForceStop(Stat<T,TTag> stat)
        {
            removable = true;
            this.stat = stat;
            timer.OnTimerStop?.Invoke();
        }

    }
}
