using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreNumbersAnimated : MonoBehaviour
{
    public float idleAmplitude = 1f;
    public float idleSpeed = 1f;
    public float wobbleSpeed = 1f;
    private Image image;
    private RectTransform rt;
    private Vector2 basePos;
    private float timeOffset;
    private float wobbleStrength;
    private bool ready;

    void Awake()
    {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        timeOffset = Random.Range(0f, 100f);
    }
    public void Init()
    {
        basePos = rt.anchoredPosition;
        ready = true;
    }

    void Update()
    {
        if (!ready) return;

        float idleY = Mathf.Sin(Time.time * idleSpeed + timeOffset) * idleAmplitude;

        if (wobbleStrength <= 0f)
        {
            rt.anchoredPosition = basePos + new Vector2(0f, idleY);
            return;
        }

        float t = Time.time * wobbleSpeed + timeOffset;
        rt.anchoredPosition = basePos + new Vector2(
            Mathf.Sin(t * 1.3f) * wobbleStrength,
            Mathf.Sin(t) * wobbleStrength + idleY
        );
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.9f) * wobbleStrength * 2f);

        wobbleStrength = Mathf.MoveTowards(wobbleStrength, 0f, Time.deltaTime * 6f);
    }

    public void SetSprite(Sprite sprite) {image.sprite = sprite;}

    public void Trigger(float strength)
    {
        basePos = rt.anchoredPosition;
        ready = true;
        wobbleStrength = Mathf.Clamp(strength, 1f, 12f);
        timeOffset = Random.Range(0f, 100f);
    }
}