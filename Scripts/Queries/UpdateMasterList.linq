<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

async Task Main()
{
    var identifiers = await GetDeduplicatedIdentifiers();
    
    var existing = ReadExistingIdentifiers();
    identifiers.Where(id => !existing.Any(eid => eid.Contains(id) || id.Contains(eid)))
        // .Where(id => id.Contains("single_line"))
        .Dump();
}

public List<string> ReadExistingIdentifiers()
{
    var existing = File.ReadAllLines(Path.Combine(Util.CurrentQueryPath, "..", "..", "..", ".editorconfig"));
    var pattern = new Regex(@"^(?<id>[a-z_0-9]*)\s*=");
    return (from line in existing
            let match = pattern.Match(line)
            where match.Success
            let identifier = match.Groups["id"].Value.Trim()
            select identifier).ToList();
}

public async Task<List<string>> GetDeduplicatedIdentifiers()
{
    var identifiers = await GetIdentifiers();
    foreach (var identifier in identifiers.Where(id => id.StartsWith("resharper_")).ToList())
    {
        var subidentifier = identifier.Substring(10);
        identifiers.RemoveAll(id => "resharper_" + id == identifier);
        identifiers.RemoveAll(id => id == "csharp_" + subidentifier);
        identifiers.RemoveAll(id => id == "resharper_csharp_" + subidentifier);
        identifiers.RemoveAll(id => id == "resharper_cpp_" + subidentifier);
    }

    foreach (var lang in new[] { "cpp", "js", "vb", "css", "razor", "html" })
    {
        identifiers.RemoveAll(id => id.StartsWith(lang + "_"));
        identifiers.RemoveAll(id => id.Contains("_" + lang + "_"));
    }
    identifiers.RemoveAll(id => id.EndsWith("_braces"));
    return identifiers;
}

public async Task<List<string>> GetIdentifiers()
{
    string html;
    using (var client = new HttpClient())
    {
        html = await client.GetStringAsync(@"https://www.jetbrains.com/help/resharper/EditorConfig_Index.html");
    }

    var pattern = new Regex(@"<code.*?>(?<key>[^<]+?)</code>");

    var matches = pattern.Matches(html);
    return matches.OfType<Match>().Select(ma => ma.Groups["key"].Value).ToList();
}
