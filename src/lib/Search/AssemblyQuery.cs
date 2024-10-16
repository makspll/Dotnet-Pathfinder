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

public class AssemblyQuery(ModuleDefMD module, PathfinderConfig? config = null)
{
    readonly ModuleDefMD LoadedModule = module;
    readonly PathfinderConfig? config = config;
    readonly IEnumerable<ConventionalRoute> routes = config?.ConventionalRoutes
        .Select(x => ConventionalRoute.Parse(x.Template, x.Defaults).ValueOrDefault)
        .OfType<ConventionalRoute>() ?? [];

    public AssemblyQuery(string dll, PathfinderConfig? config = null) : this(ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext()), config ?? FindAndParseNearestConfig(dll)) { }


    public static PathfinderConfig? ParseConfig(FileInfo configFile)
    {
        if (!configFile.Exists)
            return null;

        var config = JsonSerializer.Deserialize<PathfinderConfig>(File.ReadAllText(configFile.FullName));

        if (config == null)
            return null;

        var results = config.ConventionalRoutes.Select(x => ConventionalRoute.Parse(x.Template, x.Defaults)).ToList();
        if (results == null)
            return null;

        var failedResults = results.Where(x => x.IsFailed).Select(x => x.Errors).ToList();

        if (failedResults.Count > 0)
        {
            throw new Exception($"Encountered errors when parsing templates: {string.Join('\n', failedResults)}");
        }

        return config;
    }

    /// <summary>
    /// Finds and parses the nearest pathfinder.json file in the directory tree starting from the given directory. Returns null if no file is found.
    /// </summary>
    public static PathfinderConfig? FindAndParseNearestConfig(string dll)
    {
        var dllDirectory = Path.GetDirectoryName(dll);
        var configPath = FileSearch.FindNearestFile("pathfinder.json", dllDirectory ?? dll);
        if (configPath == null)
            return null;

        return ParseConfig(configPath);
    }

    // static HTTPMethod? ActionNameToVerb(string name)
    // {
    //     foreach (var verb in Enum.GetNames<HTTPMethod>())
    //     {
    //         // title case the verb 
    //         var titleCaseVerb = verb.ToString()[0].ToString().ToUpper() + verb.ToString()[1..].ToLower();
    //         if (name.StartsWith(titleCaseVerb))
    //         {
    //             return Enum.Parse<HTTPMethod>(verb);
    //         }
    //     }
    //     return null;
    // }

    IEnumerable<ControllerCandidate> FindControllers()
    {
        var types = LoadedModule.GetTypes();
        var controllers = new List<Controller>();
        foreach (var controllerCandidate in ControllerFinder.FindControllers(LoadedModule))
        {

            ActionFinder.PopulateActions(controllerCandidate);
            AttributePropagator.PropagateAttributes(controllerCandidate);
            controllerCandidate.Actions.ForEach(RouteCalculator.PopulateRoutes);
            foreach (var conventionalRoute in routes)
            {
                controllerCandidate.Actions.ForEach(x => RouteCalculator.PopulateConventionalRoutes(x, conventionalRoute));
            }

            yield return controllerCandidate;
        }
    }

    public IEnumerable<Controller> FindAllControllers()
    {
        var controllers = FindControllers();
        var converted = CandidateConverter.ConvertCandidates(controllers);
        PlaceholderInliner.InlinePlaceholders(converted);

        return converted;
    }
}