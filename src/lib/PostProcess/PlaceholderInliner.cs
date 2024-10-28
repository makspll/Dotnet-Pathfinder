using System.Text.RegularExpressions;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public interface IPlaceholderInliner
{
    void InlinePlaceholders(IEnumerable<ControllerCandidate> controllers);
    void SwapSpecialCharacters(IEnumerable<ControllerCandidate> controllers);
    void UnescapeSwappedSpecialCharacters(IEnumerable<ControllerCandidate> controllers);
}

public partial class PlaceholderInliner(FrameworkVersion version) : IPlaceholderInliner
{
    private readonly FrameworkVersion _version = version;

    public static readonly string LEFT_BRACE_ESCAPE_PLACEHOLDER = "Ѻ";
    public static readonly string RIGHT_BRACE_ESCAPE_PLACEHOLDER = "ѻ";
    public static readonly string LEFT_BRACKET_ESCAPE_PLACEHOLDER = "҈";
    public static readonly string RIGHT_BRACKET_ESCAPE_PLACEHOLDER = "҉";

    public static readonly Dictionary<string, string> specialCharMapping = new()
    {
        { "[[", LEFT_BRACKET_ESCAPE_PLACEHOLDER },
        { "]]", RIGHT_BRACKET_ESCAPE_PLACEHOLDER },
        { "{{", LEFT_BRACE_ESCAPE_PLACEHOLDER},
        { "}}", RIGHT_BRACE_ESCAPE_PLACEHOLDER}
    };

    public static readonly Dictionary<string, string> specialCharMappingReverse = new() {
        { LEFT_BRACKET_ESCAPE_PLACEHOLDER, "[" },
        { RIGHT_BRACKET_ESCAPE_PLACEHOLDER, "]" },
        { LEFT_BRACE_ESCAPE_PLACEHOLDER, "{" },
        { RIGHT_BRACE_ESCAPE_PLACEHOLDER, "}" }
    };



    public void InlinePlaceholders(IEnumerable<ControllerCandidate> controllers)
    {
        if (_version != FrameworkVersion.DOTNET_FRAMEWORK)
        {
            Inline(controllers);
        }
    }

    public void SwapSpecialCharacters(IEnumerable<ControllerCandidate> controllers)
    {
        AllRoutes(controllers).ToList().ForEach(x =>
        {
            foreach (var (key, value) in specialCharMapping)
            {
                x.Route.Path = x.Route.Path.Replace(key, value);
            }
        });
    }

    public void UnescapeSwappedSpecialCharacters(IEnumerable<ControllerCandidate> controllers)
    {
        AllRoutes(controllers, true).ToList().ForEach(x =>
        {
            foreach (var (key, value) in specialCharMappingReverse)
            {
                x.Route.Path = x.Route.Path.Replace(key, value);
            }
        });
    }

    private static IEnumerable<(ControllerCandidate Controller, ActionCandidate Action, Route Route)> AllRoutes(IEnumerable<ControllerCandidate> controllers, bool includeConventional = false)
    {
        foreach (var action in AllActions(controllers))
        {
            foreach (var route in action.Action.Routes)
            {
                yield return (action.Controller, action.Action, route);
            }
            if (includeConventional)
            {
                foreach (var conventionalRoute in action.Action.ConventionalRoutes)
                {
                    yield return (action.Controller, action.Action, conventionalRoute);
                }
            }
        }
    }

    private static IEnumerable<(ControllerCandidate Controller, ActionCandidate Action)> AllActions(IEnumerable<ControllerCandidate> controllers)
    {
        foreach (var controller in controllers)
        {
            foreach (var action in controller.Actions)
            {
                yield return (controller, action);
            }
        }
    }

    private void Inline(IEnumerable<ControllerCandidate> controllers)
    {
        foreach (var (controller, action) in AllActions(controllers))
        {
            var toAdd = new List<Route>();
            foreach (var RouteIndex in action.Routes.Select((value, index) => new { value, index }))
            {
                // save time if there are no placeholderrs
                if (RouteIndex.value.Path == null || !(RouteIndex.value.Path.Contains('{') || RouteIndex.value.Path.Contains('[')))
                    continue;

                var controllerPlaceholders = ControllerPlaceholderRegex().Match(RouteIndex.value.Path);
                var optionalControllerPlaceholders = OptionalControllerPlaceholderRegex().Match(RouteIndex.value.Path);
                var actionPlaceholders = ActionPlaceholderRegex().Match(RouteIndex.value.Path);
                var optionalActionPlaceholders = OptionalActionPlaceholderRegex().Match(RouteIndex.value.Path);

                var routeIndex = RouteIndex.index;
                var route = RouteIndex.value;
                var originalPath = route.Path;
                if (!optionalActionPlaceholders.Success && !optionalControllerPlaceholders.Success)
                {
                    // simple case
                    // replace all placeholders in the existing route
                    route.Path = ControllerPlaceholderRegex().Replace(originalPath, controller.ControllerName);
                    route.Path = ActionPlaceholderRegex().Replace(route.Path, action.ActionName(_version));
                }
                else
                {
                    // complex case, we might need to add multiple routes

                    // always add the route with all placeholders inlined
                    route.Path = ControllerPlaceholderRegex().Replace(route.Path, controller.ControllerName);
                    route.Path = ActionPlaceholderRegex().Replace(route.Path, action.ActionName(_version));
                    route.Path = OptionalControllerPlaceholderRegex().Replace(route.Path, controller.ControllerName);
                    route.Path = OptionalActionPlaceholderRegex().Replace(route.Path, action.ActionName(_version));

                    // then we might also need to add the routes with some placeholders missing
                    // if first placeholder is optional it only works when empty if the second one is also optional
                    var newRoutes = new List<string>();

                    if (optionalControllerPlaceholders.Success || optionalActionPlaceholders.Success)
                    {
                        var firstPlaceholderLocation = Math.Min(optionalControllerPlaceholders.Index, optionalActionPlaceholders.Index);
                        var replacedMissingController = originalPath[..firstPlaceholderLocation];
                        replacedMissingController = OptionalActionPlaceholderRegex().Replace(replacedMissingController, "");
                        replacedMissingController = OptionalControllerPlaceholderRegex().Replace(replacedMissingController, "");
                        replacedMissingController = Route.RemoveEmptySegments(replacedMissingController);

                        var replacedAll = ControllerPlaceholderRegex().Replace(originalPath, controller.ControllerName);
                        replacedAll = ActionPlaceholderRegex().Replace(replacedAll, action.ActionName(_version));
                        replacedAll = OptionalControllerPlaceholderRegex().Replace(replacedAll, "");
                        replacedAll = OptionalActionPlaceholderRegex().Replace(replacedAll, "");
                        replacedAll = Route.RemoveEmptySegments(replacedAll);

                        if (replacedMissingController == replacedAll)
                        {
                            newRoutes.Add(replacedMissingController);
                        }

                    }


                    // do always add the route with the first placeholder filled in and a combination of the second placeholder missing and filled in
                    var firstControllerIndex = Math.Min(controllerPlaceholders.Success ? controllerPlaceholders.Index : int.MaxValue, optionalControllerPlaceholders.Success ? optionalControllerPlaceholders.Index : int.MaxValue);
                    var firstActionIndex = Math.Min(actionPlaceholders.Success ? actionPlaceholders.Index : int.MaxValue, optionalActionPlaceholders.Success ? optionalActionPlaceholders.Index : int.MaxValue);
                    if (firstControllerIndex <= firstActionIndex)
                    {
                        var replacedController = ControllerPlaceholderRegex().Replace(originalPath, controller.ControllerName);
                        replacedController = OptionalControllerPlaceholderRegex().Replace(replacedController, controller.ControllerName);
                        replacedController = ActionPlaceholderRegex().Replace(replacedController, action.ActionName(_version));

                        var withMissingAction = OptionalActionPlaceholderRegex().Replace(replacedController, "");
                        var withFilledInAction = OptionalActionPlaceholderRegex().Replace(replacedController, action.ActionName(_version));
                        withMissingAction = Route.RemoveEmptySegments(withMissingAction);
                        withFilledInAction = Route.RemoveEmptySegments(withFilledInAction);
                        newRoutes.Add(withMissingAction);
                        newRoutes.Add(withFilledInAction);
                    }
                    else
                    {
                        var replacedAction = ActionPlaceholderRegex().Replace(originalPath, action.ActionName(_version));
                        replacedAction = OptionalActionPlaceholderRegex().Replace(replacedAction, action.ActionName(_version));
                        replacedAction = ControllerPlaceholderRegex().Replace(replacedAction, controller.ControllerName);

                        var withMissingController = OptionalControllerPlaceholderRegex().Replace(replacedAction, "");
                        var withFilledInController = OptionalControllerPlaceholderRegex().Replace(replacedAction, controller.ControllerName);
                        withMissingController = Route.RemoveEmptySegments(withMissingController);
                        withFilledInController = Route.RemoveEmptySegments(withFilledInController);
                        newRoutes.Add(withMissingController);
                        newRoutes.Add(withFilledInController);
                    }

                    foreach (var newRoute in newRoutes)
                    {
                        var newRouteObj = new Route
                        {
                            Path = newRoute,
                            Methods = route.Methods
                        };
                        toAdd.Add(newRouteObj);
                    }
                }

            }

            foreach (var route in toAdd)
            {
                if (!action.Routes.Any(x => x.Path == route.Path))
                    action.Routes.Add(route);
            }
        }

    }

    [GeneratedRegex(@"\[(controller)\]|{controller}", RegexOptions.Compiled)]
    private static partial Regex ControllerPlaceholderRegex();

    [GeneratedRegex(@"\[(action)\]|{action}", RegexOptions.Compiled)]
    private static partial Regex ActionPlaceholderRegex();

    [GeneratedRegex(@"{controller.*?\?}", RegexOptions.Compiled)]
    private static partial Regex OptionalControllerPlaceholderRegex();

    [GeneratedRegex(@"{action.*?\?}", RegexOptions.Compiled)]
    private static partial Regex OptionalActionPlaceholderRegex();
}
