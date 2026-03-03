using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Sprite[] allSprites;
    public GameObject digitPrefab;
    public float digitWidth = 48f;
    public float digitSpacing = 4f;
    public float labelYOffset = 60f;
    public float labelSpacing = 4f;
    public float pointsPerWobbleUnit = 50f;
    public float deltaDigitWidth = 32f;
    public float deltaDigitSpacing = 2f;
    public float deltaLifetime = 1.5f;
    public float deltaFloatSpeed = 30f;
    public Vector2 deltaOffset = new Vector2(20f, -20f);
    private readonly List<ScoreNumbersAnimated> digits  = new();
    private readonly List<ScoreNumbersAnimated> letters = new();
    private int displayedScore;
    private Coroutine countCoroutine;
    private Sprite DigitSprite(int n)  => allSprites[n];
    private Sprite LetterSprite(int i) => allSprites[10 + i];

    void Start()
    {
        StartCoroutine(BuildLabel());
        RenderScore(0);
    }

    IEnumerator BuildLabel()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        float totalWidth = 5 * digitWidth + 4 * labelSpacing;

        for (int i = 0; i < 5; i++)
        {
            var go   = Instantiate(digitPrefab, transform);
            var rt   = go.GetComponent<RectTransform>();
            var anim = go.GetComponent<ScoreNumbersAnimated>();

            float x = -(totalWidth / 2f) + i * (digitWidth + labelSpacing) + digitWidth / 2f;
            rt.anchoredPosition = new Vector2(x, labelYOffset);
            rt.sizeDelta        = new Vector2(digitWidth, digitWidth);

            anim.SetSprite(LetterSprite(i));
            anim.Init();
            letters.Add(anim);
        }
    }

    public void UpdateScore(int newScore, int pointsAdded)
    {
        if (pointsAdded != 0)
            StartCoroutine(ShowDelta(pointsAdded));

        float wobble = Mathf.Clamp(Mathf.Abs(pointsAdded) / pointsPerWobbleUnit, 1f, 12f);
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(CountUp(displayedScore, newScore, wobble));
    }

    IEnumerator ShowDelta(int delta) // Zeigt das unten rechts mit plus minus
    {
        bool positive = delta > 0;
        string s = Mathf.Abs(delta).ToString();
        Color col = positive ? Color.green : Color.red;

        var container = new GameObject("DeltaPopup", typeof(RectTransform));
        container.transform.SetParent(transform, false);

        var containerRt = container.GetComponent<RectTransform>();
        containerRt.anchorMin = containerRt.anchorMax = containerRt.pivot = new Vector2(0f, 1f);
        containerRt.localRotation = Quaternion.Euler(0f, 0f, -25f);

        float totalW = s.Length * deltaDigitWidth + (s.Length - 1) * deltaDigitSpacing;
        var imgList = new List<Image>();

        for (int i = 0; i < s.Length; i++)
        {
            var go  = new GameObject("d" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container.transform, false);

            var rt  = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();

            float x = -(totalW / 2f) + i * (deltaDigitWidth + deltaDigitSpacing) + deltaDigitWidth / 2f;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta        = new Vector2(deltaDigitWidth, deltaDigitWidth);

            img.sprite = DigitSprite(s[i] - '0');
            img.color  = col;
            imgList.Add(img);
        }

        containerRt.anchoredPosition = GetScoreBottomRight() + deltaOffset;

        float elapsed = 0f;
        Vector2 startPos = containerRt.anchoredPosition;

        while (elapsed < deltaLifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / deltaLifetime);

            containerRt.anchoredPosition = startPos + new Vector2(0f, deltaFloatSpeed * elapsed);

            foreach (var img in imgList)
                img.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }

        Destroy(container);
    }

    Vector2 GetScoreBottomRight()
    {
        if (digits.Count == 0) return Vector2.zero;
        float totalW = digits.Count * digitWidth + (digits.Count - 1) * digitSpacing;
        return new Vector2(totalW / 2f, -digitWidth / 2f);
    }
    IEnumerator CountUp(int from, int to, float maxWobble)
    {
        float duration = Mathf.Clamp(Mathf.Abs(to - from) * 0.005f, 0.3f, 1.5f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            RenderScore(Mathf.RoundToInt(Mathf.Lerp(from, to, t)), maxWobble * t);
            yield return null;
        }
        RenderScore(to, maxWobble);
    }

    void RenderScore(int score, float wobble = 0f)
    {
        displayedScore = score;
        string s = Mathf.Max(score, 0).ToString();

        while (digits.Count < s.Length) digits.Add(CreateAnimated());
        while (digits.Count > s.Length)
        {
            Destroy(digits[^1].gameObject);
            digits.RemoveAt(digits.Count - 1);
        }

        float totalWidth = s.Length * digitWidth + (s.Length - 1) * digitSpacing;

        for (int i = 0; i < s.Length; i++)
        {
            var d  = digits[i];
            var rt = d.GetComponent<RectTransform>();

            float x = -(totalWidth / 2f) + i * (digitWidth + digitSpacing) + digitWidth / 2f;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta        = new Vector2(digitWidth, digitWidth);

            d.SetSprite(DigitSprite(s[i] - '0'));
            d.Init();
            if (wobble > 0f) d.Trigger(wobble);
        }
    }

    ScoreNumbersAnimated CreateAnimated()
    {
        return Instantiate(digitPrefab, transform).GetComponent<ScoreNumbersAnimated>();
    }
}