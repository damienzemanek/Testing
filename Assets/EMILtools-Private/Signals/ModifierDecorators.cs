using System;
using EMILtools.Timers;
using UnityEngine;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Timers.TimerUtility;

namespace EMILtools.Signals
{
    
    public struct StatModTimed<T, TMod> : IStatModStrategy<T>, IStatModStrategyCustom
        where T : struct, IEquatable<T>
        where TMod : struct, IStatModStrategy<T>
    {
        public readonly CountdownTimer timer;
        public TMod modifier;

        // keyword 'in' avoids copying into the param list. Avoids an extra copy for value-types when initilly storing them
        public StatModTimed(float duration, in TMod _modifier)
        {
            timer = new  CountdownTimer(duration);
            modifier = _modifier; // Copy still occurs here (but this is okay since this is the only storage spot)
        }

        public Func<T, T> func
        {
            get => modifier.func;
            set { throw new NotImplementedException("Modifier not settable from Decorator, use ctor"); }
        }
    }
}
