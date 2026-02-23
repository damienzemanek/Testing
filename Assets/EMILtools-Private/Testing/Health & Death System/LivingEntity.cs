using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.NumEX;
using static IDamageable;

public class LivingEntity : Entity,
    IDamageable
{
    const float RestartAnimation = ZeroF;
    const float FromBeginning = ZeroF;

    
    public float maxHealth;
    
    [ShowInInspector, ReadOnly] bool isDead = false;
    [ShowInInspector, ReadOnly] ReactiveIntercept<float> health;
    [ShowInInspector, ReadOnly] int hitLayer = 2;
    [ShowInInspector, ReadOnly] int deathLayer = 3;
    [ShowInInspector, ReadOnly] public DeathType deathStatus;

    
    public AnimHandle<DeathType> deathAnimHandle;
    public AnimHandle<DamageLocation> damageLocationAnimHandle;
    [HideInInspector] public PersistentAction<DeathType> OnDeath = new();

    
    
    void Awake()
    {
        health = new ReactiveIntercept<float>(maxHealth);
        health.Intercepts.Add(value => value < ZeroF ? ZeroF : value);
        health.Reactions.Add(CheckDie);
    }
    
    public void TakeDamage(DamageInfo info)
    {
        health.Value -= info.dmg;
    }
    
    [Button]
    public void TakeDamage(int dmg) => health.Value -= dmg;
    
    void CheckDie(float v) { if (v <= ZeroF) Die(); else LocationalDamageReaction(); }

    public void LocationalDamageReaction() 
        => damageLocationAnimHandle.PlayWeightSet(
            DamageLocation.Body,
            initialWeight: 1, 
            endWeight: ZeroF, 
            hitLayer, 
            RestartAnimation);
    
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        deathStatus = DeathType.Regular;
        OnDeath.Invoke(deathStatus);
        deathAnimHandle.PlayWeightSet(deathStatus, 1, deathLayer, FromBeginning);
    }

}