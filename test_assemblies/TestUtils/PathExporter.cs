
#if NET47
using System.Web.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

#elif NETCOREAPP

using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System.Collections.Generic;
using System.Linq;
#endif


namespace TestUtils
{


    public class RouteInfo
    {
        public RouteInfo(IEnumerable<string> httpMethods, IEnumerable<string> routes)
        {
            HttpMethods = httpMethods;
            Routes = routes;
        }

        public IEnumerable<string> HttpMethods { get; set; }
        public IEnumerable<string> Routes { get; set; }

        public string? Action { get; set; }

        public string? ActionMethodName { get; set; }

        public string? ControllerName { get; set; }

        public string? ControllerClassName { get; set; }

        public string? ControllerNamespace { get; set; }

    }

#if NETCOREAPP

    public static class PathsExporter
    {
        public static List<RouteInfo> ListAllRoutes(IEnumerable<ControllerActionDescriptor> _endpointSources, IEnumerable<EndpointDataSource> endpointSources, bool includeConventional = true, bool includeAttributeRoutes = true)
        {
            var output = _endpointSources.Select(
                controller =>
                {
                    var action = controller?.ActionName;

                    var controllerName = controller?.ControllerTypeInfo.Name;
                    var controllerClassName = controllerName;
                    if (controllerName?.EndsWith("Controller") ?? false)
                        controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

                    var controllerNamespace = controller?.ControllerTypeInfo.Namespace;
                    var actionMethodName = controller?.MethodInfo.Name;


                    var route = controller?.AttributeRouteInfo?.Template;
                    string[] allHttpMethods = {"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"};
                    var httpMethods = controller!.ActionConstraints?.OfType<HttpMethodActionConstraint>().SingleOrDefault()?.HttpMethods ?? allHttpMethods;

                    if (route == null && !includeConventional)
                        return null;
                    if (route != null && !includeAttributeRoutes)
                        return null;

                    if (route != null && !route.StartsWith("/"))
                        route = "/" + route;


                    List<string> all_routes = route == null ? new List<string>(){} : new List<string>{route};

                    if (route == null)
                    {
                        // figure out which conventional routes route to this controller

                        var all_conventional_routes = endpointSources
                            .SelectMany(e => e.Endpoints)
                            .OfType<RouteEndpoint>()
                            .Where(e =>
                            {
                                var routeControllerName = e.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.Name;
                                var routeActionName = e.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName;
                                if (e.RoutePattern.Defaults.Count == 0 && e.RoutePattern.Parameters.Count == 0)
                                    return false;

                                var hasControllerParameterKey = e.RoutePattern.Parameters.Any(p => p.Name == "controller");
                                var hasActionParameterKey = e.RoutePattern.Parameters.Any(p => p.Name == "action");

                                if (routeActionName == null)
                                {
                                    // try find parameters in the route pattern as well 
                                    routeActionName = (string)(e.RoutePattern.Parameters
                                        .Where(p => p.Name == "action")
                                        .Select(p => p.Default)
                                        .FirstOrDefault() ?? e.RoutePattern.Defaults.Where(p => p.Key == "action").Select(p => p.Value).FirstOrDefault() ?? "");
                                }
                                if (routeControllerName == null)
                                {
                                    // try find parameters in the route pattern as well 
                                    routeControllerName = (string)(e.RoutePattern.Parameters
                                        .Where(p => p.Name == "controller")
                                        .Select(p => p.Default)
                                        .FirstOrDefault() ?? e.RoutePattern.Defaults.Where(p => p.Key == "controller").Select(p => p.Value).FirstOrDefault() ?? "");
                                }


                                return controllerName == routeControllerName && routeActionName == action;

                            })
                            .Select(e => e.RoutePattern.RawText)
                            .ToList();
                        all_routes = all_conventional_routes!;
                    }

                    return new RouteInfo(httpMethods,all_routes)
                    {
                        Action = action,
                        ActionMethodName = actionMethodName,
                        ControllerName = controllerName,
                        ControllerNamespace = controllerNamespace,
                        ControllerClassName = controllerClassName
                    };
                }
            );

            return output.OfType<RouteInfo>().ToList();
        }
    }
#elif NET47
    public static class PathExporter
    {
        public static List<RouteInfo> ListAllRoutes(RouteCollection config, bool includeConventional = true, bool includeAttributeRoutes = true)
        {
            // get all attribute routes
            return config
                .OfType<Route>()
                .Select(r =>
                    {
                        return new RouteInfo(new List<string>(), new List<string> { r.Url })
                        {

                        };
                    }
                ).ToList();
        }
    }


#endif

}