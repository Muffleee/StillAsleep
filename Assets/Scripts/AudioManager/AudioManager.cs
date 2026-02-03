using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tilePlacing;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip loop;
    void Awake()
    {
        if(audioSource == null) audioSource = this.GetComponent<AudioSource>();
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        audioSource.clip = intro;
        audioSource.Play();
        audioSource.PlayScheduled(AudioSettings.dspTime + intro.length);
        AudioSource loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.clip = loop;
        loopSource.loop = true;
        loopSource.PlayScheduled(AudioSettings.dspTime + intro.length);
    }
    public void PlayTilePlacing()
    {
        audioSource.PlayOneShot(tilePlacing);
    }

    public void PlayButtonClick()
    {
        audioSource.PlayOneShot(buttonClick);
    }
}
