using System;
using System.Collections;
using EMILtools.Core;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Start() => UpdateScore(0);

    public void UpdateScore(int score) => StartCoroutine(C_UpdateScore());
    
    IEnumerator C_UpdateScore()
    {
        yield return null;
        text.text = GameManager.Instance.score.ToString();
    }
}
