using System.Text;
using dnlib.DotNet;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.Parsing;

public static class AttributeParser
{

    private static readonly HashSet<string> routingAttributeNames = new()
    {
        "RouteAttribute",
        "RoutePrefixAttribute",
        "ApiControllerAttribute",
        "NonActionAttribute",
        "ActionNameAttribute",
        "AreaAttribute",
        "AcceptVerbsAttribute",
        "HttpGetAttribute",
        "HttpPostAttribute",
        "HttpPutAttribute",
        "HttpDeleteAttribute",
        "HttpPatchAttribute",
        "HttpHeadAttribute",
        "HttpOptionsAttribute"
    };

    public static T? GetAttributeNamedArgOrConstructorArg<T>(CustomAttribute attribute, string name, int index = -1)
    {
        var output = attribute.NamedArguments.FirstOrDefault(x => x.Name == name)?.Value;
        if (index >= 0 && attribute.HasConstructorArguments)
        {
            output = attribute.ConstructorArguments[index].Value;
        }


        if (output == null)
        {
            return default;
        }
        else if (typeof(T) == typeof(string) && output is UTF8String utf8String)
        {
            return (T)(object)utf8String.ToString();
        }
        else if (typeof(T).IsAssignableFrom(output.GetType()))
        {
            return (T)output;
        }
        else
        {
            throw new ArgumentException($"Expected argument of type {typeof(T).FullName} but got {output.GetType().FullName}");
        }
    }

    private static object SimplifyObjectValue(object value)
    {
        if (value is UTF8String utf8String)
        {
            return Encoding.UTF8.GetString(utf8String.Data);
        }
        else if (value is TypeDefOrRefSig typeDefOrRef)
        {
            return typeDefOrRef.ReflectionFullName;
        }
        else if (value is GenericInstSig genericSig)
        {
            return genericSig.ReflectionFullName;
        }
        else if (value is List<CAArgument> list)
        {
            return list.Select(x => SimplifyObjectValue(x.Value)).ToList();
        }
        return value;
    }

    public static SerializedAttribute? ParseNonRoutingAttribute(CustomAttribute attribute)
    {
        if (routingAttributeNames.Contains(attribute.AttributeType.Name))
        {
            return null;
        }


        var constructorArguments = attribute.ConstructorArguments.Select((x, i) => (i.ToString(), x.Value)).ToList();
        var namedArguments = attribute.NamedArguments.Select(x => (Encoding.UTF8.GetString(x.Name.Data), x.Value));

        var allProperties = constructorArguments.Concat(namedArguments).ToDictionary(x => x.Item1, x => SimplifyObjectValue(x.Value));
        return new SerializedAttribute
        {
            Name = attribute.AttributeType.Name,
            Properties = allProperties
        };
    }


    public static RoutingAttribute? ParseAttribute(CustomAttribute attribute)
    {

        switch (attribute.AttributeType.Name)
        {
            case "RouteAttribute":
                return new RouteAttribute(GetAttributeNamedArgOrConstructorArg<string>(attribute, "Template", 0));
            case "RoutePrefixAttribute":
                return new RoutePrefixAttribute(GetAttributeNamedArgOrConstructorArg<string>(attribute, "Prefix", 0));
            case "ApiControllerAttribute":
                return new ApiControllerAttribute();
            case "NonActionAttribute":
                return new NonActionAttribute();
            case "ActionNameAttribute":
                return new ActionNameAttribute(GetAttributeNamedArgOrConstructorArg<string>(attribute, "Name", 0));
            case "AreaAttribute":
                return new AreaAttribute(GetAttributeNamedArgOrConstructorArg<string>(attribute, "NONAME", 0));
            case "AcceptVerbsAttribute":
                var methods = GetAttributeNamedArgOrConstructorArg<List<CAArgument>>(attribute, "Http Methods", 0)?.Select(x => Enum.Parse<HTTPMethod>(x.Value.ToString() ?? "UNKNOWN"));
                var routeValue = GetAttributeNamedArgOrConstructorArg<string>(attribute, "Route", -1);
                return new AcceptVerbsAttribute(methods) { RouteValue = routeValue };
            case "HttpGetAttribute":
            case "HttpPostAttribute":
            case "HttpPutAttribute":
            case "HttpDeleteAttribute":
            case "HttpPatchAttribute":
            case "HttpHeadAttribute":
            case "HttpOptionsAttribute":
                var method = attribute.AttributeType.Name.Replace("Attribute", "").Replace("Http", "").ToUpper();
                var methodParsed = (HTTPMethod)Enum.Parse(typeof(HTTPMethod), method);
                var route = GetAttributeNamedArgOrConstructorArg<string>(attribute, "Template", 0);
                return new HttpAttribute(methodParsed, route);
            default:
                return null;
        }
    }
}