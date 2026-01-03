using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Signals
{
    public interface IStatModCustom<T, TMod>
        where T : struct
        where TMod : struct, IStatModStrategy<T>
    {
        public bool removable { get; set; }
        public Type linkType => typeof(TMod);
        public Stat<T, TMod> stat { get; set; }
        public T ApplyThruDecoratorFirst(T input);
        public Action OnAdd { get; set; }
        public Action OnRemove{ get; set; }
    }
    
    public abstract class ModifierDecorator<T, TMod> : IStatModCustom<T, TMod>
        where T : struct
        where TMod : struct, IStatModStrategy<T>
    {
        public bool removable { get; set; }
        public ulong hash { get; set; }
        public Stat<T, TMod> stat { get; set; }
        public abstract T ApplyThruDecoratorFirst(T input);
        public Action OnAdd { get; set; } = delegate { };
        public Action OnRemove { get; set; } = delegate { };

        public ModifierDecorator(ulong hash)
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
    
    public class TimedModifier<T, TMod> : ModifierDecorator<T, TMod>, ITimerUser
        where T : struct
        where TMod : struct, IStatModStrategy<T>
    {
        public CountdownTimer timer;
        public override T ApplyThruDecoratorFirst(T input) => input;
        
        public TimedModifier(ulong hash, CountdownTimer timer, Action add = null, Action rm = null) : base(hash)
        {
            removable = false;
            this.timer = timer;
            this.InitializeTimers((timer, false));
            this.Sub(timer.OnTimerStop, RemoveModifier);
            
            OnAdd += timer.Start;
            OnAdd += add;

            OnRemove += this.ShutdownTimers;
            OnRemove += rm;
        }
        

        public void ForceStop(Stat<T,TMod> stat)
        {
            removable = true;
            this.stat = stat;
            timer.OnTimerStop?.Invoke();
        }

    }
}
