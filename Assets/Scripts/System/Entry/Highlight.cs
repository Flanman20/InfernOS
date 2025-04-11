using System.Collections.Generic;
using System.Text.RegularExpressions;

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
