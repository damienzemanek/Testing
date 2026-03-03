using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI text;
    public void UpdateGas(float gas) => text.text = "" + gas;
}