using System;
using UnityEngine;
using static Effectability;
using DG.Tweening;
using EMILtools.Core;
using EMILtools.Extensions;


public class Collectible : Entity
{
    public EffectUser eff_spawn;
    public float enlargeTime = 1f;
    public IntEventChannel scoreChannel;

    private void Start()
    {
        eff_spawn.Play(destroyCancellationToken);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, enlargeTime)
            .SetEase(Ease.OutBack);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TagIs("Player")) return;
        scoreChannel?.Invoke(1);
        Destroy(gameObject);
    }
}