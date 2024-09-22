using System.Text.Json;
using dnlib.DotNet;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.RoutingConfig;

namespace Makspll.Pathfinder.Search;

public class AssemblyQuery(ModuleDefMD module, IEnumerable<ConventionalRoute>? config = null)
{
    readonly ModuleDefMD LoadedModule = module;
    readonly IEnumerable<ConventionalRoute>? config = config;

    public AssemblyQuery(string dll) : this(ModuleDefMD.Load(dll, ModuleDef.CreateModuleContext()), FindAndParseNearestConfig(dll)) { }

    /// <summary>
    /// Finds and parses the nearest pathfinder.json file in the directory tree starting from the given directory. Returns null if no file is found.
    /// </summary>
    public static IEnumerable<ConventionalRoute>? FindAndParseNearestConfig(string dll)
    {
        var dllDirectory = Path.GetDirectoryName(dll);
        var configPath = FileSearch.FindNearestFile("pathfinder.json", dllDirectory ?? dll);
        if (configPath == null)
            return null;

        var config = JsonSerializer.Deserialize<PathfinderConfig>(File.ReadAllText(configPath.FullName));

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

        var otherMethods = allAttributes.Select(x => x.HttpMethodOverride()).OfType<HTTPMethod>();
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
            return [sourceMethod.Value];
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
        var propagatedSuffix = routingAttrs.FirstOrDefault(x => x.Propagation() == RoutePropagation.Propagate)?.Route();

        var httpMethods = routingAttrs.Select(x => x.HttpMethodOverride()).OfType<HTTPMethod>();

        var routes = routingAttrs.Select(s =>
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

    static IEnumerable<MethodDef> EnumerateMethodsWhichCouldBeActions(IEnumerable<MethodDef> methods, bool excludePrivate = true)
    {
        foreach (var method in methods)
        {
            if (method.IsConstructor || method.IsGetter || method.IsSetter || method.IsStatic || method.IsAbstract || (excludePrivate && !method.IsPublic))
                continue;
            yield return method;
        }
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

    static List<Routing.Action> FindConventionalActions(ConventionalRoute route, TypeDef controllerType, IEnumerable<MethodDef> methods)
    {
        // calculating a conventional routes is simple, we fill in the values of the route template if we match, leave the rest as parameters 
        // https://learn.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/routing-and-action-selection

        var controller = route.Controller;
        var action = route.Action;
        var area = route.Area;

        var controllerRoutingAttrs = controllerType.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList();

        var areaAttribute = controllerRoutingAttrs.Select(x => x.Area()).OfType<string>().FirstOrDefault();
        var areaName = areaAttribute ?? area?.DefaultValue ?? "";
        var finalActions = new List<Routing.Action>();
        // ignore defaults we handle those somewhere else
        // actions can also override their name via the ActionName attribute, as well as exclude themselves from routing by using the NonAction attribute
        if (controller != null && action != null)
        {
            foreach (var method in EnumerateMethodsWhichCouldBeActions(methods))
            {
                var actionNameOverride = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().Select(x => x.ActionName()).OfType<string>().FirstOrDefault();

                string finalRoute = route.InstantiateTemplateWith(controllerType.Name, actionNameOverride ?? method.Name, areaName);
                var routingAttrs = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList();
                finalActions.Add(new Routing.Action
                {
                    Name = method.Name,
                    Routes =
                    [
                        new() {
                            Path = finalRoute,
                            Methods = AllowedMethods(routingAttrs, null)
                        }
                    ],
                    IsConventional = true,
                    Attributes = routingAttrs
                });
            }

            // the methods 
        }
        else if (controller != null)
        {
            // if we only have a controller, we use HTTP verbs to identify the action
            // for example a GET request to /api/test would be routed to any action/method whose name is prefixed with the 'Get' value in its name
            // if we don't find a matching verb we default to POST 
            foreach (var method in EnumerateMethodsWhichCouldBeActions(methods))
            {
                var actionNameOverride = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().Select(x => x.ActionName()).OfType<string>().FirstOrDefault();
                var allowedMethod = ActionNameToVerb(actionNameOverride ?? method.Name) ?? HTTPMethod.POST;

                string finalRoute = route.InstantiateTemplateWith(controllerType.Name, actionNameOverride ?? method.Name, areaName);

                finalActions.Add(new Routing.Action
                {
                    Name = method.Name,
                    Routes =
                    [
                        new() {
                            Path = finalRoute,
                            Methods = [allowedMethod]
                        }
                    ],
                    IsConventional = true,
                    Attributes = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList()
                });
            }
        }

        return finalActions;
    }

    static List<Routing.Action> FindActions(IEnumerable<MethodDef> methods, string? propagatedPrefix)
    {
        var actions = new List<Routing.Action>();
        foreach (var method in EnumerateMethodsWhichCouldBeActions(methods))
        {

            var routingAttrs = method.CustomAttributes
                .Select(AttributeParser.ParseAttribute)
                .OfType<RoutingAttribute>()
                .ToList();

            List<Route> routes = CalculateAttributeRoutes(routingAttrs, propagatedPrefix);

            var action = new Routing.Action
            {
                Name = method.Name,
                Routes = routes,
                Attributes = routingAttrs,
                IsConventional = false
            };

            actions.Add(action);
        }

        return actions;
    }

    /**
     * Recursively traverses the inheritance tree to find the given base type, if the assembly is not loaded it will not find all base types
     */
    static public bool InheritsFrom(ITypeDefOrRef? type, string basetype)
    {
        if (type == null)
            return false;
        else
        {
            var basetypeType = type.GetBaseType();
            if (basetypeType == null)
                return false;
            else if (basetypeType.Name.String == basetype)
                return true;
            else
                return InheritsFrom(basetypeType, basetype);
        }
    }

    static public bool IsController(TypeDef type, IEnumerable<RoutingAttribute> attributes)
    {
        if (type.IsAbstract)
            return false;

        if (attributes.Any(x => x is ApiControllerAttribute))
        {
            return true;
        }

        if (InheritsFrom(type, "Controller") || InheritsFrom(type, "ControllerBase"))
        {
            return true;
        }

        return false;
    }

    public IEnumerable<TypeDef> EnumerateControllerTypes()
    {
        foreach (var type in LoadedModule.GetTypes())
        {
            var attributes = type.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList();
            if (IsController(type, attributes))
            {
                yield return type;
            }
        }
    }

    /**
    * Finds all controllers, or if names are provided, finds the controllers matching those conventional names.
    * If a conventional route template is passed, will ignore attribute routing and generate routes based on the template.
    **/
    List<Controller> FindControllers(ConventionalRoute? conventionalRoute = null, params string[] names)
    {
        var types = LoadedModule.GetTypes();
        var controllers = new List<Controller>();
        foreach (var type in EnumerateControllerTypes())
        {
            if (names.Length > 0 && !names.Any(x => x == type.Name || x == $"{type.Name}Controller"))
            {
                continue;
            }

            // figure out if the controller has a route it propagates to its actions
            var attributes = type.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList();
            var routePrefixes = attributes.Where(x => x.Propagation() == RoutePropagation.Propagate && x.Route() != null).Select(x => x.Route());

            if (routePrefixes.Count() > 1)
            {
                throw new Exception($"Multiple route prefixes found on controller: {type.Name}. Prefixes: {string.Join(", ", routePrefixes)}");
            }

            List<Routing.Action> actions;

            if (conventionalRoute != null)
            {
                actions = FindConventionalActions(conventionalRoute, type, type.Methods);
            }
            else
            {
                actions = FindActions(type.Methods, routePrefixes.FirstOrDefault());
            }

            var controller = new Controller
            {
                Name = type.Name,
                Namespace = type.Namespace,
                Prefix = routePrefixes.FirstOrDefault(),
                Actions = actions,
                Attributes = attributes
            };
            controllers.Add(controller);
        }
        return controllers;
    }


    IEnumerable<Controller> FindConventionalControllers()
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
            controllers.AddRange(FindControllers(route).ToList());
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
            var existingController = attributeControllers.FirstOrDefault(x => x.Name == controller.Name && x.Namespace == controller.Namespace);
            if (existingController == null)
            {
                attributeControllers.Add(controller);
            }
            else
            {
                // don't add conventional routes for actions routed via attribute routing
                foreach (var action in controller.Actions)
                {
                    var existingAction = existingController.Actions.FirstOrDefault(x => x.Name == action.Name);
                    if (existingAction == null)
                    {
                        existingController.Actions.Add(action);
                    }
                    else
                    {
                        // merge the routes if none exist yet
                        if (existingAction.Routes.Count == 0)
                        {
                            existingAction.IsConventional = true;
                            existingAction.Routes.AddRange(action.Routes);
                        }
                    }
                }
            }
        }

        return attributeControllers;
    }
}