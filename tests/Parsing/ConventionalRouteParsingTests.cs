using Makspll.Pathfinder.Routing;
using static Makspll.Pathfinder.Routing.ConventionalRoute;

namespace tests.Parsing;

public class ConventionalRouteParsingTests
{

    public static IEnumerable<object[]> GetRoutePartsData()
    {
        yield return new object[] { "part" };
        yield return new object[] { "{part}" };
        yield return new object[] { "{part?}" };
        yield return new object[] { "{part:int}" };
        yield return new object[] { "{part:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}})?}" };
        yield return new object[] { "{part:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}})}" };
        yield return new object[] { "{part:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}})=5}" };
        yield return new object[] { "{part:max(1,2)}" };
        yield return new object[] { "{part:int?}" };
        yield return new object[] { "{part:int=5}" };
        yield return new object[] { "{*catchall}" };
        yield return new object[] { "{**catchall}" };

    }

    [Theory]
    [MemberData(nameof(GetRoutePartsData))]
    public void TestRoutePartsToString(string part)
    {
        var testPart = RouteTemplatePart.Parse(part).Value;
        Assert.Equal(part, testPart.ToString());
    }

    [Fact]
    public void TestConventionalRouteParsing()
    {
        var route = Parse("a/{controller}/{controller?}/{action}/{action?}/{id}/{id?}", null, null).Value;
        Assert.Equal("/a/Home/Home/action/action/{id}/{id?}", route.InstantiateTemplateWith("Home", "action", null));
    }

    [Fact]
    public void TestConventionalRouteParsing_StartAndEndSlash()
    {
        var route = Parse("/a/{controller}/{controller?}/{action}/{action?}/{id}/{id?}/", null, null).Value;
        Assert.Equal("/a/Home/Home/action/action/{id}/{id?}", route.InstantiateTemplateWith("Home", "action", null));
    }

    [Fact]
    public void TestConventionalRouteParsingComplex()
    {
        var route = Parse("{hello}.{complex}{part:max(1,2)}/{part:regex(^\\d{{3}}-\\d{{2}}-\\d{{4}})?}/{banana}yoghurt{apple}lol{**catchall}", null, null).Value;
        Assert.Equal("/{hello}.{complex}{part:max(1,2)}/{part:regex(^\\dѺ3ѻ-\\dѺ2ѻ-\\dѺ4ѻ)?}/{banana}yoghurt{apple}lol{**catchall}", route.InstantiateTemplateWith(null, null, null));
    }
}