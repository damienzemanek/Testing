using EMILtools.Signals;
using EMILtools.Timers;
using NUnit.Framework;
using static EMILtools.Signals.ModifierStrategies;
using UnityEngine;

public class StatsSystemTests : MonoBehaviour
{
    private class TestStatUser : IStatUser
    {
        public Stat<float, SpeedModifier> speed = new Stat<float, SpeedModifier>(10f);
    }

    [Test]
    public void ModifyStatUser_ApplyingSpeedModifier_ChangesStatValue()
    {
        var user = new TestStatUser();
        var mult = 2f;
        var strat = new SpeedModifier(x => x * mult);

        user.CacheStatFields();

        user.ModifyStatUser(strat);

        Assert.AreEqual(20f, user.speed.Value, "The state value should be doubled by the modifier");
    }

    [Test]
    public void ModifyStatUser_MultipleModifiers_CalculateCorrectly()
    {
        var user = new TestStatUser();
        user.CacheStatFields();
        
        user.ModifyStatUser(new SpeedModifier(x => x + 5f));
        user.ModifyStatUser(new SpeedModifier(x => x * 2f));
        
        Assert.AreEqual(30f, user.speed.Value, "Multiple modifiers should stack in order");
    }
    
    [Test]
    public void ModifyStatUser_WithInterfaceVariable_CalculateCorrectly()
    {
        var user = new TestStatUser();
        user.CacheStatFields();
        
        IStatModStrategy strat = new SpeedModifier(x => x * 2f);
        user.ModifyStatUser(strat);
        
        Assert.AreEqual(20f, user.speed.Value, "The state value should be doubled by the IStatModStrategy interface modifier");
    }
    
    [Test]
    public void ModifyStatUser_WithInterfaceVariable_Timed_CalculateCorrectly()
    {
        var user = new TestStatUser();
        user.CacheStatFields();
        
        IStatModStrategy strat = new SpeedModifier(x => x * 2f).WithTimed(5f);
        CountdownTimer timer = (strat as ITimedModifier).timer;
        
        user.ModifyStatUser(strat);
        
        Assert.AreEqual(20f, user.speed.Value, "The state value should be doubled by the IStatModStrategy interface modifier");
        Assert.AreEqual(true, timer.isRunning);
    }
    
    
}
