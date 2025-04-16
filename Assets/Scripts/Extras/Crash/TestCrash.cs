using UnityEngine;
public class TestCrash : MonoBehaviour
{
    private bool isCrashed = false;
    public bool IsCrashed => isCrashed;
    void Start()
    {
        AudioListener.pause = true;
        TriggerGlitch();
        TriggerCrash();
    }
    void Update()
    {
        if (isCrashed)
        {
            while (true)
            {

            }
        }
    }
    void TriggerGlitch()
    {
        var cameraEffect = Camera.main.GetComponent<ScreenShearEffect>();
        if (cameraEffect != null)
        {
            cameraEffect.StartShearing(0.2f, 1000f);
        }
    }
    void TriggerCrash()
    {
        Debug.LogError("InfernOS is crashing intentionally...");
        isCrashed = true;
    }
}

