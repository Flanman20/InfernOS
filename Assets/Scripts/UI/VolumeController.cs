using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider AmbientSlider;
    public Slider FXSlider;
    public AudioSource AmbientSource;
    public AudioSource FXSource;
    // Start is called before the first frame update
    void Awake()
    {
        if (AmbientSource != null && AmbientSlider != null)
        {
            float savedAmbientVolume = PlayerPrefs.GetFloat("Ambient", 1f);
            AmbientSource.volume = savedAmbientVolume;
            AmbientSlider.value = savedAmbientVolume;
            AmbientSlider.onValueChanged.AddListener(SetAmbient);
        }

        if (FXSource != null && FXSlider != null)
        {
            float savedFXVolume = PlayerPrefs.GetFloat("FX", 1f);
            FXSource.volume = savedFXVolume;
            FXSlider.value = savedFXVolume;
            FXSlider.onValueChanged.AddListener(SetFX); // This was missing
        }
    }
    void SetAmbient(float volume)
    {
        if (AmbientSource != null)
        {
            AmbientSource.volume = volume;
            PlayerPrefs.SetFloat("Ambient", volume);
            PlayerPrefs.Save();
        }
    }
        
    void SetFX(float volume)
    {
        if (FXSource != null)
        {
            FXSource.volume = volume;
            PlayerPrefs.SetFloat("FX", volume);
            PlayerPrefs.Save();
        }
    }

}
