using TMPro;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;

    public void InitSlot(CollectibleManager.Collectible type)
    {
        nameText.text = "" + type;
        countText.text = "" + 0;
    }
    
    public void UpdateSlotUI(float am)
    {
        countText.text = "" + am;
    }
}
