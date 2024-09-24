using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;
using System.Reflection;
using TestUtils;

namespace dotnet8
{
    [ApiController]
    [Route("attributecontrollerprefix2")]
    [Route("attributecontrollerprefix")]
    public class AttributeController
    {

        public Task Get() => Task.FromResult(new OkResult());


        [HttpGet]
        [Route("getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new OkResult());

        [HttpGet]
        [Route("multipleroutes/1")]
        [Route("multipleroutes/2")]
        public Task MultipleRoutes() => Task.FromResult(new OkResult());

    }

    [ApiController]
    public class AttributeControllerNoRoute
    {
        [HttpGet("attributecontrollernoroute/getWithHttpGetRoute")] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }


        [Route("attributecontrollernoroute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [Route("inheritingcontrollerprefix")]
    [Route("inheritingcontrollerprefix2")]
    public class InheritingController : ControllerBase
    {
        [HttpGet]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }

    }

    public class InheritingControllerNoRoute : ControllerBase
    {
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("InheritingControllerNoRoute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [Route("inheritingcontroller2prefix")]
    [Route("inheritingcontroller2prefix2")]
    public class InheritingController2 : Controller
    {
        [HttpGet]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }

    }

    public abstract class CustomBase : Controller;

    public class CustomBaseInheritingController : CustomBase
    {

        [HttpGet]
        [Route("custombase/route")]
        public Task Get() => Task.FromResult(new OkResult());
    }

    public class InheritingController2NoRoute : Controller
    {
        [HttpGet]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("inheritingcontroller2noroute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [ApiController]
    [Route("activeController")]
    [Route("activeController2")]
    public class ActiveControllerWithNoMethodRoute
    {
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ActiveControllerWithNoMethodRoute"));
        }
    }

    [ApiController]
    [Route("controllerComplexHttpMethods")]
    [Route("controllerComplexHttpMethods2")]
    public class ControllerComplexHttpMethods
    {
        [HttpGet("get")]
        [HttpPost("post")]
        [HttpPut("put")]
        [HttpDelete("delete")]
        [HttpPatch("patch")]
        [HttpOptions("options")]
        [HttpHead("head")]
        public Task MultipleHTTPMethodsWithRotues() => Task.FromResult(new OkResult());

        [HttpGet("get2")]
        [HttpPost]
        [HttpPut("put2")]
        [HttpDelete]
        [HttpPatch("patch2")]
        [HttpOptions("options2")]
        [HttpHead]
        public Task MultipleHTTPMethodsSomeWithRoutes() => Task.FromResult(new OkResult());

        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButNoMethod() => Task.FromResult(new OkResult());

        [HttpGet("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButGetMethod() => Task.FromResult(new OkResult());

        [HttpPost("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButPostMethod() => Task.FromResult(new OkResult());
    }

    public class DefaultConventionalController : Controller
    {
        [HttpGet]
        public Task<OkObjectResult> DefaultAction() => Task.FromResult(new OkObjectResult("DefaultConventionalController"));

        [HttpGet]
        public Task<OkObjectResult> NonDefaultAction() => Task.FromResult(new OkObjectResult("NonDefaultAction"));
    }

    public class DefaultControllerConventional : Controller
    {
        [NonAction]
        [HttpGet]
        public Task<OkObjectResult> DefaultAction() => Task.FromResult(new OkObjectResult("DefaultConventionalController"));

        public Task<OkObjectResult> NonDefaultAction() => Task.FromResult(new OkObjectResult("NonDefaultAction"));
    }


    [ApiController]
    public class ControllerWithNoRoutes;

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
}