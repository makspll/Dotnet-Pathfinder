namespace Makspll.ReflectionUtils.Routing;

public class Action
{
    public required string Name { get; init; }
    public required IEnumerable<string> Routes { get; init; }

    public required IEnumerable<RoutingAttribute> Attributes { get; init; }

}
