using UnityEngine;
using System.Collections.Generic;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource SoundFXObject;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();     // list of active audio sources

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public AudioSource PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // spawn in gameObject
        AudioSource audioSource = Instantiate(SoundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        // get length of the clip
        float clipLength = audioSource.clip.length;

        activeAudioSources.Add(audioSource);    // add to list

        Destroy(audioSource.gameObject, clipLength);    // schedule destruction

        return audioSource;
    }

    public AudioSource PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, audioClip.Length);

        AudioClip randClip = audioClip[rand];

        return PlaySoundFXClip(randClip, spawnTransform, volume);
    }

    // stop a specific sound early
    public void StopSound(AudioSource audioSource)
    {
        if (audioSource != null && activeAudioSources.Contains(audioSource))
        {
            audioSource.Stop();
            activeAudioSources.Remove(audioSource);
            Destroy(audioSource.gameObject);
        }
    }

    public void StopAll()
    {
        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource != null)
            {
                StopSound(audioSource);
            }
        }
    }
}
