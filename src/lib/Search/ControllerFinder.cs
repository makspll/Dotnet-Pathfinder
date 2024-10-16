using dnlib.DotNet;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Search;

public static class ControllerFinder
{
    public static IEnumerable<ControllerCandidate> FindControllers(ModuleDef module)
    {
        foreach (var type in module.GetTypes())
        {
            var routingAttributes = type.CustomAttributes.Select(AttributeParser.ParseAttribute).OfType<RoutingAttribute>();
            if (IsController(type, routingAttributes))
            {
                yield return new ControllerCandidate()
                {
                    Type = type,
                    RoutingAttributes = routingAttributes
                };
            }
        }

    }

    public static bool IsController(TypeDef type, IEnumerable<RoutingAttribute> routingAttributes)
    {
        return !type.IsAbstract && (
            routingAttributes.Any(x => x is ApiControllerAttribute)
                || InheritsFrom(type, "Controller")
                || InheritsFrom(type, "ControllerBase")
                || InheritsFrom(type, "ApiController")
            );
    }

    static private bool InheritsFrom(ITypeDefOrRef? type, string basetype)
    {
        if (type == null)
            return false;
        else
        {
            var basetypeType = type.GetBaseType();
            if (basetypeType == null)
                return false;
            else if (basetypeType.Name.String == basetype)
                return true;
            else
                return InheritsFrom(basetypeType, basetype);
        }
    }

}