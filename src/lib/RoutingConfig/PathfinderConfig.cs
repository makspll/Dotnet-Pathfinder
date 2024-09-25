using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.RoutingConfig;

public class PathfinderConfig
{
    public required IEnumerable<ConventionalRouteTemplateConfig> ConventionalRoutes { get; set; }
}

public class ConventionalRouteTemplateConfig
{
    public required string Template { get; set; }

    public Dictionary<string, string>? Defaults { get; set; }
}