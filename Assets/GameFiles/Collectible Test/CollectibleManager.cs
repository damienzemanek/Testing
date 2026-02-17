using System;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public enum Collectible { Coin, Gem, Banana, Soup }

    [Serializable]
    public struct CollectibleData
    {
        public Collectible type;
        public float amount;
        public ItemSlot slot;
    }

    public CollectibleData[] collectibles;
    public GameObject itemSlotPrefab;
    public Transform itemSlotsParent;
    
    private void Awake()
    {
        collectibles = new CollectibleData[System.Enum.GetValues(typeof(Collectible)).Length];
        Array values = Enum.GetValues(typeof(Collectible));
        for (int i = 0; i < collectibles.Length; i++)
        {
            collectibles[i].type = (Collectible)values.GetValue(i);
            collectibles[i].amount = 0f;
            GameObject newSlot = Instantiate(itemSlotPrefab, itemSlotsParent);
            collectibles[i].slot = newSlot.Get<ItemSlot>();
            collectibles[i].slot.InitSlot(collectibles[i].type);
        }
    }
    
    public void Collect(int am, Collectible type)
    {
        for (int i = 0; i < collectibles.Length; i++)
            if (collectibles[i].type == type)
            {
                collectibles[i].amount += am;
                collectibles[i].slot.UpdateSlotUI(collectibles[i].amount);
            }
    }

}
