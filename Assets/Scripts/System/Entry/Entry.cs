using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class Entry
{
    public string name;

    [TextArea(3, 10)]
    public string output;  
    
    public int progressionLevel;

    [TextArea(3, 10)]
    public string altOutput;
    public type entryType;

    public enum type
    {
        Normal,
        SlowType,
        Corrupted
    }

    // Method to process the output string with variables
    public string ProcessOutput(BIOS bios)
    {
        if (string.IsNullOrEmpty(output)) return string.Empty;

        string processed = output;

        var matches = Regex.Matches(processed, @"\{([^}]+)\}");

        foreach (Match match in matches)
        {
            string placeholder = match.Groups[1].Value.Trim();
            string replacement = "";

            // Handle different variable cases
            switch (placeholder.ToLower())
            {
                case "bios.progression.progressionlevel":
                    replacement = bios.progression.progressionLevel.ToString();
                    break;
                case "metadata.version":
                    replacement = Metadata.VERSION;
                    break;
                // Add more cases here for other variables
                case "metadata.build_number":
                    replacement = Metadata.BUILD_NUMBER; // Add the case for VERSION
                    break;
                case "randomvalue":
                    replacement = UnityEngine.Random.Range(0f, 0.001f).ToString();
                    break;
            }

            processed = processed.Replace(match.Value, replacement);
        }

        return processed;
    }

    // Method to process altOutput string
    public string ProcessAltOutput(BIOS bios)
    {
        if (string.IsNullOrEmpty(altOutput)) return string.Empty;

        string processed = altOutput;
        var matches = Regex.Matches(processed, @"\{([^}]+)\}");

        foreach (Match match in matches)
        {
            string placeholder = match.Groups[1].Value.Trim();
            string replacement = "";

            switch (placeholder.ToLower())
            {
                case "bios.progression.progressionlevel":
                    replacement = bios.progression.progressionLevel.ToString();
                    break;
                    // Add more cases here for other variables
            }

            processed = processed.Replace(match.Value, replacement);
        }

        return processed;
    }
}