using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip entryFoundSound;
    public AudioClip errorSound;
    public AudioClip errorSound2;

    public AudioSource audioSource; // FX Audio Source
    public AudioSource backgroundSource; // Background/Ambient Audio Source

    private Progression progression;
    private float ambientVolume = 1f; // Default ambient volume, set by player

    private void Start()
    {
        progression = GetComponent<Progression>();
    }

    public void HandleAudio(Entry entry)
    {
        if (entry.progressionLevel > progression.progressionLevel && entry.altOutput == string.Empty)
        {
            PlaySound(errorSound);
        }
        else
        {
            PlaySound(entryFoundSound);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            float fxVolume = audioSource.volume; // Get the current FX AudioSource volume
            Debug.Log($"fxvolume:{fxVolume}");
            audioSource.PlayOneShot(clip, fxVolume); // Use the volume as a multiplier
        }
        else
        {
            Debug.LogError("AudioClip is not assigned for this action!");
        }
    }

    public IEnumerator FadeIn(AudioSource source, float speed)
    {
        float targetVolume = ambientVolume; // Fade up to player-set ambient volume
        source.volume = 0f; // Ensure it starts from zero

        while (source.volume < targetVolume)
        {
            source.volume = Mathf.Min(source.volume + speed, targetVolume);
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Call this to update the ambient volume based on player settings
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = volume;
        backgroundSource.volume = volume; // Apply immediately if needed
    }
}
