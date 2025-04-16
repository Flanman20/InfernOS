using UnityEngine;

public class TestCrash : MonoBehaviour
{
    private bool shouldCrash = false;
    public bool IsCrashed => shouldCrash;
    public Saving saving;
    void Start()
    {
        saving.Save();
        Time.timeScale = 0f;
        TriggerGlitch();
        Invoke("TriggerCrash", 5f);
    }

    void Update()
    {
        if (shouldCrash)
        {
            // Trigger an infinite loop to crash the game
            while (true)
            {
                // Deliberate no-op to create an infinite loop
            }
        }
    }

    void TriggerCrash()
    {
        Debug.LogError("InfernOS is crashing intentionally...");
        shouldCrash = true;
    }

    void TriggerGlitch()
    {
        var cameraEffect = Camera.main.GetComponent<ScreenShearEffect>();
        if (cameraEffect != null)
        {
            cameraEffect.StartShearing(0.2f, 1000f);
        }

        // Pause all audio globally
        AudioListener.volume = 0f;
    }
}
    