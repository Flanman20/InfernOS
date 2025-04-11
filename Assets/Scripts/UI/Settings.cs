using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject settingsMenu; // UI Panel that holds the settings UI elements
    public Slider FXSlider; // Volume slider
    public TMP_Text FXText; // Text for the volume display
    public Slider ambientSlider; // Brightness slider (Example)
    public TMP_Text ambientText; // Text for brightness display

    private const int maxBars = 15; // Maximum bars for the volume/brightness display

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the settings menu as hidden
        settingsMenu.SetActive(false);

        // Load saved settings or set defaults
        FXSlider.value = PlayerPrefs.GetFloat("FX", 0.5f);
        ambientSlider.value = PlayerPrefs.GetFloat("Ambient", 0.5f);

        // Update texts on start
        UpdateVolumeText(FXSlider.value);
        UpdateBrightnessText(ambientSlider.value);

        // Add listeners to sliders
        FXSlider.onValueChanged.AddListener(UpdateVolumeText);
        ambientSlider.onValueChanged.AddListener(UpdateBrightnessText);
    }

    // Update the volume text display
    private void UpdateVolumeText(float value)
    {
        int filledBars = Mathf.RoundToInt(value * maxBars);
        string volumeBar = "[" + new string('|', filledBars) + new string(' ', maxBars - filledBars) + "]";
        FXText.text = volumeBar;

        // Save the volume setting
        PlayerPrefs.SetFloat("FX", value);
    }

    // Update the brightness text display (example)
    private void UpdateBrightnessText(float value)
    {
        int filledBars = Mathf.RoundToInt(value * maxBars);
        string brightnessBar = "[" + new string('|', filledBars) + new string(' ', maxBars - filledBars) + "]";
        ambientText.text = brightnessBar;

        // Save the brightness setting
        PlayerPrefs.SetFloat("Ambient", value);
    }

    // Toggle settings menu visibility
    public void ToggleSettingsMenu(bool isOpen)
    {
        settingsMenu.SetActive(isOpen);
    }
}