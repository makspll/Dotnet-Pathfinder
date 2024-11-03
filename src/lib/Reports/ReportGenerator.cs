using System.Reflection;
using HandlebarsDotNet;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.Search;

namespace Makspll.Pathfinder.Reports;

public enum ReportKind
{
    RawTemplates,
    Endpoint
}


public class ReportGenerator
{
    public ReportGenerator(ReportKind kind, string? additonalTemplatesDir)
    {
        SetupHelpers();

        if (additonalTemplatesDir != null)
        {
            FileSearch.FindAllFiles(["*.hbs"], additonalTemplatesDir)
                .ToList()
                .ForEach(x =>
                {
                    var content = File.ReadAllText(x);
                    Handlebars.RegisterTemplate(Path.GetFileNameWithoutExtension(x), content);
                });
        }

        var templates = FindStaticResources("Resources.Templates");
        foreach (var (fileName, resourcePath) in templates)
        {
            // User templates take precedence
            if (Handlebars.Configuration.RegisteredTemplates.ContainsKey(fileName))
                continue;

            var content = LoadStaticResource(resourcePath);
            Handlebars.RegisterTemplate(fileName, content);
        }
    }

    private static void SetupHelpers()
    {
        Handlebars.RegisterHelper("id", (writer, context, parameters) =>
        {
            var obj = context.Value;
            if (parameters.Length > 0)
            {
                obj = parameters[0];
            }

            switch (obj)
            {
                case Routing.Assembly assembly:
                    writer.Write(assembly.Name);
                    break;
                case Routing.Controller controller:
                    writer.Write($"{controller.Namespace}::{controller.ControllerName}");
                    break;
                case Routing.Action action:
                    writer.Write($"{action.GetHashCode()}{action.Name}");
                    break;
                case Routing.Route route:
                    writer.Write($"{route.GetHashCode()}{route.Path}");
                    break;
                default:
                    writer.Write(obj.GetHashCode());
                    break;
            }
        });
    }

    private object PrepareContext(List<Routing.Assembly> assemblies)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        Routing.Assembly.Serialize(assemblies, OutputFormat.JSON, writer);
        stream.Flush();
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return new
        {
            Assemblies = assemblies,
            Json = json
        };
    }

    public void GenerateReport(IEnumerable<Routing.Assembly> assemblies, string? outputDirectory)
    {
        var template = Handlebars.Compile("{{> report.hbs }}");
        var context = PrepareContext(assemblies.ToList());
        var result = template(context);
        var reportDir = Path.Combine(outputDirectory ?? ".", "report");
        Directory.CreateDirectory(reportDir);

        var index = Path.Combine(reportDir, "index.html");
        File.WriteAllText(index, result);

        var staticResources = FindStaticResources("Resources.Static");
        foreach (var (fileName, resourcePath) in staticResources)
        {
            var content = LoadStaticResource(resourcePath);
            var outputPath = Path.Combine(reportDir, fileName);
            File.WriteAllText(outputPath, content);
        }
    }

    private static readonly string RootNamespace = "Makspll.Pathfinder";

    private static string ManifestPathToUri(string path)
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

    private static IEnumerable<(string FileName, string ResourcePath)> FindStaticResources(string prefix)
    {
        var fullPrefix = $"{RootNamespace}.{prefix}";
        return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(x => x.StartsWith(fullPrefix))
            .Select(x => (ManifestPathToUri(x.Replace(fullPrefix + ".", "")), x));
    }

    private static string LoadStaticResource(string resourcePath)
    {
        using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath)
            ?? throw new FileNotFoundException($"Resource {resourcePath} not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}