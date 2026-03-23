using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Class handling user interface widget for the player's current energy.
/// </summary>
public class EnergyUI : MonoBehaviour
{
    [SerializeField] private PlayerResources player;
    [SerializeField] private TMP_Text energyText;
    private Vector3 originalScale;
    private RectTransform rt;

    private void Start()
    {
        if(GameManager.INSTANCE != null) GameManager.INSTANCE.NoCrystals.AddListener(NoEnergy); 
        rt = energyText.GetComponent<RectTransform>();
        originalScale = rt.localScale;
    }
    /// <summary>
    /// Updates displayed energy levels each frame.
    /// </summary>
    void Update()
    {
        this.energyText.text = ""+ this.player.CurrentEnergy;
    }

    public void NoEnergy()
    {
        StartCoroutine(RedPulsating());
    }
    private IEnumerator RedPulsating()
    {
        energyText.color = Color.red;
        Vector3 targetScale = originalScale * 1.2f;

        float time = 0f;
        float duration = 0.25f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rt.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rt.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        rt.localScale = originalScale;
        energyText.color = Color.white;
    }
}

