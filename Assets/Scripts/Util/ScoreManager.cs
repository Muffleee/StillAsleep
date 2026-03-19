using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;
    public static ScoreManager INSTANCE { get; private set; }
    private int score = 0;
    private int highScore = 0;
    [SerializeField] private PlayerResources pr;
    private void Awake()
    {
        INSTANCE = this;
        highScore = PlayerPrefs.GetInt("Highscore", 0);
    }
    public void AddScore(int bonus, bool multiplier, string bonusname)
    {
        int crystalbonus = pr.getEnergy();
        if(multiplier && crystalbonus > 0) bonus *= crystalbonus;
        score += bonus;
        if (score < 0) score = 0;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Highscore", highScore);
            PlayerPrefs.Save();
        }
        scoreDisplay.UpdateScore(score, bonus);
    }

    public int GetScore() { return score;}
    public int GetHighScore() {return highScore;}
}
