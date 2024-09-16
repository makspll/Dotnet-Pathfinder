using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text.Json.Serialization;
using System.Reflection;

namespace TestUtils;
public record RouteInfo
{
    [JsonPropertyName("method")]
    public string? Method { get; set; }
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
    public static List<RouteInfo> ListAllRoutes(IEnumerable<EndpointDataSource> _endpointSources)
    {
        var endpoints = _endpointSources
                   .SelectMany(es => es.Endpoints)
                   .OfType<RouteEndpoint>();
        var output = endpoints.Select(
            e =>
            {
                var controller = e.Metadata
                    .OfType<ControllerActionDescriptor>()
                    .FirstOrDefault();
                var action = controller != null
                    ? $"{controller.ControllerName}.{controller.ActionName}"
                    : null;
                var controllerMethod = controller != null
                    ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
                    : null;

                var expectRouteAttr = controller?.MethodInfo.GetCustomAttribute<ExpectRouteAttribute>();
                var expectNoRouteAttr = controller?.MethodInfo.GetCustomAttribute<ExpectNoRouteAttribute>();

                return new RouteInfo()
                {
                    Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
                    Route = $"/{e.RoutePattern.RawText!.TrimStart('/')}",
                    Action = action,
                    ControllerMethod = controllerMethod,
                    ExpectedRoute = expectRouteAttr?.Path,
                    ConventionalRoute = expectRouteAttr?.Conventional ?? controllerMethod == null,
                    ExpectNoRoute = expectNoRouteAttr != null
                };
            }
        );

        return output.ToList();
    }
}
