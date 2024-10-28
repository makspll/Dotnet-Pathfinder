using dnlib.DotNet;
using FluentAssertions;
using Makspll.Pathfinder.Parsing;


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DummyAttribute : Attribute
{
    public string? Test { get; set; }
    public string? Prop;

    public int NoConstructorArgProp { get; set; }

    public DummyAttribute(string? test, string? prop)
    {
        Test = test;
        Prop = prop;
    }
}



public class DummyAttributeCarrier
{
    [Dummy("test", "prop")]
    public void NonNullOnlyConstructorArgs() { }

    [Dummy(null, null)]
    public void NullOnlyConstructorArgs() { }

    [Dummy("test", "prop", NoConstructorArgProp = 2)]
    public void NonNullConstructorArgs() { }

}

public class AttributeParserTests
{

    [Fact]
    public void TestAttributeGetPropOrField()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var method = module.Types.ToList().Find(x => x.Name == "DummyAttributeCarrier")!.Methods.ToList().Find(x => x.Name == "NonNullOnlyConstructorArgs")!;
        var attribute = method.CustomAttributes.First();

        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Test", 0).Should().Be("test");
        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Prop", 1).Should().Be("prop");
    }

    [Fact]
    public void TestAttributeGetPropOrField_Nulls()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var method = module.Types.ToList().Find(x => x.Name == "DummyAttributeCarrier")!.Methods.ToList().Find(x => x.Name == "NullOnlyConstructorArgs")!;
        var attribute = method.CustomAttributes.First();

        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Test", 0).Should().BeNull();
        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Prop", 1).Should().BeNull();
    }

    [Fact]
    public void TestAttributeGetPropOrField_NoConstructorType()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var method = module.Types.ToList().Find(x => x.Name == "DummyAttributeCarrier")!.Methods.ToList().Find(x => x.Name == "NonNullConstructorArgs")!;
        var attribute = method.CustomAttributes.First();

        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Test", 0).Should().Be("test");
        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "Prop", 1).Should().Be("prop");
        AttributeParser.GetAttributeNamedArgOrConstructorArg<int>(attribute, "NoConstructorArgProp", -1).Should().Be(2);
    }

    [Fact]
    public void TestAttributeGetPropOrField_BackupConstructorIdx()
    {
        var module = ModuleDefMD.Load(typeof(DummyAttributeCarrier).Module);
        var method = module.Types.ToList().Find(x => x.Name == "DummyAttributeCarrier")!.Methods.ToList().Find(x => x.Name == "NonNullOnlyConstructorArgs")!;
        var attribute = method.CustomAttributes.First();

        AttributeParser.GetAttributeNamedArgOrConstructorArg<string>(attribute, "NoConstructorArgProp", 0).Should().Be("test");
    }
}