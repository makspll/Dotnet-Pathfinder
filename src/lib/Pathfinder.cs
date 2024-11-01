
using System.Text.Json;
using ANSIConsole;
using dnlib.DotNet;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.RoutingConfig;
using Makspll.Pathfinder.Search;

namespace Makspll.Pathfinder;

public class Pathfinder(IEnumerable<string> dllGlobs, string directory, string configPath)
{

    public IEnumerable<Assembly> Analyze()
    {
        var dlls = FileSearch.FindAllFiles(dllGlobs, directory).ToList();

        var output = new List<Assembly>();

        foreach (var dll in dlls)
        {
            AssemblyQuery query;
            var dllDirectory = Path.GetDirectoryName(dll) ?? dll;
            var configFileLocations = new List<string>
            {
                configPath,
                Path.Combine(directory, "pathfinder.json"),
                Path.Combine(dllDirectory, "..", "..", "..", "pathfinder.json"),
                Path.Combine(dllDirectory, "..", "..", "pathfinder.json"),
                Path.Combine(dllDirectory, "..", "pathfinder.json"),
            };

            var module = ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext());
            var frameworkVersion = module.DetectFrameworkVersion();

            var config = configFileLocations.Select(x => ParseConfig(new FileInfo(x), frameworkVersion)).FirstOrDefault(x => x != null);
            if (config == null)
            {
                Console.WriteLine($"No 'pathfinder.json' file specified or found when processing dll: '{dll}'. Ignoring".Color(ConsoleColor.Red));
            }

            query = new AssemblyQueryBuilder()
                .WithModule(module)
                .WithConfig(config)
                .Build();

            var controllers = query.FindAllControllers().ToList();

            var assembly = new Assembly
            {
                Name = Path.GetFileNameWithoutExtension(dll),
                Path = dll,
                Controllers = controllers,
                FrameworkVersion = query.DetectedFramework
            };
            output.Add(assembly);
        }

        return output;
    }

    public static ParsedPathfinderConfig? ParseConfig(FileInfo configFile, FrameworkVersion version)
    {
        if (!configFile.Exists)
            return null;

        var config = JsonSerializer.Deserialize<PathfinderConfig>(File.ReadAllText(configFile.FullName));

        if (config == null)
            return new();

        return new ParsedPathfinderConfig(config, version);
    }
}

public enum OutputFormat
{
    JSON,
    Text
}