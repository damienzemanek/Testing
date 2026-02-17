using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private CollectibleManager manager;

    public CollectibleManager.Collectible collectibleType;

    public int value;
    
    public GameObject audioParent;
    public AudioSource source;
    public AudioClip clip;
    
    public string tag;

    private void Awake()
    {
        manager = FindAnyObjectByType(typeof(CollectibleManager)) as CollectibleManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    void Collect()
    {
        manager.Collect(value, collectibleType);
        audioParent.transform.parent = null;
        source.clip = clip;
        source.Play();
        Destroy(gameObject);
    }
}
