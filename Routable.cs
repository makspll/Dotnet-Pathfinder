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

    [ApiController]
    [Route("acceptverbscontroller")]
    public class AcceptVerbsController : Controller
    {
        [AcceptVerbs("GET", "POST", Route = "acceptverbsroute")]
        public Task AcceptVerbsRoute() => Task.FromResult(new OkResult());

        [AcceptVerbs("PATCH", "DELETE")]
        public Task AcceptVerbsWithoutRoute() => Task.FromResult(new OkResult());
    }


    [ApiController]
    [Route("controllerWithPrefixButOnlyVerbsOnMethods")]
    public class ControllerWithPrefixButOnlyVerbsOnMethods
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new OkResult());

        [HttpPost]
        public Task Post() => Task.FromResult(new OkResult());

        [HttpPut]
        public Task Put() => Task.FromResult(new OkResult());

        [HttpDelete]
        public Task Delete() => Task.FromResult(new OkResult());

        [HttpPatch]
        public Task Patch() => Task.FromResult(new OkResult());

        [HttpOptions]
        public Task Options() => Task.FromResult(new OkResult());

        [HttpHead]
        public Task Head() => Task.FromResult(new OkResult());

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

    public class ConventionalControllerOverriddenActionNames : Controller
    {
        [HttpGet]
        [ActionName("OverridenActionName")]
        public Task<OkObjectResult> Get() => Task.FromResult(new OkObjectResult("ConventionalControllerOverriddenActionNames"));

        [HttpPost]
        [ActionName("OverridenActionName")]
        public Task<OkObjectResult> Post() => Task.FromResult(new OkObjectResult("ConventionalControllerOverriddenActionNamesPost"));
    }


    [ApiController]
    public class ControllerWithNoRoutes;


    [Route("[controller]")]
    public class ControllerWithPlaceholders : Controller
    {
        [HttpGet("[action]")]
        [ActionName("OverridenActionName")]
        public Task<OkObjectResult> Get() => Task.FromResult(new OkObjectResult("ControllerWithPlaceholders"));

        [HttpPost("[action]")]
        public Task<OkObjectResult> Post() => Task.FromResult(new OkObjectResult("ControllerWithPlaceholdersPost"));
    }
}