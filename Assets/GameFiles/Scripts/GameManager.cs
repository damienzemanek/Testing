using DesignPatterns.CreationalPatterns;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public int score;

    public void AddScore(int amount)
    {
        print($"score: {score} + {amount} = {score + amount}");
        score += amount;
    }
}
