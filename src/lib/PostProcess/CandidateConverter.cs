using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public static class CandidateConverter
{
    public static IList<Controller> ConvertCandidates(IEnumerable<ControllerCandidate> controllers)
    {
        return controllers.Select(controller =>
            new Controller
            {
                ControllerName = controller.ControllerName,
                ClassName = controller.Type.Name,
                Namespace = controller.Type.Namespace,
                Attributes = controller.RoutingAttributes,
                Actions = controller.Actions.Select(action =>
                {
                    var isConventional = !action.Routes.Any() && action.ConventionalRoutes.Any();
                    return new Routing.Action
                    {
                        Name = action.ActionName,
                        MethodName = action.Method.Name,
                        Attributes = action.RoutingAttributes,
                        IsConventional = isConventional,
                        Routes = isConventional ? [.. action.ConventionalRoutes] : [.. action.Routes]
                    };
                }).ToList()
            }
        ).ToList();
    }
}