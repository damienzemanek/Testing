using UnityEngine;
using EMILtools.Timers;
using static EMILtools.SignalUtility;
using static EMILtools.Timers.TimerUtility;

public class CollectibleSpawnManager : EntitySpawnManager, ITimerUser
{
    [SerializeField] CollectibleData[] collectibleData;
    [SerializeField] Reference<float> spawnInterval;

    EntitySpawner<Collectible> spawner;
    
    public CountdownTimer spawnTimer;

    int counter = 0;
    
    protected override void Awake()
    {
        base.Awake();

        spawner = new EntitySpawner<Collectible>(  new EntityFactory<Collectible>(collectibleData),
                                                   spawnPointStrategy);

        spawnTimer = new CountdownTimer(spawnInterval);
        this.InitializeTimers((spawnTimer, false))
            .Sub(spawnTimer.OnTimerStop, HandleTimerStop);
    }
    
    void OnDestroy() => this.ShutdownTimers();
    
    void Start() => spawnTimer.Start();
    public override void Spawn() => spawner.Spawn();

    void HandleTimerStop()
    {
        if(counter++ >= spawnPoints.Length) { spawnTimer.Stop(); return;}
        Spawn();
        spawnTimer.Start();
    }
    
    
}