using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

public class SystemMessageBox : MonoBehaviour
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint WM_CLOSE = 0x0010;

    private static int showCount = 0;
    private static System.Random random = new System.Random();
    private static int screenWidth;
    private static int screenHeight;
    private static List<string> windowTitles = new List<string>();
    private bool shouldQuit = false;

    public Saving saving;

    void Start()
    {
        saving.Save();
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        ShowMessage("InfernOS has crashed.\n\nPlease check " + desktopPath + " for more information.", "Message");
    }

    void Update()
    {
        if (shouldQuit)
        {
            Application.Quit();
        }
    }

    public void ShowMessage(string message, string title)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        new Thread(() => {
            if (showCount < 10)
            {
                MessageBoxW(IntPtr.Zero, message, title, MB_OK | MB_ICONINFORMATION);
                showCount++;

                if (showCount < 4)
                {
                    ShowMessage(message, title);
                }
                else
                {
                    // Create all message boxes
                    for (int i = 0; i < 10; i++)
                    {
                        string uniqueTitle = $"{title} {Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        windowTitles.Add(uniqueTitle);

                        new Thread(() => {
                            MessageBoxW(IntPtr.Zero, "Please check " + desktopPath + " for more information.", uniqueTitle, MB_OK | MB_ICONINFORMATION);
                        }).Start();

                        Thread.Sleep(100);
                    }

                    // Wait for all to appear then close them
                    new Thread(() => {
                        Thread.Sleep(10); // Proper wait time
                        foreach (var windowTitle in windowTitles)
                        {
                            IntPtr hWnd = FindWindow(null, windowTitle);
                            if (hWnd != IntPtr.Zero)
                            {
                                PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            }
                        }
                        windowTitles.Clear();
                        shouldQuit = true; // Trigger quit from main thread
                    }).Start();
                }
            }
        }).Start();
#else
        Debug.Log("Windows message boxes only work on Windows: " + message);
#endif
    }
}