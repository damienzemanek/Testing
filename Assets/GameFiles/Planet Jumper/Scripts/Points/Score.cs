using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI text;
    public void UpdateScore(int score) => text.text = "" + score;
}