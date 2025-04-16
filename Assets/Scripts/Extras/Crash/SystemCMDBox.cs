using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

public class SystemCMDBox : MonoBehaviour
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

    // Number of CMD windows to create
    private const int NUM_TERMINALS = 6;
    // Delay between terminal launches
    private const int LAUNCH_DELAY_MS = 100;
    // Time before cleanup (in seconds)
    private const int CLEANUP_DELAY_SEC = 3;

    public Saving saving;

    // Cache file paths to avoid recalculating 
    private string tempPath;
    private string killerBatchPath;
    private string[] batchFilePaths;

    private void Awake()
    {
        // Pre-compute file paths
        tempPath = Path.GetTempPath();
        killerBatchPath = Path.Combine(tempPath, "InfernOS_Cleanup.bat");

        // Pre-allocate array for batch file paths
        batchFilePaths = new string[NUM_TERMINALS];
        for (int i = 0; i < NUM_TERMINALS; i++)
        {
            batchFilePaths[i] = Path.Combine(tempPath, $"InfernOS_Terminal_{i}.bat");
        }
    }

    public void ShowMessage(string message, string title)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Create batch files asynchronously on ThreadPool instead of new Thread
        ThreadPool.QueueUserWorkItem(_ => CreateAndLaunchTerminals(desktopPath));
#else
        UnityEngine.Debug.Log("Windows message boxes only work on Windows: " + message);
#endif
    }

    private void CreateAndLaunchTerminals(string desktopPath)
    {
        // Create all batch files first (single batch content creation)
        string baseContent = GetBaseBatchContent(desktopPath);

        // Create all batch files first
        for (int i = 0; i < NUM_TERMINALS; i++)
        {
            // Create batch file with terminal-specific title
            string content = baseContent.Replace("{TERMINAL_NUMBER}", i.ToString());
            File.WriteAllText(batchFilePaths[i], content);
        }

        // Create killer script before launching terminals
        CreateKillerScript();

        // Launch all terminals
        for (int i = 0; i < NUM_TERMINALS; i++)
        {
            LaunchTerminal(batchFilePaths[i]);
            Thread.Sleep(LAUNCH_DELAY_MS);
        }

        // Launch killer script
        LaunchKillerScript();
    }

    private string GetBaseBatchContent(string desktopPath)
    {
        // Create batch content template once instead of for each terminal
        return "@echo off\r\n" +
               "title InfernOS Terminal {TERMINAL_NUMBER}\r\n" +
               "color 4f\r\n" + // Red background, white text
               "cls\r\n" +
               ":loop\r\n" +
               $"echo InfernOS has crashed. Please check {desktopPath} for more information.\r\n" +
               "ping -n 1 127.0.0.1 > nul\r\n" + // Small delay
               "goto loop\r\n";
    }

    private void CreateKillerScript()
    {
        string killerContent = "@echo off\r\n" +
                              $"timeout /t {CLEANUP_DELAY_SEC} > nul\r\n" +
                               "taskkill /f /fi \"WINDOWTITLE eq InfernOS Terminal*\"\r\n" + // Kill all terminals
                               "del /q \"%~f0\"\r\n"; // Self-delete the batch file

        File.WriteAllText(killerBatchPath, killerContent);
    }

    private void LaunchTerminal(string batchFilePath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c start \"\" \"{batchFilePath}\"",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        Process.Start(startInfo);
    }

    private void LaunchKillerScript()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = killerBatchPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        Process.Start(startInfo);
    }

    private void OnDestroy()
    {
        // Clean up any leftover batch files when component is destroyed
        try
        {
            // Attempt to terminate any remaining processes
            Process.Start("taskkill", "/f /fi \"WINDOWTITLE eq InfernOS Terminal*\"");

            // Clean up batch files
            for (int i = 0; i < NUM_TERMINALS; i++)
            {
                if (File.Exists(batchFilePaths[i]))
                {
                    File.Delete(batchFilePaths[i]);
                }
            }

            // Clean up killer batch
            if (File.Exists(killerBatchPath))
            {
                File.Delete(killerBatchPath);
            }
        }
        catch (Exception)
        {
            // Silently fail on cleanup errors
        }
    }
}