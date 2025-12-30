using System;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Signals
{
    public interface ITimedModifier : IStatModStrategyCustom
    {
        CountdownTimer timer { get; set; }
        public IStatModStrategy modifier { get; set; }

        IStatModStrategy<T> IStatModStrategyCustom.GetStrategy<T>() where T : struct
        {
            timer.Start();
            Debug.Log($"Starting timer {timer.isRunning}");
            return modifier as IStatModStrategy<T>;
        }
        
        Type IStatModStrategyCustom.ModifierType() => modifier.GetType();
    }
    
    public struct TimedStatModStrategy : ITimedModifier, IStatModStrategy
    {
        public CountdownTimer timer { get; set; }
        public IStatModStrategy modifier { get; set; }
        
        public TimedStatModStrategy(IStatModStrategy modifier, float duration)
        {
            this.modifier = modifier;
            timer = new CountdownTimer(duration);
        }
        
    }
}
