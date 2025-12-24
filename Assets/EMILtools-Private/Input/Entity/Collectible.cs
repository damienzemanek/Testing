using System;
using UnityEngine;
using static Effectability;
using DG.Tweening;


public class Collectible : Entity
{
    public EffectUser eff_spawn;
    public float enlargeTime = 1f;

    private void Start()
    {
        eff_spawn.UseEffect(destroyCancellationToken);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, enlargeTime)
            .SetEase(Ease.OutBack);
    }
    
}