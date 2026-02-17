using System;
using System.Collections;
using EMILtools.Extensions;
using EMILtools.Utilities;
using UnityEngine;

public class VolleyProjectileSpawner : MonoBehaviour
{
    public ProjectileSpawnManager projSpawner;
    public bool volleyFiring = false;
    public float volleyDelay = 1f;
    public float volleyLength = 2f;
    public bool volleyOnCooldown;
    public Deviatable animSpeed;


    void Start()
    {
        this.Get<Animator>().speed = animSpeed;
    }

    void Update()
    {
        if(volleyFiring && !volleyOnCooldown) projSpawner.Spawn();
        FireVolley();
    }

    void FireVolley()
    {
        if (!volleyFiring && !volleyOnCooldown)
        {
            StartCoroutine(C_FireVolley());
            volleyFiring = true;
        }
    }
    IEnumerator C_FireVolley()
    {
        yield return new WaitForSeconds(volleyLength);
        volleyFiring = false;
        volleyOnCooldown = true;
        yield return new WaitForSeconds(volleyDelay);
        volleyOnCooldown = false;

    }
}