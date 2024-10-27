
using System.Text.Json;
using ANSIConsole;
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

            var config = configFileLocations.Select(x => ParseConfig(new FileInfo(x))).FirstOrDefault(x => x != null);
            if (config == null)
            {
                Console.WriteLine($"No 'pathfinder.json' file specified or found when processing dll: '{dll}'. Ignoring".Color(ConsoleColor.Red));
            }

            query = new AssemblyQueryBuilder()
                .WithModule(dll)
                .WithConfig(config)
                .Build();

            var controllers = query.FindAllControllers().ToList();

            var assembly = new Assembly
            {
                Name = dll.Split('/').Last(),
                Path = dll,
                Controllers = controllers,
                FrameworkVersion = query.DetectedFramework
            };
            output.Add(assembly);
        }

        return output;
    }

    public static ParsedPathfinderConfig ParseConfig(FileInfo configFile)
    {
        if (!configFile.Exists)
            return new();

        var config = JsonSerializer.Deserialize<PathfinderConfig>(File.ReadAllText(configFile.FullName));

        if (config == null)
            return new();

        return new ParsedPathfinderConfig(config);
    }

    /// <summary>
    /// Finds and parses the nearest pathfinder.json file in the directory tree starting from the given directory. Returns null if no file is found.
    /// </summary>
    public static ParsedPathfinderConfig FindAndParseNearestConfig(string dll)
    {
        var dllDirectory = Path.GetDirectoryName(dll);
        var configPath = FileSearch.FindNearestFile("pathfinder.json", dllDirectory ?? dll);
        if (configPath == null)
            return new();

        return ParseConfig(configPath);
    }
}

public enum OutputFormat
{
    JSON,
    Text
}