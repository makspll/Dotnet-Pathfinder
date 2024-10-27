using System.Text.Json;
using System.Text.Json.Serialization;

namespace Makspll.Pathfinder.Routing;

public class Action
{
    public required string Name { get; init; }
    public required string MethodName { get; init; }
    public required List<Route> Routes { get; set; }
    public required bool IsConventional { get; set; }

    public required IEnumerable<SerializedAttribute> Attributes { get; init; }
}

public class Route
{
    public required string Path { get; set; }
    public required List<HTTPMethod> Methods { get; init; }

    public static string RemoveEmptySegments(string path)
    {
        return path.Replace("//", "/");
    }
}

public record SerializedAttribute
{
    public required string Name { get; init; }

    public required Dictionary<string, object> Properties { get; init; }
}
