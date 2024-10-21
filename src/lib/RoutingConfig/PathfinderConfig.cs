using CommandLine;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.RoutingConfig;


public class ParsedPathfinderConfig
{

    public ParsedPathfinderConfig(PathfinderConfig config)
    {

        var results = config.ConventionalRoutes.Select(x =>
        {
            var route = ConventionalRoute.Parse(x.Template, x.Defaults, x.Type);
            return (route, x.Type);
        }).ToList();

        var failures = results.Where(x => x.route.IsFailed).Select(x => x.route.Errors);
        if (failures.Any())
            throw new Exception($"Encountered errors when parsing config: {string.Join('\n', failures)}");

        ConventionalRoutes = results
            .Select(x => x.route.ValueOrDefault)
            .OfType<ConventionalRoute>()
            .ToList();
    }

    public ParsedPathfinderConfig()
    {

    }

    public IEnumerable<ConventionalRoute> ConventionalRoutes { get; set; } = [];
}

public class PathfinderConfig
{
    public required IEnumerable<ConventionalRouteTemplateConfig> ConventionalRoutes { get; set; }
}

public class ConventionalRouteTemplateConfig
{
    public required string Template { get; set; }

    public Dictionary<string, string>? Defaults { get; set; }

    public ConventionalRouteType? Type { get; set; }
}