using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Propagation;

public static class AttributePropagator
{
    public static void PropagateAttributes(ControllerCandidate controller)
    {
        var propagatedRoutes = controller.RoutingAttributes
            .Select(x => ConvertToPropagatedRoute(x, true))
            .OfType<PropagatedRoute>();

        foreach (var action in controller.Actions)
        {
            var actionPropagations = action.RoutingAttributes
                .Select(x => ConvertToPropagatedRoute(x, false))
                .OfType<PropagatedRoute>();

            action.PropagatedRoutes = propagatedRoutes.Concat(actionPropagations);
        }
    }

    private static PropagatedRoute? ConvertToPropagatedRoute(RoutingAttribute attribute, bool fromController)
    {
        if (attribute.Propagation() != RoutePropagation.None && attribute.Route() != null)
        {
            return new PropagatedRoute
            {
                Prefix = attribute.Route()!,
                PropagationType = attribute.Propagation(),
                FromController = fromController
            };
        }
        else
        {
            return null;
        }
    }
}