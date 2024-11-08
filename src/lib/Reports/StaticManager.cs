namespace Makspll.Pathfinder.Reports;

internal static class StaticManager
{

    private static readonly string RootNamespace = "Makspll.Pathfinder";

    public static string ManifestPathToUri(string path)
    {
        var parts = path.Split('.');
        if (parts.Length == 0)
        {
            return path;
        }

        var extension = parts[^1];

        var filePath = string.Join('/', parts[..^1]);
        return $"{filePath}.{extension}";
    }

    public static IEnumerable<(string FileName, string ResourcePath)> FindStaticResources(string prefix)
    {
        var fullPrefix = $"{RootNamespace}.{prefix}";
        return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(x => x.StartsWith(fullPrefix))
            .Select(x => (ManifestPathToUri(x.Replace(fullPrefix + ".", "")), x));
    }

    public static string LoadStaticResource(string resourcePath)
    {
        using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath)
            ?? throw new FileNotFoundException($"Resource {resourcePath} not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static IEnumerable<string> GenerateStylesheets()
    {
        // find all css files 
        var cssFiles = FindStaticResources("Resources.Static")
            .Where(x => x.FileName.EndsWith(".css"))
            .Select(x => LoadStaticResource(x.ResourcePath));

        var cssFileTemplate = @"
            <style>
                {0}
            </style>
        ";

        return cssFiles.Select(x => string.Format(cssFileTemplate, x));
    }

    public static IEnumerable<string> GenerateScripts()
    {
        // find all js files 
        var jsFiles = FindStaticResources("Resources.Static")
            .Where(x => x.FileName.EndsWith(".js"))
            .Select(x => LoadStaticResource(x.ResourcePath));

        var jsFileTemplate = @"
            <script>
                {0}
            </script>
        ";

        return jsFiles.Select(x => string.Format(jsFileTemplate, x));
    }
}