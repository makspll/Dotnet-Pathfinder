using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public static class ActionFinder
{
    public static IEnumerable<ActionCandidate> FindActions(ControllerCandidate controller)
    {
        return FindActionCandidates(controller);
    }

    public static IEnumerable<ActionCandidate> FindConventionalActions(ControllerCandidate controller)
    {
        return FindActionCandidates(controller, true);
    }

    private static IEnumerable<ActionCandidate> FindActionCandidates(ControllerCandidate controller, bool excludeDisabledConventionalActions = true)
    {

        foreach (var method in controller.Type.Methods)
        {
            if (method.IsConstructor || method.IsGetter || method.IsSetter || method.IsStatic || method.IsAbstract || !method.IsPublic)
                continue;

            var routingAttributes = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>();
            if (excludeDisabledConventionalActions && routingAttributes.Any(x => x.DisablesConventionalRoutes()))
                continue;


            yield return new ActionCandidate()
            {
                Method = method,
                RoutingAttributes = routingAttributes
            };
        }
    }
}