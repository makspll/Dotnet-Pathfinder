using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TestUtils;
namespace dotnet8;

[ApiController]
[Route("api")]
public class ListAllRoutesController(
    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
    IEnumerable<EndpointDataSource> routeCollection
    ) : ControllerBase
{
    private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    private readonly IEnumerable<EndpointDataSource> routeCollection = routeCollection;
    [HttpGet]
    [Route("attributeroutes")]
    public Task<OkObjectResult> ListAttributeRoutes()
    {
        var actionDescriptors = actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
        return Task.FromResult(new OkObjectResult(PathsExporter.ListAllRoutes(actionDescriptors, routeCollection, false)));
    }

    [HttpGet]
    [Route("conventionalroutes")]
    public Task<OkObjectResult> ListConventionalRoutes()
    {
        var actionDescriptors = actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
        return Task.FromResult(new OkObjectResult(PathsExporter.ListAllRoutes(actionDescriptors, routeCollection, true, false)));
    }
}