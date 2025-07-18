using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine;
using System.Linq;

public class Highlight
{
    public static string HighlightEntries(string output, Database database)
    {
        foreach (var entry in database.entries)
        {
            string[] names = entry.name.Split(',');

            foreach (var name in names)
            {
                if (entry.name != "yellow")
                {
                    string pattern = @"\b" + Regex.Escape(name.Trim()) + @"\b";
                    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

                    // Do not highlight inside <ignore> tags
                    output = Regex.Replace(output, @"<ignore>(.*?)</ignore>", match => match.Value); // preserve ignore blocks
                    output = regex.Replace(output, match =>
                    {
                        // Check if match is inside an <ignore> tag (skip it)
                        int index = match.Index;
                        int before = output.LastIndexOf("<ignore>", index);
                        int after = output.IndexOf("</ignore>", index);
                        if (before != -1 && after != -1 && before < index && index < after)
                            return match.Value;

                        return $"<color=yellow>{match.Value}</color>";
                    });
                }
            }
        }

        return output;
    }



    public static string UnderlineEntries(string entryText)
    {
        // Collect all entry names and sort by length (longest first)
        var allEntryNames = new List<string>();
        foreach (var entry in EntryTracker.GetAllFoundEntries())
        {
            foreach (var entryName in entry.Split(", "))
            {
                allEntryNames.Add(entryName.Trim());
            }
        }

        // Sort by length descending to prioritize longer matches
        allEntryNames = allEntryNames.OrderByDescending(name => name.Length).ToList();

        // Create a clean version without HTML tags for matching
        string cleanText = Regex.Replace(entryText, @"<[^>]+>", "");

        // Find all matches in the clean text
        var matches = new List<(int start, int end, string matchedText)>();

        foreach (var entryName in allEntryNames)
        {
            string pattern = $@"\b({Regex.Escape(entryName)})\b";
            var regexMatches = Regex.Matches(cleanText, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in regexMatches)
            {
                // Check if this position is already covered by a longer match
                bool isOverlapped = matches.Any(m =>
                    (match.Index >= m.start && match.Index < m.end) ||
                    (match.Index + match.Length > m.start && match.Index + match.Length <= m.end) ||
                    (match.Index <= m.start && match.Index + match.Length >= m.end));

                if (!isOverlapped)
                {
                    matches.Add((match.Index, match.Index + match.Length, match.Value));
                }
            }
        }

        // Sort matches by position (reverse order so we can insert from right to left)
        matches = matches.OrderByDescending(m => m.start).ToList();

        // Apply underlines to the original text by mapping clean positions to original positions
        string result = entryText;
        foreach (var match in matches)
        {
            // Find the corresponding position in the original text
            int originalStart = FindOriginalPosition(entryText, match.start);
            int originalEnd = FindOriginalPosition(entryText, match.end);

            if (originalStart != -1 && originalEnd != -1)
            {
                // Extract the actual text from the original (including any tags)
                string originalMatchText = result.Substring(originalStart, originalEnd - originalStart);

                // Don't underline if it's already underlined
                if (!originalMatchText.Contains("<u>") && !originalMatchText.Contains("</u>"))
                {
                    // Add underline tags inside the existing HTML structure
                    string underlinedText = AddUnderlineInsideTags(originalMatchText);
                    result = result.Substring(0, originalStart) + underlinedText + result.Substring(originalEnd);
                }
            }
        }

        return result;
    }

    private static string AddUnderlineInsideTags(string text)
    {
        // Find all text content (not inside tags) and wrap it with <u> tags
        return Regex.Replace(text, @"(?<=>|^)([^<]+)(?=<|$)", match =>
        {
            return $"<u>{match.Groups[1].Value}</u>";
        });
    }

    private static int FindOriginalPosition(string originalText, int cleanPosition)
    {
        int cleanIndex = 0;
        int originalIndex = 0;
        bool inTag = false;

        while (originalIndex < originalText.Length && cleanIndex < cleanPosition)
        {
            if (originalText[originalIndex] == '<')
            {
                inTag = true;
            }
            else if (originalText[originalIndex] == '>')
            {
                inTag = false;
                originalIndex++;
                continue;
            }

            if (!inTag)
            {
                cleanIndex++;
            }

            originalIndex++;
        }

        return originalIndex;
    }
    public static string RemoveTags(string output)
    {
        output = output.Replace("<ignore>", "");
        output = output.Replace("</ignore>", "");
        return output;
    }
    public static string CorruptText(string output)
    {
        var sections = new List<(string text, int index, bool skipCorruption)>();

        string yellowPattern = @"<color=yellow>.*?</color>";
        string ignorePattern = @"<ignore>.*?</ignore>";
        string combinedPattern = $"{yellowPattern}|{ignorePattern}";

        foreach (Match match in Regex.Matches(output, combinedPattern))
        {
            bool skipCorruption = match.Value.StartsWith("<color=yellow>") || match.Value.StartsWith("<ignore>");
            sections.Add((match.Value, match.Index, skipCorruption));
        }

        sections.Sort((a, b) => a.index.CompareTo(b.index));

        var finalSections = new List<string>();
        int currentPos = 0;

        foreach (var section in sections)
        {
            if (section.index > currentPos)
            {
                string before = output.Substring(currentPos, section.index - currentPos);
                string corrupted = CorruptSection(before);
                finalSections.Add($"<color=red>{corrupted}</color>");
            }

            if (section.skipCorruption)
            {
                finalSections.Add(section.text);
            }

            currentPos = section.index + section.text.Length;
        }

        if (currentPos < output.Length)
        {
            string leftover = output.Substring(currentPos);
            string corrupted = CorruptSection(leftover);
            finalSections.Add($"<color=red>{corrupted}</color>");
        }

        string result = string.Join("", finalSections);

        // Finally, remove <ignore> tags but keep inner content
        result = Regex.Replace(result, @"</?ignore>", "");

        return result;
    }

    private static string CorruptSection(string text)
    {
        char[] chars = text.ToCharArray();
        bool insideTag = false;

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '<')
            {
                insideTag = true;
            }
            else if (chars[i] == '>')
            {
                insideTag = false;
            }
            else if (!insideTag && !char.IsWhiteSpace(chars[i]) && char.IsLetter(chars[i]))
            {
                chars[i] = '☐';
            }
        }

        return new string(chars);
    }
}
