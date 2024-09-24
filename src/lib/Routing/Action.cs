namespace Makspll.Pathfinder.Routing;

public class Action
{
    public required string MethodName { get; init; }
    public required List<Route> Routes { get; init; }
    public required bool IsConventional { get; set; }

    public required IEnumerable<RoutingAttribute> Attributes { get; init; }

}

public class Route
{
    public required string Path { get; init; }
    public required List<HTTPMethod> Methods { get; init; }
}
