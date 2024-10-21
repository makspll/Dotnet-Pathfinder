using dnlib.DotNet;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Intermediate;

public class ActionCandidate
{
    public required ControllerCandidate Controller { get; set; }
    public required MethodDef Method { get; set; }
    public required IEnumerable<RoutingAttribute> RoutingAttributes { get; set; }
    public IEnumerable<PropagatedRoute> PropagatedRoutes { get; set; } = [];
    public IList<Route> Routes { get; set; } = [];
    public IList<Route> ConventionalRoutes { get; set; } = [];

    public string ActionName(FrameworkVersion version) => RoutingAttributes.FirstOrDefault(x => x.ActionName(version) != null)?.ActionName(version) ?? Method.Name;
}

public record PropagatedRoute
{
    public required string? Prefix { get; set; }
    public required string? Route { get; set; }
    public required bool FromController { get; set; }
}