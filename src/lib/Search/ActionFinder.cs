using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public class ActionFinder(FrameworkVersion version)
{
    private readonly FrameworkVersion _version = version;

    public void PopulateActions(ControllerCandidate controller)
    {
        var actions = FindActionCandidates(controller);
        controller.Actions.AddRange(actions);
    }

    public void PopulateConventionalActions(ControllerCandidate controller)
    {
        var actions = FindActionCandidates(controller, true);
        controller.Actions.AddRange(actions);
    }

    private IEnumerable<ActionCandidate> FindActionCandidates(ControllerCandidate controller, bool excludeDisabledConventionalActions = true)
    {

        foreach (var method in controller.Type.Methods)
        {
            if (method.IsConstructor || method.IsGetter || method.IsSetter || method.IsStatic || method.IsAbstract || !method.IsPublic)
                continue;

            var routingAttributes = method.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>();
            if (excludeDisabledConventionalActions && routingAttributes.Any(x => x.DisablesConventionalRoutes(_version)))
                continue;


            yield return new ActionCandidate()
            {
                Method = method,
                RoutingAttributes = routingAttributes,
                Controller = controller
            };
        }
    }
}