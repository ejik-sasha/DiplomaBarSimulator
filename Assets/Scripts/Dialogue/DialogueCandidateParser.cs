using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueCandidateParser
{
    public List<string> ParseCandidates(string rawResponse)
    {
        List<string> candidates = new();

        if (string.IsNullOrWhiteSpace(rawResponse))
            return candidates;

        string[] lines = rawResponse.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = Regex.Replace(line, @"^\d+[\).\-\s]+", "");
            line = Regex.Replace(line, @"^[-•]\s*", "");
            line = line.Trim();

            line = line.Trim('"', '«', '»');

            if (!string.IsNullOrWhiteSpace(line))
                candidates.Add(line);
        }

        if (candidates.Count == 0 && !string.IsNullOrWhiteSpace(rawResponse))
            candidates.Add(rawResponse.Trim());

        return candidates;
    }
}