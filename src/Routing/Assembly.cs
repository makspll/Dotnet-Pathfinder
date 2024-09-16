namespace Makspll.ReflectionUtils.Routing;

public class Assembly
{
    public required string Name { get; init; }

    public required string Path { get; init; }
    public required IEnumerable<Controller> Controllers { get; init; }
}