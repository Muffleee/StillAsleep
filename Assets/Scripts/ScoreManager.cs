using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int pointsPerRound = 100;
    [SerializeField] private int phaseDoneBonus = 500;
    public static ScoreManager INSTANCE { get; private set; }
    private int score = 0;
    private int highScore = 0;
    private int crystalMultiplier = 10;

    private void Awake()
    {
        INSTANCE = this;
        highScore = PlayerPrefs.GetInt("Highscore", 0);
    }
    public void ScoreRoundCompleted(bool phaseDone)
    {
        score += pointsPerRound;
        if(phaseDone){score += phaseDoneBonus;}

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Highscore", highScore);
            PlayerPrefs.Save();
        }

        Debug.Log($"[Score] {score} | Highscore: {highScore}");
    }

    public void AddBonusScore(int bonus, bool multiplier, string bonusname)
    {
        if(multiplier) bonus *= crystalMultiplier;
        score += bonus;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("Highscore", highScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"{bonusname}: {bonus}");
        Debug.Log($"[Score] {score} | Highscore: {highScore}");
    }

    public int GetScore() { return score;}
    public int GetHighScore() {return highScore;}
}
