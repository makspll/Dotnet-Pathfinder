using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Propagation;

public class AttributePropagator(FrameworkVersion version)
{
    private readonly FrameworkVersion _version = version;

    public void PropagateAttributes(ControllerCandidate controller)
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

    private PropagatedRoute? ConvertToPropagatedRoute(RoutingAttribute attribute, bool fromController)
    {
        if (attribute.Propagation(_version) != RoutePropagation.None && attribute.Route(_version) != null)
        {
            return new PropagatedRoute
            {
                Prefix = attribute.Route(_version)!,
                PropagationType = attribute.Propagation(_version),
                FromController = fromController,
                PropagatesInControllerKinds = attribute.PropagationContexts()
            };
        }
        else
        {
            return null;
        }
    }
}