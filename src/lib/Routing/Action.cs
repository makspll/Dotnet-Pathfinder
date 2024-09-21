namespace Makspll.Pathfinder.Routing;

public class Action
{
    public required string Name { get; init; }
    public required IEnumerable<Route> Routes { get; init; }

    public required IEnumerable<RoutingAttribute> Attributes { get; init; }

}

public class Route
{
    public required string Path { get; init; }
    public required IEnumerable<HTTPMethod> Methods { get; init; }
}
