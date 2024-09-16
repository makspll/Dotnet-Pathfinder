
using System;

namespace TestUtils;

[AttributeUsage(AttributeTargets.Method)]
public class ExpectRouteAttribute : Attribute
{
    public string Path { get; }
    public bool Conventional { get; set; }

    public ExpectRouteAttribute(string argument, bool conventional = false)
    {
        Path = argument;
        Conventional = conventional;
    }

}

[AttributeUsage(AttributeTargets.Method)]
public class ExpectNoRouteAttribute : Attribute
{
    public ExpectNoRouteAttribute() { }
}
