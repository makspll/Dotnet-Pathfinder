using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public class RouteCalculator(FrameworkVersion version)
{

    private readonly FrameworkVersion _version = version;

    public List<HTTPMethod> DefaultMethods(ControllerKind controllerKind) =>
        (_version == FrameworkVersion.DOTNET_FRAMEWORK && controllerKind == ControllerKind.API) ?
            [HTTPMethod.POST] :
            [.. Enum.GetValues<HTTPMethod>()];

    private Route? CalculateRoute(ActionCandidate action, RoutingAttribute? routeCandidate, PropagatedRoute? propagatedRoute, IEnumerable<PropagatedRoute> propagatedPrefixes)
    {
        string? suffix = null;
        if (routeCandidate != null)
        {
            suffix = routeCandidate.Route(_version) ?? action.PropagatedRoutes.FirstOrDefault(x => !x.FromController)?.Route;
            if (suffix != null && propagatedRoute?.Prefix != null)
                suffix = Join(propagatedRoute?.Prefix, suffix);
        }
        if (suffix == null && propagatedRoute != null)
            suffix = propagatedRoute.Route;
        if (suffix == null)
            return null;

        var prefix = propagatedPrefixes.FirstOrDefault(x => x.Prefix != null)?.Prefix;
        // // If the propagation needs a standalone route, we don't propagate the prefix without one
        // if (suffix == null && propagatedPrefix?.PropagationType == RoutePropagation.PropagateToRoutes)
        //     prefix = null;
        // // if the attribute only propagates to unrouted actions, don't propagate in the case of an existing suffix
        // else if (suffix != null && propagatedPrefix?.PropagationType == RoutePropagation.PropagateToUnrouted)
        //     prefix = null;

        var path = Join(prefix, suffix);

        if (path == null)
            return null;

        return new Route
        {
            Methods = AllowedMethods(action.RoutingAttributes, routeCandidate, action.Method.Name, action.ActionName(_version), action.Controller.Kind),
            Path = path
        };
    }
    public void PopulateRoutes(ActionCandidate action)
    {
        var routes = new List<Route>();

        // if nothing gets propagated we still want to allow routes to generate
        IEnumerable<PropagatedRoute?> propagatedControllerRoutes = action.PropagatedRoutes.Where(x => x.FromController);
        if (!propagatedControllerRoutes.Any())
            propagatedControllerRoutes = [null];

        var propagatedPrefixes = propagatedControllerRoutes.Where(x => x?.Prefix != null && x?.Route == null).OfType<PropagatedRoute>().ToList();
        var propagatedRoutes = propagatedControllerRoutes.Where(x => x?.Route != null).ToList();
        if (propagatedRoutes.Count == 0)
            propagatedRoutes = [null];

        foreach (var controllerPropagation in propagatedRoutes)
        {
            // we still want to allow routes to generate if only the controller contains route information
            IEnumerable<RoutingAttribute?> routeCandidates = action.RoutingAttributes.Where(x => x.CanGenerateRoute(_version));
            if (!routeCandidates.Any(x => x?.Route(_version) != null))
                routeCandidates = [null];
            foreach (var routeCandidate in routeCandidates)
            {
                var route = CalculateRoute(action, routeCandidate, controllerPropagation, propagatedPrefixes);
                if (route != null)
                    routes.Add(route);
            }
        }


        action.Routes = [.. action.Routes, .. CoalesceRoutes(routes)];
    }

    public void PopulateConventionalRoutes(ActionCandidate action, ConventionalRoute template)
    {
        // calculating a conventional routes is simple, we fill in the values of the route template if we match, leave the rest as parameters 
        // https://learn.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/routing-and-action-selection

        var routedController = template.ControllerPart != null ? action.Controller.ControllerName : template.GetPartDefaultValue("controller");
        if (routedController != action.Controller.ControllerName)
            return;

        var routedAction = template.ActionPart != null ? action.ActionName(_version) : template.GetPartDefaultValue("action");
        if (routedAction != action.ActionName(_version))
            return;

        var templateControllerKind = template.Type.ToControllerKind();
        if (_version == FrameworkVersion.DOTNET_FRAMEWORK && templateControllerKind != null && templateControllerKind != action.Controller.Kind)
            return;

        var path = template.InstantiateTemplateWith(routedController, routedAction, null);

        var allowedMethods = AllowedMethods(action.RoutingAttributes, null, action.Method.Name, action.ActionName(_version), action.Controller.Kind);

        action.ConventionalRoutes.Add(
            new Route { Path = path, Methods = allowedMethods }
        );
    }

    static string Join(string? prefix, string? suffix)
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

    List<HTTPMethod> AllowedMethods(IEnumerable<RoutingAttribute> allAttributes, RoutingAttribute? routeSource, string methodName, string actionName, ControllerKind controllerKind)
    {
        var otherMethods = allAttributes.SelectMany(x => x.HttpMethodOverride(_version) ?? []).OfType<HTTPMethod>();
        var sourceMethod = routeSource?.HttpMethodOverride(_version);

        if (sourceMethod != null)
        {
            // the more specific method override takes precedence
            return sourceMethod.ToList();
        }

        if (otherMethods.Any())
        {
            return otherMethods.ToList();
        }

        if (_version == FrameworkVersion.DOTNET_FRAMEWORK && controllerKind == ControllerKind.API)
        {
            // .net framework also uses the name of the action to determine the allowed method
            foreach (var httpMethod in Enum.GetValues<HTTPMethod>())
            {
                // note method name not action name for some reason
                if (methodName.StartsWith(httpMethod.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return [httpMethod];
                }
            }
        }

        return DefaultMethods(controllerKind);
    }
}

