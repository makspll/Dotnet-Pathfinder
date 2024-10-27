namespace Makspll.Pathfinder.Routing;

/// <summary>
///  Represents the concept of a WEB controller in a .NET assembly
/// </summary>
public class Controller
{
    /// <summary>
    /// The name of the controller. This is the name of the class without the "Controller" suffix if one exists
    /// </summary>
    public required string ControllerName { get; init; }

    /// <summary>
    /// The class name of the controller
    /// </summary>
    public required string ClassName { get; init; }

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
    public required List<Action> Actions { get; init; }

    /// <summary>
    /// Attributes marking up the controller
    /// </summary>
    public required List<SerializedAttribute> Attributes { get; init; }

    public static string ParseControllerName(string className)
    {
        if (className.EndsWith("Controller"))
        {
            return className[..^"Controller".Length];
        }
        return className;
    }
}

