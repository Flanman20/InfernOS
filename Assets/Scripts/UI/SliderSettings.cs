using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSettings : MonoBehaviour
{
    public Slider slider; // Attach your slider here in the inspector
    public TMP_Text sliderText;    // Attach your text UI element here in the inspector

    private const int maxBars = 15; // Maximum bars for the volume display

    void Start()
    {
        // Initialize the slider and text
        slider.onValueChanged.AddListener(UpdateVolumeBar);
        UpdateVolumeBar(slider.value);
    }

    public void UpdateVolumeBar(float value)
    {
        int filledBars = Mathf.RoundToInt(value * maxBars);
        string sliderBar = "[" + new string('|', filledBars) + new string(' ', maxBars - filledBars) + "]";
        sliderText.text = sliderBar;
    }
}