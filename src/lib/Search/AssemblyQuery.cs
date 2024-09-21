using dnlib.DotNet;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public class AssemblyQuery
{
    readonly ModuleDefMD LoadedModule;

    public AssemblyQuery(string dll)
    {
        var moduleContext = ModuleDef.CreateModuleContext();
        LoadedModule = ModuleDefMD.Load(dll, moduleContext);
    }

    public AssemblyQuery(ModuleDefMD module)
    {
        LoadedModule = module;
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
    static IEnumerable<Route> CoalesceRoutes(IEnumerable<Route> routes)
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

    static List<Routing.Action> FindActions(IEnumerable<MethodDef> methods, string? propagatedPrefix)
    {
        var actions = new List<Routing.Action>();
        foreach (var method in methods)
        {
            if (method.IsConstructor || method.IsGetter || method.IsSetter || method.IsStatic || method.IsAbstract)
            {
                continue;
            }

            var routingAttrs = method.CustomAttributes
                .Select(AttributeParser.ParseAttribute)
                .OfType<RoutingAttribute>()
                .ToList();

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

            var action = new Routing.Action
            {
                Name = method.Name,
                Routes = coalescedRoutes,
                Attributes = routingAttrs
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
    * Finds all controllers, or if names are provided, finds the controllers matching those conventional names
    **/
    public IEnumerable<Controller> FindControllers(params string[] names)
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

            var actions = FindActions(type.Methods, routePrefixes.FirstOrDefault());

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


    // public IEnumerable<Controller> FindControllersWithConventionalRouting(IEnumerable<ConventionalRoute> conventionalRoutes)
    // {
    // conventional routing exposes controllers either via:
    // 1. a {controller} route parameter
    // 2. a route template with a default pointing to the controller, i.e. 'api/myroute' with a default of 'TestController'
    // we need to do 2 things, find all controllers through those avenues
    // and then replace all parameter names with concrete values, if we're left with any parameters, we leave them in the route
    // foreach (var route in conventionalRoutes)
    // {
    // var controllers = new List<Controller>();
    // var 
    // // first check if we have a {controller} part
    // var controllerPart = route.Template.FirstOrDefault(x => x.PartName == "controller");
    // if (controllerPart != null)
    // {

    // }
    // else
    // {
    //     // if we don't rely on the defaults for the route 
    //     route.Defaults.TryGetValue("controller", out var controllerName);

    //     if (controllerName != null)
    //     {
    //         var controller = FindControllers(controllerName).FirstOrDefault();
    //         if (controller != null)
    //         {
    //             controllers.Add(controller);
    //         }
    //     }
    // }
    // }
    // }
}