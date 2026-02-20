using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    public float maxHp;
    [ShowInInspector] float hp;
    public Animator animator;

    private static readonly int dieAnim = Animator.StringToHash("die");
    
    private void Awake()
    {
        hp = maxHp;
    }

    public void TakeDmg(float amount)
    {
        hp -= amount;
        if (hp <= 0) Die();
    }

    public void Die()
    {
        animator.Play(dieAnim, 0, normalizedTime: 0);
    }
}


public interface IDamagable
{
    void TakeDmg(float amount);
}