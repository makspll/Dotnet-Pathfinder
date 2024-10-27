using dnlib.DotNet;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public interface IControllerFinder
{
    IEnumerable<ControllerCandidate> FindControllers(ModuleDef module);
}

public class ControllerFinder(FrameworkVersion _version) : IControllerFinder
{
    private readonly FrameworkVersion version = _version;

    public IEnumerable<ControllerCandidate> FindControllers(ModuleDef module)
    {
        foreach (var type in module.GetTypes())
        {
            var routingAttributes = type.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>();
            var controllerKind = GetControllerKind(type, routingAttributes);
            if (controllerKind != null)
            {
                yield return new ControllerCandidate()
                {
                    Type = type,
                    RoutingAttributes = routingAttributes,
                    Kind = controllerKind.Value
                };
            }
        }

    }

    public ControllerKind? GetControllerKind(TypeDef type, IEnumerable<RoutingAttribute> routingAttributes)
    {
        if (type.IsAbstract)
            return null;
        var hasControllerSuffix = type.Name.String.EndsWith("Controller");

        if (InheritsFrom(type, ["Controller", "ControllerBase"], "System.Web.Mvc") && hasControllerSuffix)
            return ControllerKind.MVC;
        else if (InheritsFrom(type, ["ApiController"], "System.Web.Http") && hasControllerSuffix)
            return ControllerKind.API;
        else if (InheritsFrom(type, ["Controller", "ControllerBase"], "Microsoft.AspNetCore.Mvc"))
            return ControllerKind.CORE;
        else if (routingAttributes.Any(x => x.EnablesController(version)))
            return ControllerKind.CORE;
        else
            return null;
    }

    static private bool InheritsFrom(ITypeDefOrRef? type, IEnumerable<string> basetypes, string? namespacePrefix = null)
    {
        if (type == null)
            return false;
        else
        {
            var basetypeType = type.GetBaseType();
            if (basetypeType == null)
                return false;
            else if (basetypes.Contains(basetypeType.Name.String) && (namespacePrefix == null || basetypeType.Namespace.StartsWith(namespacePrefix)))
                return true;
            else
                return InheritsFrom(basetypeType, basetypes, namespacePrefix);
        }
    }

}