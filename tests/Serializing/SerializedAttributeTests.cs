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

public class ValueTypeAttribute(int Int, string String, bool Bool, char Char) : Attribute
{
    public int Int { get; } = Int;
    public string String { get; } = String;
    public bool Bool { get; } = Bool;

    public char Char { get; } = Char;
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

    [ValueType(1, "test", true, 'a')]
    public void Test6() { }

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
                {"0", new AttributeParser.ValueType("System.String", null!)},
                {"1", "System.Int32"},
                {"2", new List<object> { new AttributeParser.ValueType("System.String","a"), new AttributeParser.ValueType("System.String","b"), new AttributeParser.ValueType("System.String","c") }},
                {"3", new AttributeParser.ValueType("tests.Serializing.SomeEnum", 1)},
                {"4", new List<object> { new AttributeParser.ValueType("tests.Serializing.SomeEnum", 1), new AttributeParser.ValueType("tests.Serializing.SomeEnum", 2)}}
            }
        });
        Assert.Equal(expectedSerialized, serialized);
    }

    [Fact]
    public void TestSerializedValueTypes_AreValues()
    {
        var module = ModuleDefMD.Load(typeof(ValueTypeAttribute).Module);
        var methods = module.Types.ToList().SelectMany(x => x.Methods.ToList());
        var method = methods.First(x => x.Name == "Test6");


        var attribute = method.CustomAttributes.FirstOrDefault();
        var parsed = AttributeParser.ParseNonRoutingAttribute(attribute!);
        var serialized = JsonConvert.SerializeObject(parsed);
        var expectedSerialized = JsonConvert.SerializeObject(new SerializedAttribute
        {
            Name = "ValueTypeAttribute",
            Properties = new() {
                {"0", new AttributeParser.ValueType("System.Int32", 1)},
                {"1", new AttributeParser.ValueType("System.String", "test")},
                {"2", new AttributeParser.ValueType("System.Boolean", true)},
                {"3", new AttributeParser.ValueType("System.Char", 'a')}
            }
        });
        Assert.Equal(expectedSerialized, serialized);
    }
}