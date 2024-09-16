using dnlib.DotNet;
using Makspll.ReflectionUtils.Parsing;
using Makspll.ReflectionUtils.Routing;

namespace Makspll.ReflectionUtils.Search;

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
                route = route.Substring(0, route.Length - 1);
            }
            return route;
        }
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

            var routeSuffix = routingAttrs.FindAll(x => x.Route() != null).Select(x => x.Route()).ToList();


            var routes = routeSuffix.Select(s => JoinRoutes(propagatedPrefix, s)).Where(r => r != "").ToList();

            if (routes.Count == 0)
            {
                // if no route is specified, we can still get routable actions via propagated prefix
                if (string.IsNullOrEmpty(propagatedPrefix))
                    continue;
                else
                    routes.Add(JoinRoutes(propagatedPrefix, ""));
            }

            var action = new Routing.Action
            {
                Name = method.Name,
                Routes = routes,
                Attributes = routingAttrs
            };

            actions.Add(action);
        }
        return actions;
    }

    static public bool IsController(TypeDef type, IEnumerable<RoutingAttribute> attributes)
    {
        if (attributes.Any(x => x is ApiControllerAttribute))
        {
            return true;
        }

        if (type.BaseType != null && (type.BaseType.Name == "Controller" || type.BaseType.Name == "ApiController" || type.BaseType.Name == "ControllerBase"))
        {
            return true;
        }

        return false;
    }

    public IEnumerable<Controller> FindControllers()
    {
        var types = LoadedModule.GetTypes();
        var controllers = new List<Controller>();
        foreach (var type in types)
        {


            // first of all figure out if this is a valid controller
            var attributes = type.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>().ToList();
            var isController = IsController(type, attributes);

            if (!isController)
            {
                continue;
            }

            // figure out if the controller has a route it propagates to its actions

            var routePrefixes = attributes.Where(x => x.Propagation() == RoutePropagation.PropagateToActions && x.Route() != null).Select(x => x.Route());

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
}