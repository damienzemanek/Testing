using System;
using EMILtools.Extensions;
using EMILtools.Timers;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Timers.TimerUtility;


[Serializable]
public class ProjectileSpawnManager : EntitySpawnManager, ITimerUser
{
    public ProjectileData[] data = new ProjectileData[1];
    public Ref<float> fireInterval = 1f;
    EntitySpawner<Projectile> projSpawner;
    [SerializeField] CountdownTimer fireTimer;

    protected override void InitializationImplementation()
    {
        projSpawner = new EntitySpawner<Projectile>(
            new EntityFactory<Projectile>(data),
            spawnPointStrategy);

        fireTimer = new CountdownTimer(fireInterval);
        this.InitializeTimers((fireTimer, true));
        
        Debug.Log("Projectile Spawner Initialization Complete");
    }

    protected override void SpawnImplementation()
    {
        if (fireTimer.isRunning) return;
        fireTimer.Start();
        Projectile proj = projSpawner.Spawn().Initalize(data[0]);
        proj.rb.AddForce(proj.transform.forward * data[0].forceScalar, data[0].forceMode);
    }
}
