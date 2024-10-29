using dnlib.DotNet;
using Makspll.Pathfinder.Parsing;
using Makspll.Pathfinder.Routing;
using Newtonsoft.Json;

namespace tests.Serializing;


public enum SomeEnum
{
    A,
    B,
    C
}

public class GenericClass<T>
{
    public required T Value { get; set; }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DummyAttribute(string? Test, Type Type, string[] Array, SomeEnum e, SomeEnum[] enums) : Attribute
{
    public string? Test { get; } = Test;
    public Type Type { get; } = Type;
    public string[] Array { get; } = Array;

    public SomeEnum Enum { get; } = e;
    public SomeEnum[] Enums { get; } = enums;
}

public class DummyAttributeCarrier
{

    [Dummy("test", typeof(int), ["a", "b", "c"], SomeEnum.A, [SomeEnum.A, SomeEnum.B])]
    public void Test() { }

    [Dummy(null, typeof(int), ["a", "b", "c"], SomeEnum.B, [SomeEnum.B, SomeEnum.C])]
    public void Test2() { }

    [Dummy(null, typeof(List<string>), ["a", "b", "c"], SomeEnum.C, [SomeEnum.C, SomeEnum.A])]
    public void Test3() { }

    [Dummy(null, typeof(List<GenericClass<int>>), ["a", "b", "c"], SomeEnum.A, [SomeEnum.A, SomeEnum.B])]
    public void Test4() { }

    [Dummy(null, typeof(byte[]), ["a", "b", "c"], SomeEnum.A, [SomeEnum.A, SomeEnum.B])]
    public void Test5() { }

}

public class SerializedAttributeTests
{

    [Fact]
    public void TestSerializeAttribute_DoesNotBreak()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var method = module.Types.ToList().SelectMany(x => x.Methods.ToList());

        foreach (var m in method)
        {
            var attribute = m.CustomAttributes.FirstOrDefault();
            if (attribute != null)
            {
                var parsed = AttributeParser.ParseNonRoutingAttribute(attribute);
                var serialized = JsonConvert.SerializeObject(parsed);
                var deserialized = JsonConvert.DeserializeObject<SerializedAttribute>(serialized);
            }
        }
    }

    [Fact]
    public void TestSerializedType_IsFullName()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var methods = module.Types.ToList().SelectMany(x => x.Methods.ToList());
        var method = methods.First(x => x.Name == "Test2");


        var attribute = method.CustomAttributes.FirstOrDefault();
        var parsed = AttributeParser.ParseNonRoutingAttribute(attribute!);
        var serialized = JsonConvert.SerializeObject(parsed);
        var expectedSerialized = JsonConvert.SerializeObject(new SerializedAttribute
        {
            Name = "DummyAttribute",
            Properties = new() {
                {"0", null!},
                {"1", "System.Int32"},
                {"2", new List<object> { "a", "b", "c" }},
                {"3", 1},
                {"4", new List<object> { 1, 2 }}
            }
        });
        Assert.Equal(expectedSerialized, serialized);
    }
}