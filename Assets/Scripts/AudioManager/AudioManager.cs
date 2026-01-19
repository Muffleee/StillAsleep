using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip tilePlacing;
    [SerializeField] private AudioClip buttonClick;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayTilePlacing()
    {
        sfxSource.PlayOneShot(tilePlacing);
    }

    public void PlayButtonClick()
    {
        sfxSource.PlayOneShot(buttonClick);
    }
}
