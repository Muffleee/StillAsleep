using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CountdownCond : IMapCondition
{
    private PlayerMovement playerMovement;
    private TMP_Text countdownText;
    private Coroutine countdown;
    private bool isActive = false;
    private float currentCountdown = 0;
    private float currentDuration = -1;
    private readonly float[] durations = {10, 9, 8, 7, 6, 5, 4, 3.5f, 3, 2.75f, 2.5f, 2.25f, 2, 1.75f, 1.5f, 1.25f, 1};

    public int Difficulty()
    {
        return 5;
    }

    public void Initiate(int level)
    {
        countdownText = GameManager.INSTANCE.GetPrefabLibrary().countdownText;

        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(ResetCountdown);

        currentDuration = durations[Math.Min(level, durations.Length - 1)];
        if (currentDuration <= 0) return;

        currentCountdown = currentDuration;
        countdownText.text = currentCountdown.ToString();

        countdownText.GameObject().SetActive(true);

        isActive = true;
        countdown = GameManager.INSTANCE.StartCoroutine(CountdownCoroutine());
    }

    public void Deactivate()
    {
        GameManager.INSTANCE.StopCoroutine(countdown);
        isActive = false;

        countdownText.text = "";
        currentCountdown = 0;

        countdownText.GameObject().SetActive(false);
    }

    void ResetCountdown(Vector2Int lastPos, Vector2Int newPos, WallPos direction, long count)
    {
        currentCountdown = currentDuration;
    }

    private IEnumerator CountdownCoroutine()
    {
        float deltaTimeSeconds = .1f;
        while (currentCountdown > 0)
        {
            currentCountdown -= deltaTimeSeconds;
            countdownText.text = currentCountdown.ToString("F1");
            yield return new WaitForSeconds(deltaTimeSeconds);
        }

        GameManager.INSTANCE.LoseGame("Time ran out!");
        Deactivate();
        yield break;
    }
}