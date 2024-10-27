using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Propagation;

public interface IAttributePropagator
{
    void PropagateAttributes(ControllerCandidate controller);
}

public class AttributePropagator(FrameworkVersion version) : IAttributePropagator
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
        if (attribute.PropagateToActions(_version) && (attribute.Route(_version) != null) || attribute.RoutePrefix(_version) != null)
        {
            return new PropagatedRoute
            {
                Route = attribute.Route(_version),
                Prefix = attribute.RoutePrefix(_version),
                FromController = fromController,
            };
        }
        else
        {
            return null;
        }
    }
}