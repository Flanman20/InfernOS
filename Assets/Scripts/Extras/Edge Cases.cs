using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

public class EdgeCases : MonoBehaviour
{
    public TMP_Text output;
    public bool nullOrAsk = false;
    BIOS bios;
    Progression progression;
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public void HandleEdgeCases(string input)
    {
        progression = GetComponent<Progression>();
        bios = GetComponent<BIOS>();
        if (string.IsNullOrEmpty(input))
        {
            output.text = "Please also include an <color=yellow>entry</color><br><color=green>e.g. *ask where am I?</color>";
            nullOrAsk = true;
        }

        if (input.Contains("hell") && progression.seenHell == false && !input.Contains("hello") && progression.progressionLevel == 0)
        {
            IntPtr unityWindow = GetActiveWindow(); // Move this to the very beginning

            AudioListener.volume = 0f;
            Time.timeScale = 0f;
            Thread.Sleep(2384 / 2);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c exit",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal, // Also change to Minimized
                CreateNoWindow = false
            };
            Process.Start(startInfo);
            Thread.Sleep(2384 / 2);

            SetForegroundWindow(unityWindow);
            Time.timeScale = 1f;
            AudioListener.volume = 1f;
            progression.seenHell = true;
            bios.audioManager.backgroundSource.Stop();
            UnityEngine.Debug.Log("should stop now");
            StartCoroutine(WaitSound(5f));
        }

        if (input.Contains("serverdisruption") && !progression.seenDisruption) 
        {
            UnityEngine.Debug.Log("goat shit here");
            var cameraEffect = Camera.main.GetComponent<ScreenShearEffect>();
            if (cameraEffect != null)
            {
                cameraEffect.StartShearing(0.5f, 0.13f);
                AudioListener.volume = 0f;
                
                Thread.Sleep(100);
                AudioListener.volume = 1f;
            }
            progression.seenDisruption = true;
        }
    }   
    //real shit code but idk ill just kms or smth
    IEnumerator WaitSound(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        float maxVol = bios.audioManager.backgroundSource.volume;
        bios.audioManager.backgroundSource.volume = 0;
        bios.audioManager.backgroundSource.Play();
        StartCoroutine(bios.audioManager.FadeIn(bios.audioManager.backgroundSource, 0.001f, maxVol));
        StopCoroutine(WaitSound(seconds));
    }
}
