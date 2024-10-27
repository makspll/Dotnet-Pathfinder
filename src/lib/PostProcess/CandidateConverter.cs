using dnlib.DotNet;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public interface ICandidateConverter
{
    IList<Controller> ConvertCandidates(IEnumerable<ControllerCandidate> controllers);
}

public class CandidateConverter(FrameworkVersion version) : ICandidateConverter
{
    readonly FrameworkVersion _version = version;

    private SerializedAttribute ConvertAttribute(RoutingAttribute attribute) => attribute.IntoSerializedAttribute(_version);

    private SerializedAttribute? ConvertAnyAtribute(CustomAttribute attribute)
    {
        var attr = AttributeParser.ParseNonRoutingAttribute(attribute);
        if (attribute.AttributeType.Namespace.Contains("System.Runtime"))
        {
            return null;
        }
        return attr;
    }

    public IList<Controller> ConvertCandidates(IEnumerable<ControllerCandidate> controllers)
    {
        return controllers.Select(controller =>
            new Controller
            {
                ControllerName = controller.ControllerName,
                ClassName = controller.Type.Name,
                Namespace = controller.Type.Namespace,
                Attributes = controller.RoutingAttributes
                    .Select(ConvertAttribute)
                    .Concat(controller.Type.CustomAttributes
                    .Select(ConvertAnyAtribute).OfType<SerializedAttribute>())
                    .ToList(),
                Actions = controller.Actions.Select(action =>
                {
                    var isConventional = !action.Routes.Any() && action.ConventionalRoutes.Any();
                    return new Routing.Action
                    {
                        Name = action.ActionName(_version),
                        MethodName = action.Method.Name,
                        Attributes = action.RoutingAttributes
                            .Select(ConvertAttribute)
                            .Concat(action.Method.CustomAttributes
                            .Select(ConvertAnyAtribute).OfType<SerializedAttribute>())
                            .ToList(),
                        IsConventional = isConventional,
                        Routes = isConventional ? [.. action.ConventionalRoutes] : [.. action.Routes]
                    };
                }).ToList()
            }
        ).ToList();
    }

}