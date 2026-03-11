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
    private static float soundVolume = 1.0f; 
    private static float musicVolume = 1.0f;

    public static void SetSoundVolume(float volume)
    {   
        PlayerPrefs.SetFloat("SoundVolume", volume);
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public static float GetSoundVolume()
    {
        return PlayerPrefs.GetFloat("SoundVolume", 1.0f);
    }

    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1.0f);
    }

    void Awake()
    {   
        AudioManager.musicVolume = AudioManager.GetMusicVolume();
        AudioManager.soundVolume = AudioManager.GetSoundVolume();
        if(musicSource == null) musicSource = this.GetComponent<AudioSource>();
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        musicSource.volume = 0.3f * AudioManager.musicVolume;
    }
    private void Start()
    {
        musicSource.clip = intro;
        musicSource.Play();
        musicSource.PlayScheduled(AudioSettings.dspTime + intro.length);
        AudioSource loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.volume = 0.3f * AudioManager.musicVolume;
        loopSource.clip = loop;
        loopSource.loop = true;
        loopSource.PlayScheduled(AudioSettings.dspTime + intro.length);
    }
    private void Update()
    {   
        float currVol = (float) 0.3 * AudioManager.musicVolume;
        if(musicSource.volume != currVol)
        {
            musicSource.volume = currVol;
        }
    }
    public void PlayTilePlacing()
    {
        sfxSource.PlayOneShot(tilePlacing, AudioManager.soundVolume);
    }
    public void PlayJumping()
    {
        sfxSource.PlayOneShot(jumping, AudioManager.soundVolume);
    }
    public void PlayButtonClick()
    {
        sfxSource.PlayOneShot(buttonClick, AudioManager.soundVolume);
    }
    public void PlayTrap()
    {
        sfxSource.clip = trap;
        sfxSource.loop = false;
        sfxSource.volume = AudioManager.soundVolume;
        sfxSource.PlayScheduled(AudioSettings.dspTime + 0.15f);
    }
}
