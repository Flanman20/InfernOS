using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntryTracker
{
    private static HashSet<string> foundEntries = new HashSet<string>();

    public static void MarkAsFound(string entryName)
    {
        Debug.LogWarning($"Entry received as {entryName}");
        foundEntries.Add(entryName.ToLowerInvariant());
    }

    public static bool IsFound(string entryName)
    {
        return foundEntries.Contains(entryName.ToLowerInvariant());
    }

    public static IEnumerable<string> GetAllFoundEntries()
    {
        return foundEntries;
    }

    // Add methods for saving/loading
    public static List<string> GetFoundEntriesForSaving()
    {
        return new List<string>(foundEntries);
    }

    public static void LoadFoundEntries(List<string> entries)
    {
        foundEntries.Clear();
        if (entries != null)
        {
            foreach (string entry in entries)
            {
                foundEntries.Add(entry);
            }
        }
    }

    public static void ClearFoundEntries()
    {
        foundEntries.Clear();
    }
}