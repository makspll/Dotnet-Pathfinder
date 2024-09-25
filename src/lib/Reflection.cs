using System.Text.Json.Serialization;
using dnlib.DotNet;

namespace Makspll.Pathfinder;

public record AssemblyMetadata
{
    public required string? Name { get; init; }
    public required Version? Version { get; init; }
    public required AssemblyFramework Framework { get; init; }
    public required IEnumerable<ControllerMetadata> Controllers { get; init; }
}

public record ControllerMetadata
{
    public required string Name { get; init; }
    // public required IEnumerable<Attribute> Attributes { get; init; }

    public required IEnumerable<MethodMetadata> Methods { get; init; }
}

public record MethodMetadata
{
    public required string Name { get; init; }

    /*
     * The actual route bound to the controller.
     */
    public required string Route { get; init; }

    /*
     * The route defined by the controller itself. Will be the same as Route if nothing overrides it.
     */
    public required string? AttributeRoute { get; init; }
    // public required IEnumerable<Attribute> Attributes { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssemblyFramework
{
    /// <summary>
    /// First released in 2002
    /// </summary>
    NETFRAMEWORK,  // ASP.NET 1.0, 1.1, 2.0, 3.5, 4.0, 4.5, 4.6, 4.7, 4.8

    /// <summary>
    /// First released in 2019
    /// </summary>
    NETCORE,       // NET Core 1.0, 1.1, 2.0, 2.1, 2.2, 3.0, 3.1, 5.0, 6.0, 7.0, 8.0, 9.0
    NETSTANDARD,   // The various NET standard targetting assemblies
    UNKNOWN,       // OTHER framework or could not be determined

}


public class Pathfinder
{
    public static AssemblyFramework GetAssemblyFramework(AssemblyDef assembly)
    {
        var frameworkName = assembly.CustomAttributes
            .Select(a => a.AttributeType.Name == "TargetFrameworkAttribute" ?
                AttributeReader.GetAttributeConstructorValues(a, ",") :
                null)
            .FirstOrDefault(x => x != null && x != "")?
            .ToLower();

        if (frameworkName == null)
        {
            return AssemblyFramework.UNKNOWN;
        }

        if (frameworkName.Contains("netstandard"))
        {
            return AssemblyFramework.NETSTANDARD;
        }
        else if (frameworkName.Contains("netcoreapp"))
        {
            return AssemblyFramework.NETCORE;
        }
        else if (frameworkName.Contains("netframework"))
        {
            return AssemblyFramework.NETFRAMEWORK;
        }

        return AssemblyFramework.UNKNOWN;
    }

}
