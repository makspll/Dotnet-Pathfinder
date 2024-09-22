using Microsoft.AspNetCore.Routing;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ActionConstraints;


namespace TestUtils;
public record RouteInfo
{
    [JsonPropertyName("methods")]
    public required IEnumerable<string> HttpMethods { get; set; }
    [JsonPropertyName("route")]
    public required string Route { get; set; }
    [JsonPropertyName("action")]
    public string? Action { get; set; }
    [JsonPropertyName("controllerMethod")]
    public string? ControllerMethod { get; set; }

    [JsonPropertyName("expectedRoute")]
    public string? ExpectedRoute { get; set; }

    [JsonPropertyName("expectNoRoute")]
    public bool ExpectNoRoute { get; set; }

    [JsonPropertyName("conventionalRoute")]
    public bool ConventionalRoute { get; set; }
}

public static class PathsExporter
{
    public static List<RouteInfo> ListAllRoutes(IEnumerable<ControllerActionDescriptor> _endpointSources, bool includeConventional = true, bool includeAttributeRoutes = true)
    {
        var output = _endpointSources.Select(
            controller =>
            {
                var action = controller != null
                    ? $"{controller.ControllerName}.{controller.ActionName}"
                    : null;
                var controllerMethod = controller != null
                    ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                    : null;

                var expectRouteAttr = controller?.MethodInfo.GetCustomAttribute<ExpectRouteAttribute>();
                var expectNoRouteAttr = controller?.MethodInfo.GetCustomAttribute<ExpectNoRouteAttribute>();

                var route = controller?.AttributeRouteInfo?.Template;
                string[] allHttpMethods = ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"];
                var httpMethods = controller!.ActionConstraints?.OfType<HttpMethodActionConstraint>().SingleOrDefault()?.HttpMethods ?? allHttpMethods;

                if (route == null && !includeConventional)
                    return null;
                if (route != null && !includeAttributeRoutes)
                    return null;

                if (route != null && !route.StartsWith('/'))
                    route = "/" + route;

                return new RouteInfo()
                {
                    HttpMethods = httpMethods,
                    Route = route!,
                    Action = action,
                    ControllerMethod = controllerMethod,
                    ExpectedRoute = expectRouteAttr?.Path,
                    ConventionalRoute = false,
                    ExpectNoRoute = expectNoRouteAttr != null
                };
            }
        );

        return output.OfType<RouteInfo>().ToList();
    }
}
