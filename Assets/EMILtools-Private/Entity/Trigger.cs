using System;
using EMILtools.Extensions;
using EMILtools.Signals;
using static EMILtools.Signals.ModifierStrategies;
using UnityEngine;

public class Trigger : Entity
{
    SpeedModStrategy<float> speedModifier = new SpeedModStrategy<float>(x => x * 1.5f);
    

    void OnTriggerEnter(Collider other)
    {
        print(other.tag);
        if (!other.TryGetComponent(out IStatUser stat)) return;
        
        print("give speed buff");
        stat.ModifyStatUser(speedModifier);
    }
}
