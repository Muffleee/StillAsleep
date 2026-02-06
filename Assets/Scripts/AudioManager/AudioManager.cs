using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip tilePlacing;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip jumping;
    [SerializeField] private AudioClip trap;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip loop;
    void Awake()
    {
        if(musicSource == null) musicSource = this.GetComponent<AudioSource>();
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource.volume = 0.3f;
    }
    private void Start()
    {
        musicSource.clip = intro;
        musicSource.Play();
        musicSource.PlayScheduled(AudioSettings.dspTime + intro.length);
        AudioSource loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.volume = 0.3f;
        loopSource.clip = loop;
        loopSource.loop = true;
        loopSource.PlayScheduled(AudioSettings.dspTime + intro.length);
    }
    public void PlayTilePlacing()
    {
        sfxSource.PlayOneShot(tilePlacing, 1.0f);
    }
    public void PlayJumping()
    {
        sfxSource.PlayOneShot(jumping, 1.0f);
    }
    public void PlayButtonClick()
    {
        sfxSource.PlayOneShot(buttonClick, 1.0f);
    }
    public void PlayTrap()
    {
        sfxSource.clip = trap;
        sfxSource.loop = false;
        sfxSource.volume = 1.0f;
        sfxSource.PlayScheduled(AudioSettings.dspTime + 0.15f);
    }
}
