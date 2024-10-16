using dnlib.DotNet;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Intermediate;

public class ControllerCandidate
{
    public required TypeDef Type { get; set; }
    public required IEnumerable<RoutingAttribute> RoutingAttributes { get; set; }

    public List<ActionCandidate> Actions { get; set; } = [];

    public string ControllerName => Type.Name.EndsWith("Controller") ? Type.Name.Substring(0, Type.Name.Length - "Controller".Length) : Type.Name;
}