using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

public class SystemMessageBox : MonoBehaviour
{
    public SystemCMDBox CMD;
    public Saving saving;

    // Windows API imports
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // Constants
    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint WM_CLOSE = 0x0010;
    private const int MAX_SHOW_COUNT = 10;
    private const int RECURSIVE_THRESHOLD = 4;
    private const int MESSAGE_BOX_COUNT = 30;
    private const int THREAD_SLEEP_MS = 10;

    // Static fields
    private static int showCount = 0;
    private static System.Random random = new System.Random();
    private static int screenWidth;
    private static int screenHeight;
    private static List<string> windowTitles = new List<string>();

    // Instance fields
    private volatile bool shouldQuit = false;

    private void Start()
    {
        // Save data
        saving.Save();

        // Initialize screen dimensions
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        ShowMessage("InfernOS has crashed.\n\nPlease check " + desktopPath + " for more information.", "Message");
    }

    private void Update()
    {
        // Force crash if flag is set
        if (shouldQuit)
        {
            UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
        }
    }

    /// <summary>
    /// Shows a message box and handles the cascade of multiple message boxes
    /// </summary>
    public void ShowMessage(string message, string title)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        new Thread(() => {
            // Only proceed if we haven't shown too many messages
            if (showCount < MAX_SHOW_COUNT)
            {
                // Show the message box
                MessageBoxW(IntPtr.Zero, message, title, MB_OK | MB_ICONINFORMATION);
                showCount++;

                // Recursive message box creation
                if (showCount < RECURSIVE_THRESHOLD)
                {
                    ShowMessage(message, title);
                }
                else
                {
                    // After reaching threshold, proceed with CMD box and multiple messages
                    CMD.ShowMessage("InfernOS has crashed.\n\nPlease check " + desktopPath + " for more information.", "Message");

                    // Create multiple message boxes
                    CreateMultipleMessageBoxes(title, desktopPath);

                    // Wait and close all windows
                    CloseWindowsAndCrash();
                }
            }
        }).Start();
#else
        Debug.Log("Windows message boxes only work on Windows: " + message);
#endif
    }

    /// <summary>
    /// Creates multiple message boxes with unique titles
    /// </summary>
    private void CreateMultipleMessageBoxes(string baseTitle, string desktopPath)
    {
        // Create all message boxes
        windowTitles.Clear();
        for (int i = 0; i < MESSAGE_BOX_COUNT; i++)
        {
            string uniqueTitle = $"{baseTitle} {Guid.NewGuid().ToString("N").Substring(0, 8)}";
            windowTitles.Add(uniqueTitle);

            new Thread(() => {
                MessageBoxW(IntPtr.Zero, "Please check " + desktopPath + " for more information.", uniqueTitle, MB_OK | MB_ICONINFORMATION);
            }).Start();

            Thread.Sleep(70);
        }
    }

    /// <summary>
    /// Closes all created windows and triggers application crash
    /// </summary>
    private void CloseWindowsAndCrash()
    {
        new Thread(() => {
            // Small delay to ensure windows appear
            Thread.Sleep(THREAD_SLEEP_MS);

            // Close each window by its title
            foreach (var windowTitle in windowTitles)
            {
                IntPtr hWnd = FindWindow(null, windowTitle);
                if (hWnd != IntPtr.Zero)
                {
                    PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }

            // Clean up and set flag to crash in Update
            windowTitles.Clear();
            shouldQuit = true;
        }).Start();
    }
}