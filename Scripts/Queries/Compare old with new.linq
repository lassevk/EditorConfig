<Query Kind="Program" />

void Main()
{
    var rootPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "..", ".."));
    var oldFilename = Path.Combine(rootPath, ".editorconfig.old");
    var newFilename = Path.Combine(rootPath, ".editorconfig");
    
    var oldSettings = File.ReadAllLines(oldFilename);
    var newSettings = File.ReadAllLines(newFilename);

    var sectionPattern = new Regex(@"^\[(?<name>.*)\]$");
    string sectionPrefix = string.Empty;
    
    foreach (var line in newSettings)
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;
        if (line.StartsWith("#"))
            continue;

        var ma = sectionPattern.Match(line);
        if (ma.Success)
        {
            sectionPrefix = ma.Groups["name"].Value + ".";
            continue;
        }
        
        var trimmed = line;
        if (trimmed.StartsWith("resharper_"))
            trimmed = trimmed.Substring(10);
        if (trimmed.StartsWith("csharp_"))
            trimmed = trimmed.Substring(7);
            
        var matches = oldSettings.Where(s => s.Contains(trimmed)).Select(l => l.TrimStartIf("resharper_").TrimStartIf("csharp_")).ToList();
        if (matches.Count > 0 && matches.All(m => m == trimmed))
            continue;
            
        if (matches.Count == 0)
            continue;
        
        matches.Dump(line);
    }
}

public static class Extensions
{
    public static string TrimStartIf(this string input, string pattern)
    {
        if (input.StartsWith(pattern))
            return input.Substring(pattern.Length);
        return input;
    }
}