using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Makspll.Pathfinder.Routing;

[JsonConverter(typeof(StringEnumConverter))]
public enum HTTPMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH,
    HEAD,
    OPTIONS,
}

public static class HTTPMethodExtensions
{
    public static HttpMethod ToHttpMethod(this HTTPMethod method)
    {
        return new HttpMethod(method.ToString());
    }

    public static string ToVerbString(this HTTPMethod method)
    {
        var stringVerb = method.ToString();
        return stringVerb[0].ToString().ToUpper() + stringVerb[1..].ToLower();
    }
}


/// Tagged union of attributes relating to routing
public abstract class RoutingAttribute(string name)
{
    public string Name { get; set; } = name;

    public virtual Dictionary<string, object?> Properties(FrameworkVersion version) => new(){
        { "Route", Route(version) },
        { "HttpMethod", HttpMethodOverride(version)?.Select(x => x.ToString()).ToList() },
        { "Area", Area(version) },
        { "ActionName", ActionName(version) },
    };

    public SerializedAttribute IntoSerializedAttribute(FrameworkVersion version) => new()
    {
        Name = Name,
        Properties = Properties(version).Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value)!
    };

    /// <summary>
    /// If true seeing this attribute on a class marks it as a controller and makes it participate in route resolution.
    /// </summary>
    public virtual bool EnablesController(FrameworkVersion version) => false;


    /// <summary>
    /// If the attribute has a route attached to it, returns it
    /// </summary>
    public abstract string? Route(FrameworkVersion version);

    /// <summary>
    /// If the attribute has a route prefix attached to it, returns it
    /// </summary>
    public virtual string? RoutePrefix(FrameworkVersion version) => null;


    /// <summary>
    /// If an attribute can generate a route if it either has a route or gets one propagated to it. If false will never generate a route
    /// </summary>
    public virtual bool CanGenerateRoute(FrameworkVersion version) => Route(version) != null;

    /// <summary>
    /// If true will propagate the attribute route and/or prefix to actions
    /// </summary>
    public virtual bool PropagateToActions(FrameworkVersion version) => false;

    /// <summary>
    /// If the attribute overrides the HTTP method, return it
    /// </summary>
    public virtual IEnumerable<HTTPMethod>? HttpMethodOverride(FrameworkVersion version) => null;

    /// <summary>
    /// If the attribute defines an area for a controller, return it
    /// </summary>
    public virtual string? Area(FrameworkVersion version) => null;

    /// <summary>
    /// If the attribute overrides the action name, return it
    /// </summary>
    public virtual string? ActionName(FrameworkVersion version) => null;

    public virtual bool DisablesConventionalRoutes(FrameworkVersion version) => false;
}

public class ApiControllerAttribute : RoutingAttribute
{
    public ApiControllerAttribute() : base("ApiController") { }

    public override string? Route(FrameworkVersion version) => null;

    public override bool EnablesController(FrameworkVersion version) => true;
}

public class RouteAttribute(string? path) : RoutingAttribute("Route")
{
    public string? Path { get; init; } = path;

    public override string? Route(FrameworkVersion version) => Path;

    public override string? RoutePrefix(FrameworkVersion version) => version != FrameworkVersion.DOTNET_FRAMEWORK ? Path : null;

    // in .NET Framework, Route attributes do not act like prefixes, they are standalone routes at controller level only
    public override bool PropagateToActions(FrameworkVersion version) => true;

    public override bool CanGenerateRoute(FrameworkVersion version) => true;
}

public class RoutePrefixAttribute(string? prefix) : RoutingAttribute("RoutePrefix")
{
    public string? Prefix { get; init; } = prefix;

    public override string? Route(FrameworkVersion version) => null;
    public override string? RoutePrefix(FrameworkVersion version) => Prefix;

    public override bool CanGenerateRoute(FrameworkVersion version) => false;
    public override bool PropagateToActions(FrameworkVersion version) => true;
}

public class HttpAttribute(HTTPMethod method, string? route) : RoutingAttribute($"Http{char.ToUpper(method.ToString()[0])}{method.ToString()[1..].ToLower()}")
{
    public HTTPMethod Method { get; init; } = method;
    public string? Path { get; init; } = route;
    public override string? Route(FrameworkVersion version) => Path;

    public override bool CanGenerateRoute(FrameworkVersion version) => true;

    public override IEnumerable<HTTPMethod>? HttpMethodOverride(FrameworkVersion version) => [Method];
}

public class AreaAttribute(string? area) : RoutingAttribute("Area")
{
    public string? AreaValue { get; init; } = area;

    public override string? Route(FrameworkVersion version) => null;

    public override string? Area(FrameworkVersion version) => AreaValue;
}


public class ActionNameAttribute(string? name) : RoutingAttribute("ActionName")
{
    public string? ActionNameValue { get; init; } = name;

    public override string? Route(FrameworkVersion version) => null;

    public override string? ActionName(FrameworkVersion version) => ActionNameValue;
}

public class NonActionAttribute : RoutingAttribute
{
    public NonActionAttribute() : base("NonAction") { }

    public override string? Route(FrameworkVersion version) => null;

    public override bool DisablesConventionalRoutes(FrameworkVersion version) => true;
}

public class AcceptVerbsAttribute(IEnumerable<HTTPMethod>? methods) : RoutingAttribute("AllowedMethods")
{
    public IEnumerable<HTTPMethod>? Methods { get; init; } = methods;
    public string? RouteValue { get; init; }

    public override string? Route(FrameworkVersion version) => RouteValue;

    public override bool CanGenerateRoute(FrameworkVersion version) => true;

    public override IEnumerable<HTTPMethod>? HttpMethodOverride(FrameworkVersion version) => Methods;
}