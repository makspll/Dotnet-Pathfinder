using Makspll.Pathfinder.Routing;

namespace tests.Attributes;

public class HTTPMethodTests
{
    [Fact]
    public void TestHTTPMethodExtensions()
    {
        Assert.Equal(new HttpMethod("GET"), HTTPMethod.GET.ToHttpMethod());
        Assert.Equal(new HttpMethod("POST"), HTTPMethod.POST.ToHttpMethod());
        Assert.Equal(new HttpMethod("PUT"), HTTPMethod.PUT.ToHttpMethod());
        Assert.Equal(new HttpMethod("DELETE"), HTTPMethod.DELETE.ToHttpMethod());
        Assert.Equal(new HttpMethod("PATCH"), HTTPMethod.PATCH.ToHttpMethod());
        Assert.Equal(new HttpMethod("HEAD"), HTTPMethod.HEAD.ToHttpMethod());
        Assert.Equal(new HttpMethod("OPTIONS"), HTTPMethod.OPTIONS.ToHttpMethod());
    }

    [Fact]
    public void TestHTTPMethodExtensionsToVerbString()
    {
        Assert.Equal("Get", HTTPMethod.GET.ToVerbString());
        Assert.Equal("Post", HTTPMethod.POST.ToVerbString());
        Assert.Equal("Put", HTTPMethod.PUT.ToVerbString());
        Assert.Equal("Delete", HTTPMethod.DELETE.ToVerbString());
        Assert.Equal("Patch", HTTPMethod.PATCH.ToVerbString());
        Assert.Equal("Head", HTTPMethod.HEAD.ToVerbString());
        Assert.Equal("Options", HTTPMethod.OPTIONS.ToVerbString());
    }
}