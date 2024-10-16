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

    public string ActionName => RoutingAttributes.FirstOrDefault(x => x.ActionName() != null)?.ActionName() ?? Method.Name;
}

public record PropagatedRoute
{
    public required string Prefix { get; set; }
    public required RoutePropagation PropagationType { get; set; }
    public required bool FromController { get; set; }
}