using dnlib.DotNet;

namespace Makspll.ReflectionUtils
{
    internal static class AttributeReader
    {
        internal const string ROUTE_PREFIX_ATTRIBUTE = "RoutePrefixAttribute";
        internal const string ROUTE_ATTRIBUTE = "RouteAttribute";
        internal const string API_CONTROLLER_ATTRIBUTE = "ApiControllerAttribute";
        internal static readonly string[] HttpMethodAttributes = ["HttpGetAttribute", "HttpPostAttribute", "HttpPutAttribute", "HttpDeleteAttribute", "HttpPatchAttribute", "HttpOptionsAttribute"];
        internal static readonly string[] RoutingAttributes = [ROUTE_ATTRIBUTE, ROUTE_PREFIX_ATTRIBUTE, .. HttpMethodAttributes];


        /**
        * For example [Route("api/[controller]")] or [Route("api/[controller]/[action]")] have no named arguments so we need to get the constructor value
        * But [Route(Route = "asd")] has a named argument so we can get it by name
        */
        internal static string? GetAttributeConstructorValuesOrNamedArg(CustomAttribute attribute, IEnumerable<string> names)
        {
            var value = attribute.ConstructorArguments.Aggregate("", (acc, arg) => acc + arg.Value?.ToString());
            if (value != "" && value != null)
            {
                return value;
            }

            return attribute.NamedArguments.FirstOrDefault(a => names.Any(x => x == a.Name))?.Value?.ToString();
        }

        internal static string? GetAttributeNamedValue(CustomAttribute attribute, string name)
        {
            return attribute.NamedArguments.FirstOrDefault(a => name == a.Name)?.Value?.ToString();
        }

        internal static string? GetAttributeConstructorValues(CustomAttribute attribute, string sep)
        {
            return attribute.ConstructorArguments.Aggregate("", (acc, arg) => acc + arg.Value?.ToString() + sep);
        }

        /**
        * Get the route prefix or suffix of a controller or its method.
        * If the controller has a RouteAttribute or RoutePrefixAttribute, return the value of the Name or Prefix property.
        * If the method has neither of the attributes, return null.
        * If Name or Prefix is not set, return an empty string.
        */
        internal static string? GetRoutePrefixOrSuffix(IEnumerable<CustomAttribute> attributes, AssemblyFramework framework = AssemblyFramework.UNKNOWN)
        {
            return attributes.Select(a => RoutingAttributes
                .Any(x => a.AttributeType.Name == x) ?
                    GetAttributeConstructorValuesOrNamedArg(a, ["Prefix", "Name"]) :
                    null
                ).FirstOrDefault(a => a != null && a != "");
        }

        internal static string? GetHttpMethod(MethodDef method)
        {
            return method.CustomAttributes
                .FirstOrDefault(a =>
                    HttpMethodAttributes.Any(x => a.AttributeType.Name == x))?
                .AttributeType.Name.Replace("Attribute", "").Replace("Http", "").ToUpper();
        }

    }


}