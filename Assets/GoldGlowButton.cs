using UnityEngine;
using UnityEngine.UI;

public class GoldGlowButton : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 6f;
    public Vector2 offset;
    public Vector2 size = new Vector2(360f, 300f);

    private Image glow;
    private float timer;
    private int frame;
    private int direction = 1;
    private bool on;

    void Start()
    {
        var toggle = GetComponent<Toggle>();
        toggle.graphic = null;

        var go = new GameObject("Glow");
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        glow = go.AddComponent<Image>();
        glow.raycastTarget = false;
        glow.sprite = frames[0];

        var r = glow.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = Vector2.one * 0.5f;
        r.anchoredPosition = offset;
        r.sizeDelta = size;

        toggle.onValueChanged.AddListener(isOn => on = isOn);
    }

    void Update()
    {
        if (!on || frames.Length <= 1) return;
        timer += Time.unscaledDeltaTime;
        if (timer < 1f / fps) return;
        timer = 0;

        frame += direction;
        if (frame >= frames.Length - 1 || frame <= 0)
            direction *= -1;

        glow.sprite = frames[frame];
    }
}