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
    [Route("attributecontrollerprefix")]
    public class AttributeController
    {

        [ExpectRoute("/attributecontrollerprefix")]
        public Task Get() => Task.FromResult(new OkResult());


        [HttpGet]
        [Route("getwithroute")]
        [ExpectRoute("/attributecontrollerprefix/getwithroute")]
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
        [ExpectRoute("/attributecontrollernoroute/getWithHttpGetRoute")]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }


        [Route("attributecontrollernoroute/getwithroute")]
        [ExpectRoute("/attributecontrollernoroute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [Route("inheritingcontrollerprefix")]
    public class InheritingController : ControllerBase
    {
        [HttpGet]
        [ExpectRoute("/inheritingcontrollerprefix")]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        [ExpectRoute("/inheritingcontrollerprefix/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }

    }

    public class InheritingControllerNoRoute : ControllerBase
    {
        [ExpectRoute("/program/{controller}/{action}", conventional: true)]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("InheritingControllerNoRoute/getwithroute")]
        [ExpectRoute("/InheritingControllerNoRoute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [Route("inheritingcontroller2prefix")]
    public class InheritingController2 : Controller
    {
        [HttpGet]
        [ExpectRoute("/inheritingcontroller2prefix")]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        [ExpectRoute("/inheritingcontroller2prefix/getwithroute")]
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
        [ExpectRoute("/program/{controller}/{action}", conventional: true)]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("inheritingcontroller2noroute/getwithroute")]
        [ExpectRoute("/inheritingcontroller2noroute/getwithroute")]
        public Task<OkObjectResult> GetWithRoute()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET with route"));
        }
    }

    [ApiController]
    [Route("activeController")]
    public class ActiveControllerWithNoMethodRoute
    {
        [ExpectRoute("/activeController")]
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ActiveControllerWithNoMethodRoute"));
        }
    }

    [ApiController]
    [Route("controllerComplexHttpMethods")]
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

    [ApiController]
    public class ControllerWithNoRoutes;

    [ApiController]
    [Route("api")]
    public class ListAllRoutesController(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider
        ) : ControllerBase
    {
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;

        [HttpGet]
        [Route("attributeroutes")]
        public Task<OkObjectResult> ListAttributeRoutes()
        {
            var actionDescriptors = actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
            return Task.FromResult(new OkObjectResult(PathsExporter.ListAllRoutes(actionDescriptors, false)));
        }

        [HttpGet]
        [Route("conventionalroutes")]
        public Task<OkObjectResult> ListConventionalRoutes()
        {
            var actionDescriptors = actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
            return Task.FromResult(new OkObjectResult(PathsExporter.ListAllRoutes(actionDescriptors, true, false)));
        }
    }
}