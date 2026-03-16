using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonFog : MonoBehaviour
{
    [Header("Fog Particles")]
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private float fadeSpeed = 3f;

    private CanvasGroup fogGroup;

    void Start()
    {
        if (fogParticles != null)
        {
            fogParticles.Stop();
            fogGroup = fogParticles.GetComponent<CanvasGroup>();
            if (fogGroup == null)
                fogGroup = fogParticles.gameObject.AddComponent<CanvasGroup>();
            fogGroup.alpha = 0f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (fogParticles != null)
        {
            fogParticles.Play();
            StopAllCoroutines();
            StartCoroutine(FadeFog(1f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeFogOut());
    }

    IEnumerator FadeFog(float target)
    {
        while (fogGroup.alpha < target)
        {
            fogGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
    }

    IEnumerator FadeFogOut()
    {
        while (fogGroup.alpha > 0f)
        {
            fogGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        fogParticles.Stop();
    }
}
