using System;
using EMILtools.Extensions;
using EMILtools.Signals;
using Sirenix.OdinInspector;
using static EMILtools.Signals.ModifierStrategies;
using UnityEngine;

public class Trigger : Entity
{
    private IStatModStrategy speedModifier = new SpeedModifier(x => x * 3f);
    

    void OnTriggerEnter(Collider other)
    {
        print(other.tag);
        if (!other.TryGetComponent(out IStatUser stat)) return;
        
        print("give speed buff");
        stat.ModifyStatUser(speedModifier);
    }
}
