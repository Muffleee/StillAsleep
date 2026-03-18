using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFog : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image fogImage;
    [SerializeField] private float targetAlpha = 0.6f;
    [SerializeField] private float fadeSpeed = 5f;
    private float currentAlpha = 0f;
    private bool hovering = false;

    void Update()
    {
        currentAlpha = Mathf.Lerp(currentAlpha, hovering ? targetAlpha : 0f, fadeSpeed * Time.unscaledDeltaTime);
        Color color = fogImage.color;
        color.a = currentAlpha;
        fogImage.color = color;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovering = true;
    public void OnPointerExit(PointerEventData eventData) => hovering = false;
}