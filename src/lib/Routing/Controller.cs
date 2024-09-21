namespace Makspll.Pathfinder.Routing;

/// <summary>
///  Represents the concept of a WEB controller in a .NET assembly
/// </summary>
public class Controller
{
    /// <summary>
    /// The name of the controller
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The namespace the controller lives in
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Prefix applied to all routes in the controller
    /// </summary>
    public string? Prefix { get; init; }

    /// <summary>
    /// The actions that the controller is responsible for
    /// </summary>
    public required IEnumerable<Action> Actions { get; init; }

    /// <summary>
    /// Attributes marking up the controller
    /// </summary>
    public required IEnumerable<RoutingAttribute> Attributes { get; init; }
}