using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public class RouteCalculator(FrameworkVersion version)
{

    private readonly FrameworkVersion _version = version;

    private Route? CalculateRoute(ActionCandidate action, RoutingAttribute? routeCandidate, string? propagatedPrefix)
    {
        string? suffix = null;
        if (routeCandidate != null)
        {
            suffix = routeCandidate.Route(_version) ?? action.PropagatedRoutes.FirstOrDefault(x => !x.FromController)?.Prefix;
        }

        var path = Join(propagatedPrefix, suffix);

        if (string.IsNullOrEmpty(path))
            return null;

        return new Route
        {
            Methods = AllowedMethods(action.RoutingAttributes, routeCandidate),
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

        foreach (var controllerPropagation in propagatedControllerRoutes)
        {
            var propagatedPrefix = controllerPropagation?.Prefix;

            // we still want to allow routes to generate if only the controller contains route information
            IEnumerable<RoutingAttribute?> routeCandidates = action.RoutingAttributes.Where(x => x.CanGenerateRoute(_version));
            if (!routeCandidates.Any(x => x?.Route(_version) != null))
                routeCandidates = [null];
            foreach (var routeCandidate in routeCandidates)
            {
                if (routeCandidate?.Route(_version) == null && controllerPropagation?.PropagationType == RoutePropagation.OnlyPropagatedToAlreadyRoutedActions)
                {
                    continue;
                }

                var route = CalculateRoute(action, routeCandidate, propagatedPrefix);
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

        var routedController = template.ControllerPart != null ? action.Controller.ControllerName : template.Defaults?.GetValueOrDefault("controller");
        if (routedController != action.Controller.ControllerName)
            return;

        var routedAction = template.ActionPart != null ? action.ActionName(_version) : template.Defaults?.GetValueOrDefault("action");
        if (routedAction != action.ActionName(_version))
            return;

        var path = template.InstantiateTemplateWith(routedController, routedAction, null);

        var allowedMethods = AllowedMethods(action.RoutingAttributes, null);

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

    List<HTTPMethod> AllowedMethods(IEnumerable<RoutingAttribute> allAttributes, RoutingAttribute? routeSource)
    {
        var otherMethods = allAttributes.SelectMany(x => x.HttpMethodOverride(_version) ?? []).OfType<HTTPMethod>();
        var sourceMethod = routeSource?.HttpMethodOverride(_version);

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
}

