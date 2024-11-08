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

public static class ReportKindExtensions
{
    public static string ToFriendlyString(this ReportKind kind)
    {
        return kind switch
        {
            ReportKind.RawTemplates => "Raw Templates Report",
            ReportKind.Endpoint => "Endpoint Report",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}


public class ReportGenerator
{
    private ReportKind Kind { get; init; }

    public ReportGenerator(ReportKind kind, string? additonalTemplatesDir)
    {
        Kind = kind;
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

        var templates = StaticManager.FindStaticResources("Resources.Templates");
        foreach (var (fileName, resourcePath) in templates)
        {
            // User templates take precedence
            if (Handlebars.Configuration.RegisteredTemplates.ContainsKey(fileName))
                continue;

            var content = StaticManager.LoadStaticResource(resourcePath);
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
            Scripts = string.Join("\n", StaticManager.GenerateScripts()),
            Styles = string.Join("\n", StaticManager.GenerateStylesheets()),
            Title = Kind.ToFriendlyString(),
            Assemblies = assemblies,
            Json = json
        };
    }

    public void GenerateReport(IEnumerable<Routing.Assembly> assemblies, string? outputDirectory)
    {
        var template = Handlebars.Compile("{{> report.hbs }}");
        var context = PrepareContext(assemblies.ToList());
        var result = template(context);
        var reportDir = Path.Combine(outputDirectory ?? "./report");
        Directory.CreateDirectory(reportDir);

        var index = Path.Combine(reportDir, "index.html");
        File.WriteAllText(index, result);
    }

}