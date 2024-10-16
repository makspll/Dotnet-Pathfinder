using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public class CandidateConverter(FrameworkVersion version)
{
    readonly FrameworkVersion _version = version;

    public IList<Controller> ConvertCandidates(IEnumerable<ControllerCandidate> controllers)
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
                        Name = action.ActionName(_version),
                        MethodName = action.Method.Name,
                        Attributes = action.RoutingAttributes.Select(x => x.IntoSerializedAttribute(_version)),
                        IsConventional = isConventional,
                        Routes = isConventional ? [.. action.ConventionalRoutes] : [.. action.Routes]
                    };
                }).ToList()
            }
        ).ToList();
    }
}