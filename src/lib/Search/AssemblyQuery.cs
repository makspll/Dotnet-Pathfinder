using System.Text.Json;
using ANSIConsole;
using dnlib.DotNet;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.RoutingConfig;

namespace Makspll.Pathfinder.Search;

public enum QueryFeature
{
    AutomaticActionDiscoveryWhenNoParameterOrDefault
}

public class AssemblyQuery(ModuleDefMD module, IEnumerable<ConventionalRoute>? config = null)
{
    readonly ModuleDefMD LoadedModule = module;
    readonly IEnumerable<ConventionalRoute>? config = config;

    IEnumerable<QueryFeature> SearchFeatures = [];

    public AssemblyQuery(string dll, IEnumerable<ConventionalRoute>? routes = null) : this(ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext()), routes ?? FindAndParseNearestConfig(dll)) { }


    public static IEnumerable<ConventionalRoute>? ParseConfig(FileInfo configFile)
    {
        if (!configFile.Exists)
            return null;

        var config = JsonSerializer.Deserialize<PathfinderConfig>(File.ReadAllText(configFile.FullName));

        if (config == null)
            return null;

        var results = config.ConventionalRoutes.Select(x => ConventionalRoute.Parse(x.Template, x.Defaults)).ToList();
        if (results == null)
            return null;

        var failedResults = results.Where(x => x.IsFailed).Select(x => x.Errors).ToList();

        if (failedResults.Count > 0)
        {
            throw new Exception($"Encountered errors when parsing templates: {string.Join('\n', failedResults)}");
        }

        return results.Select(x => x.Value);
    }

    /// <summary>
    /// Finds and parses the nearest pathfinder.json file in the directory tree starting from the given directory. Returns null if no file is found.
    /// </summary>
    public static IEnumerable<ConventionalRoute>? FindAndParseNearestConfig(string dll)
    {
        var dllDirectory = Path.GetDirectoryName(dll);
        var configPath = FileSearch.FindNearestFile("pathfinder.json", dllDirectory ?? dll);
        if (configPath == null)
            return null;

        return ParseConfig(configPath);
    }

    static string JoinRoutes(string? prefix, string? suffix)
    {
        var cleanPrefix = prefix?.Trim('/') ?? "";
        var cleanSuffix = suffix?.Trim('/') ?? "";

        if (cleanPrefix == "" && cleanSuffix == "")
        {
            return "";
        }
        else
        {
            var route = $"{cleanPrefix}/{cleanSuffix}";
            if (!route.StartsWith('/'))
            {
                route = $"/{route}";
            }
            if (route.EndsWith('/'))
            {
                route = route[..^1];
            }
            return route;
        }
    }

    /**
     * Returns the allowed methods for a given route based on the attributes and the attribute providing the current route
     */
    static List<HTTPMethod> AllowedMethods(IEnumerable<RoutingAttribute> allAttributes, RoutingAttribute? routeSource)
    {
        var allExcludingSource = allAttributes.Where(x => x != routeSource);

        var otherMethods = allAttributes.SelectMany(x => x.HttpMethodOverride() ?? []).OfType<HTTPMethod>();
        var sourceMethod = routeSource?.HttpMethodOverride();

        if (sourceMethod == null)
        {
            if (otherMethods.Any())
            {
                return otherMethods.ToList();
            }
        }
        else
        {
            // the more specific method override takes precedence
            return sourceMethod.ToList();
        }

        return [.. Enum.GetValues<HTTPMethod>()];
    }

    /**
    * Coalesces routes that have the same path and merges their methods
    */
    static List<Route> CoalesceRoutes(IEnumerable<Route> routes)
    {
        var coalescedRoutes = new List<Route>();

        var groupedRoutes = routes.GroupBy(x => x.Path);
        foreach (var group in groupedRoutes)
        {
            var methods = group.SelectMany(x => x.Methods).Distinct().ToList();

            coalescedRoutes.Add(new Route
            {
                Path = group.First().Path,
                Methods = methods
            });
        }

        return coalescedRoutes;
    }

    static List<Route> CalculateAttributeRoutes(IEnumerable<RoutingAttribute> routingAttrs, string? propagatedPrefix)
    {

        // allow other routing attributes to propagate their suffix to this one if it's empty
        var propagatedSuffix = routingAttrs.FirstOrDefault(x => x.Propagation() == RoutePropagation.Propagate && x.Route() != null)?.Route();

        var httpMethods = routingAttrs.SelectMany(x => x.HttpMethodOverride() ?? []).OfType<HTTPMethod>();

        var routes = routingAttrs
            .Where(a => a.CanGenerateRoute())
            .Select(s =>
                {
                    var suffix = s.Route();
                    if (suffix == null && propagatedSuffix != null)
                    {
                        suffix = propagatedSuffix;
                    }

                    var route = JoinRoutes(propagatedPrefix, suffix);
                    var allowedMethods = AllowedMethods(routingAttrs, s);
                    return route == "" ? null : new Route
                    {
                        Methods = allowedMethods,
                        Path = route,
                    };
                }).OfType<Route>().ToList();

        // if no routing attrs and a propagated prefix is present, add a route from the propagated prefix
        if (routes.Count == 0 && propagatedPrefix != null)
        {
            routes.Add(new Route
            {
                Methods = AllowedMethods(routingAttrs, null),
                Path = JoinRoutes(propagatedPrefix, null)
            });
        }

        var coalescedRoutes = CoalesceRoutes(routes);
        return coalescedRoutes.ToList();
    }

    static HTTPMethod? ActionNameToVerb(string name)
    {
        foreach (var verb in Enum.GetNames<HTTPMethod>())
        {
            // title case the verb 
            var titleCaseVerb = verb.ToString()[0].ToString().ToUpper() + verb.ToString()[1..].ToLower();
            if (name.StartsWith(titleCaseVerb))
            {
                return Enum.Parse<HTTPMethod>(verb);
            }
        }
        return null;
    }

    List<Routing.Action> FindConventionalActions(ConventionalRoute route, ControllerCandidate controllerCandidate)
    {
        // calculating a conventional routes is simple, we fill in the values of the route template if we match, leave the rest as parameters 
        // https://learn.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/routing-and-action-selection

        var controller = route.Controller;
        var controllerDefault = controller?.DefaultValue ?? route.Defaults?.GetValueOrDefault("controller") ?? null;
        var controllerName = Controller.ParseControllerName(controllerCandidate.Type.Name);

        var action = route.Action;
        var actionDefault = action?.DefaultValue ?? route.Defaults?.GetValueOrDefault("action");

        var area = route.Area;
        var areaDefault = area?.DefaultValue ?? route.Defaults?.GetValueOrDefault("area");


        var areaAttribute = controllerCandidate.RoutingAttributes.Select(x => x.Area()).OfType<string>().FirstOrDefault();
        var areaName = areaAttribute ?? area?.DefaultValue ?? null; // TODO; areas
        var finalActions = new List<Routing.Action>();

        var routedController = controller == null ? controllerDefault : controllerName;
        if (routedController != controllerName)
        {
            return finalActions;
        }

        foreach (var actionCandidate in ActionFinder.FindConventionalActions(controllerCandidate))
        {
            var actionNameOverride = actionCandidate.RoutingAttributes.FirstOrDefault(x => x.ActionName() != null)?.ActionName();
            var actionName = actionNameOverride ?? actionCandidate.Method.Name;
            string instantiatedRoute = route.InstantiateTemplateWith(controllerName, actionName, areaName);

            var routedAction = action == null ? actionDefault : actionName;
            var routedArea = area == null ? areaDefault : areaName;

            if (routedAction != actionName || routedArea != areaName)
            {
                continue;
            }

            List<HTTPMethod> allowedMethods;
            if (routedController != null && routedAction == null && SearchFeatures.Contains(QueryFeature.AutomaticActionDiscoveryWhenNoParameterOrDefault))
            {
                // old school ASP.NET framework MVC style routing
                allowedMethods = [ActionNameToVerb(actionName) ?? HTTPMethod.POST];
            }
            else if (routedController != null && routedAction != null)
            {
                // normal routing with either fully specified {controller} and {action} parameters or defaults if these are missing
                allowedMethods = AllowedMethods(actionCandidate.RoutingAttributes, null);
            }
            else
            {
                // we can't match up the route to the controller/action
                continue;
            }

            finalActions.Add(new Routing.Action
            {
                MethodName = actionCandidate.Method.Name,
                Routes =
                [
                    new() {
                            Path = instantiatedRoute,
                            Methods = [.. allowedMethods],
                        }
                ],
                IsConventional = true,
                Attributes = actionCandidate.RoutingAttributes
            });

        }


        return finalActions;
    }

    static List<Routing.Action> FindActions(ControllerCandidate controller, string? propagatedPrefix)
    {
        var actions = new List<Routing.Action>();
        foreach (var method in ActionFinder.FindActions(controller))
        {

            List<Route> routes = CalculateAttributeRoutes(method.RoutingAttributes, propagatedPrefix);

            var action = new Routing.Action
            {
                MethodName = method.Method.Name,
                Routes = routes,
                Attributes = method.RoutingAttributes,
                IsConventional = false
            };

            actions.Add(action);
        }

        return actions;
    }

    List<Controller> FindControllers(ConventionalRoute? conventionalRoute = null)
    {
        var types = LoadedModule.GetTypes();
        var controllers = new List<Controller>();
        foreach (var controllerCandidate in ControllerFinder.FindControllers(LoadedModule))
        {
            // figure out if the controller has a route it propagates to its actions
            var routePrefixes = controllerCandidate.RoutingAttributes.Where(x => x.Propagation() == RoutePropagation.Propagate && x.Route() != null).Select(x => x.Route());
            if (!routePrefixes.Any())
                routePrefixes = [null];

            List<Routing.Action> actions = [];

            if (conventionalRoute != null)
            {
                actions.AddRange(FindConventionalActions(conventionalRoute, controllerCandidate));
            }
            else
            {

                foreach (var routePrefix in routePrefixes)
                {
                    foreach (var newAction in FindActions(controllerCandidate, routePrefix))
                    {
                        var existing = actions.FirstOrDefault(x => x.MethodName == newAction.MethodName);
                        if (existing == null)
                            actions.Add(newAction);
                        else
                            existing.Routes.AddRange(newAction.Routes);
                    }
                }
            }

            var controller = new Controller
            {
                ControllerName = Controller.ParseControllerName(controllerCandidate.Type.Name),
                ClassName = controllerCandidate.Type.Name,
                Namespace = controllerCandidate.Type.Namespace,
                Prefix = routePrefixes.FirstOrDefault(),
                Actions = actions,
                Attributes = controllerCandidate.RoutingAttributes
            };
            controllers.Add(controller);
        }
        return controllers;
    }


    List<Controller> FindConventionalControllers()
    {
        // conventional routing exposes controllers either via:
        // 1. a {controller} route parameter
        // 2. a route template with a default pointing to the controller, i.e. 'api/myroute' with a default of 'TestController'
        // we need to do 2 things, find all controllers through those avenues
        // and then replace all parameter names with concrete values, if we're left with any parameters, we leave them in the route
        var controllers = new List<Controller>();

        if (config == null)
            return controllers;

        foreach (var route in config)
        {
            controllers.AddRange([.. FindControllers(route)]);
        }

        return controllers;
    }

    public IEnumerable<Controller> FindAllControllers()
    {
        var attributeControllers = FindControllers().ToList();
        var conventionalControllers = FindConventionalControllers();

        // merge the two lists

        foreach (var controller in conventionalControllers)
        {
            var existingController = attributeControllers.FirstOrDefault(x => x.ClassName == controller.ClassName && x.Namespace == controller.Namespace);
            if (existingController == null)
            {
                attributeControllers.Add(controller);
            }
            else
            {
                // don't add conventional routes for actions routed via attribute routing
                foreach (var action in controller.Actions)
                {
                    var existingAction = existingController.Actions.FirstOrDefault(x => x.MethodName == action.MethodName);
                    if (existingAction == null)
                    {
                        existingController.Actions.Add(action);
                    }
                    else
                    {
                        // merge the routes if none attribute routes are present
                        if (existingAction.Routes.Count == 0 || existingAction.IsConventional)
                        {
                            existingAction.IsConventional = true;
                            existingAction.Routes.AddRange(action.Routes);
                        }
                    }
                }
            }
        }

        PostProcess.PlaceholderInliner.InlinePlaceholders(attributeControllers);

        return attributeControllers;
    }
}