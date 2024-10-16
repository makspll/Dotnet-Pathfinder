using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using dnlib.DotNet;
using Makspll.Pathfinder.Parsing;

namespace Makspll.Pathfinder.Routing;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FrameworkVersion
{
    DOTNET_FRAMEWORK,
    DOTNET_CORE,
    UNKNOWN
}


public static class FrameworkVersionExtensions
{
    public static FrameworkVersion DetectFrameworkVersion(this ModuleDef module)
    {
        // first check attributes
        var attrFrameworkName = module.CustomAttributes.Where(attr => attr.AttributeType.Name == typeof(TargetFrameworkAttribute).Name).FirstOrDefault();
        if (attrFrameworkName != null)
        {
            var frameworkName = AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attrFrameworkName, "FrameworkName", 0);

            if (frameworkName != null)
            {
                var parsedFrameworkName = new FrameworkName(frameworkName);
                switch (parsedFrameworkName.Identifier)
                {
                    case ".NETFramework":
                        return FrameworkVersion.DOTNET_FRAMEWORK;
                    case ".NETCoreApp":
                        return FrameworkVersion.DOTNET_CORE;
                }
            }
        }


        // easy pickings, system.web is not present in .NET Core, and Microsoft.AspNetCore.Mvc is not present in .NET Framework
        if (module.GetAssemblyRefs().Any(assemblyRef => assemblyRef.Name == "System.Web"))
        {
            return FrameworkVersion.DOTNET_FRAMEWORK;
        }
        else if (module.GetAssemblyRefs().Any(assemblyRef => assemblyRef.Name.Contains("Microsoft.AspNetCore")))
        {
            return FrameworkVersion.DOTNET_CORE;
        }

        return FrameworkVersion.UNKNOWN;
    }
}