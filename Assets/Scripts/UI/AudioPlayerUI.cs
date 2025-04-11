using UnityEngine;
using UnityEngine.UI;

public class AudioPlayerUI : MonoBehaviour
{
    public AudioSource audioSource; // Drag your AudioSource here
    public Button playPauseButton; // Drag the PlayPauseButton here
    public Slider audioSlider;     // Drag the AudioSlider here
    public Image buttonImage;      // Drag the Image component of the button here
    public Sprite playSprite;      // Assign the "Play" sprite here
    public Sprite pauseSprite;     // Assign the "Pause" sprite here
    public AudioClip audioClip;    // Assign the AudioClip here

    private bool isPlaying = false;

    void Start()
    {
        if (audioSource != null && audioClip != null)
        {
            // Assign the clip to the AudioSource
            audioSource.clip = audioClip;

            // Initialize the slider
            audioSlider.minValue = 0;
            audioSlider.maxValue = 1; // Normalized slider value
            audioSlider.value = 0;

            Debug.Log($"AudioClip Length: {audioClip.length}");
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is not set!");
            audioSlider.interactable = false;
        }

        // Set up button and slider listeners
        playPauseButton.onClick.AddListener(TogglePlayPause);
        audioSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Set initial button image
        buttonImage.sprite = playSprite;
    }

    void Update()
    {
        if (isPlaying && audioSource.clip != null)
        {
            // Update the slider value based on the current playback time
            audioSlider.value = audioSource.time / audioSource.clip.length;

            // Pause playback if the clip is finished
            if (audioSource.time >= audioSource.clip.length)
            {
                PausePlayback();
            }
        }
    }

    public void TogglePlayPause()
    {
        if (audioSource.clip == null)
        {
            Debug.LogWarning("No AudioClip assigned to the AudioSource!");
            return;
        }

        if (isPlaying)
        {
            PausePlayback();
        }
        else
        {
            PlayPlayback();
        }
    }

    private void PlayPlayback()
    {
        audioSource.Play();
        buttonImage.sprite = pauseSprite; // Change to "Pause" sprite
        isPlaying = true;
    }

    private void PausePlayback()
    {
        audioSource.Pause();
        buttonImage.sprite = playSprite; // Change to "Play" sprite
        isPlaying = false;
    }

    public void OnSliderValueChanged(float value)
    {
        // Update the AudioSource time based on the slider's normalized value
        if (audioSource.clip != null)
        {
            audioSource.time = value * audioSource.clip.length;
        }
        else
        {
            Debug.LogWarning("AudioSource has no clip assigned!");
        }
    }
}
