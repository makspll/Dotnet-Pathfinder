using dnlib.DotNet;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Intermediate;

public class ActionCandidate
{
    public required MethodDef Method { get; set; }
    public required IEnumerable<RoutingAttribute> RoutingAttributes { get; set; }
    public IEnumerable<RoutingAttribute> PropagatedControllerAttributes { get; set; } = [];
}