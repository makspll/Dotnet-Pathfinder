using System.Text.Json;
using ANSIConsole;
using dnlib.DotNet;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.PostProcess;
using Makspll.Pathfinder.Propagation;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.RoutingConfig;

namespace Makspll.Pathfinder.Search;

public class AssemblyQuery(ModuleDefMD module, FrameworkVersion frameworkVersion, ParsedPathfinderConfig? config = null)
{
    readonly ModuleDefMD LoadedModule = module;
    readonly ParsedPathfinderConfig config = config ?? new();

    public readonly FrameworkVersion DetectedFramework = frameworkVersion;
    private readonly RouteCalculator routeCalculator = new(frameworkVersion);
    private readonly ActionFinder actionFinder = new(frameworkVersion);
    private readonly AttributePropagator attributePropagator = new(frameworkVersion);
    private readonly PlaceholderInliner placeholderInliner = new(frameworkVersion);
    private readonly CandidateConverter candidateConverter = new(frameworkVersion);
    private readonly ControllerFinder controllerFinder = new(frameworkVersion);

    public AssemblyQuery(string dll, ParsedPathfinderConfig? config = null) : this(ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext()), config ?? FindAndParseNearestConfig(dll)) { }
    public AssemblyQuery(ModuleDefMD module, ParsedPathfinderConfig? config) : this(module, module.DetectFrameworkVersion(), config) { }

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


    public IEnumerable<Controller> FindAllControllers()
    {

        var types = LoadedModule.GetTypes();
        var candidateControllers = controllerFinder.FindControllers(LoadedModule).ToList();

        foreach (var controllerCandidate in candidateControllers)
        {

            actionFinder.PopulateActions(controllerCandidate);
            attributePropagator.PropagateAttributes(controllerCandidate);
            controllerCandidate.Actions.ForEach(routeCalculator.PopulateRoutes);
            foreach (var conventionalRoute in config.ConventionalRoutes)
            {
                controllerCandidate.Actions.ForEach(x => routeCalculator.PopulateConventionalRoutes(x, conventionalRoute));
            }
        }

        placeholderInliner.InlinePlaceholders(candidateControllers);
        return candidateConverter.ConvertCandidates(candidateControllers);
    }
}