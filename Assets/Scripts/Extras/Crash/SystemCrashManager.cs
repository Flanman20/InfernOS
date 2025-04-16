using System;
using System.IO;
using UnityEngine;

public class SystemCrashManager : MonoBehaviour
{
    public SystemMessageBox MessageBox;
    public SystemCMDBox CMDBox;

    private bool isCrashHandled = false;

    void Update()
    {
        // Check if the TestCrash script has triggered the crash
        TestCrash testCrash = FindObjectOfType<TestCrash>();
        if (testCrash != null && testCrash.IsCrashed && !isCrashHandled)
        {
            isCrashHandled = true;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // Execute post-crash actions
            MessageBox.ShowMessage($"InfernOS has crashed. Please check {desktopPath}", "Error");
            CMDBox.ShowMessage($"InfernOS has crashed. Please check {desktopPath}", "Error");

            // Optional: Start recovery or shutdown sequence
            Debug.Log("Post-crash systems activated.");
        }
    }
}
