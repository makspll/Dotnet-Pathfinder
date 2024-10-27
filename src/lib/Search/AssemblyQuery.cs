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

public class AssemblyQueryBuilder
{
    private ModuleDefMD? LoadedModule { get; set; }
    private ParsedPathfinderConfig? Config { get; set; }
    private FrameworkVersion? DetectedFramework { get; set; }
    private IRouteCalculator? RouteCalculator { get; set; }
    private IActionFinder? ActionFinder { get; set; }
    private IAttributePropagator? AttributePropagator { get; set; }
    private IPlaceholderInliner? PlaceholderInliner { get; set; }
    private ICandidateConverter? CandidateConverter { get; set; }
    private IControllerFinder? ControllerFinder { get; set; }


    /// <summary>
    /// Set the module to be analyzed, and automatically detect the framework version
    /// </summary>
    public AssemblyQueryBuilder WithModule(ModuleDefMD module)
    {
        LoadedModule = module;
        DetectedFramework = module.DetectFrameworkVersion();
        return this;
    }

    /// <summary>
    /// Set the module to be analyzed, and automatically detect the framework version, from a path to the DLL file of this assembly
    /// </summary>
    public AssemblyQueryBuilder WithModule(string dll)
    {
        LoadedModule = ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext());
        DetectedFramework = LoadedModule.DetectFrameworkVersion();
        return this;
    }

    /// <summary>
    /// Override the automatically detected framework version
    /// </summary>
    public AssemblyQueryBuilder WithOverrideDetectedFramework(FrameworkVersion frameworkVersion)
    {
        DetectedFramework = frameworkVersion;
        return this;
    }

    /// <summary>
    /// Set the configuration to be used for the analysis
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public AssemblyQueryBuilder WithConfig(ParsedPathfinderConfig? config)
    {
        Config = config;
        return this;
    }

    public AssemblyQueryBuilder WithRouteCalculator(RouteCalculator routeCalculator)
    {
        RouteCalculator = routeCalculator;
        return this;
    }

    public AssemblyQueryBuilder WithActionFinder(ActionFinder actionFinder)
    {
        ActionFinder = actionFinder;
        return this;
    }

    public AssemblyQueryBuilder WithAttributePropagator(AttributePropagator attributePropagator)
    {
        AttributePropagator = attributePropagator;
        return this;
    }

    public AssemblyQueryBuilder WithPlaceholderInliner(PlaceholderInliner placeholderInliner)
    {
        PlaceholderInliner = placeholderInliner;
        return this;
    }

    public AssemblyQueryBuilder WithCandidateConverter(CandidateConverter candidateConverter)
    {
        CandidateConverter = candidateConverter;
        return this;
    }

    public AssemblyQueryBuilder WithControllerFinder(ControllerFinder controllerFinder)
    {
        ControllerFinder = controllerFinder;
        return this;
    }

    public AssemblyQuery Build()
    {
        if (LoadedModule == null)
            throw new InvalidOperationException("Module must be set before building AssemblyQuery");

        if (DetectedFramework == null)
            throw new InvalidOperationException("Framework must be set before building AssemblyQuery");

        var query = new AssemblyQuery(LoadedModule, DetectedFramework.Value, Config);

        if (RouteCalculator != null)
            query.routeCalculator = RouteCalculator;
        if (ActionFinder != null)
            query.actionFinder = ActionFinder;
        if (AttributePropagator != null)
            query.attributePropagator = AttributePropagator;
        if (PlaceholderInliner != null)
            query.placeholderInliner = PlaceholderInliner;
        if (CandidateConverter != null)
            query.candidateConverter = CandidateConverter;
        if (ControllerFinder != null)
            query.controllerFinder = ControllerFinder;

        return query;
    }
}


public class AssemblyQuery(ModuleDefMD module, FrameworkVersion frameworkVersion, ParsedPathfinderConfig? config = null)
{
    readonly ModuleDefMD LoadedModule = module;
    readonly ParsedPathfinderConfig config = config ?? new();

    public FrameworkVersion DetectedFramework = frameworkVersion;
    internal IRouteCalculator routeCalculator = new RouteCalculator(frameworkVersion);
    internal IActionFinder actionFinder = new ActionFinder(frameworkVersion);
    internal IAttributePropagator attributePropagator = new AttributePropagator(frameworkVersion);
    internal IPlaceholderInliner placeholderInliner = new PlaceholderInliner(frameworkVersion);
    internal ICandidateConverter candidateConverter = new CandidateConverter(frameworkVersion);
    internal IControllerFinder controllerFinder = new ControllerFinder(frameworkVersion);


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

        placeholderInliner.SwapSpecialCharacters(candidateControllers);

        placeholderInliner.InlinePlaceholders(candidateControllers);

        placeholderInliner.UnescapeSwappedSpecialCharacters(candidateControllers);

        return candidateConverter.ConvertCandidates(candidateControllers);
    }
}