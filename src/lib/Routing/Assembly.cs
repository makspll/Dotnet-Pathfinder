namespace Makspll.Pathfinder.Routing;

public class Assembly
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public required FrameworkVersion FrameworkVersion { get; init; }
    public required IEnumerable<Controller> Controllers { get; init; }
}