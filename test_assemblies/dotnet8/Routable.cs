using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
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
        [HttpGet("getWithHttpGetRoute")] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        [ExpectRoute("/getWithHttpGetRoute")]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }


        [Route("getwithroute")]
        [ExpectRoute("/getwithroute")]
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
        [HttpGet]
        [ExpectRoute("/program/{controller}/{action}", conventional: true)]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        [ExpectRoute("/getwithroute")]
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

    public class InheritingController2NoRoute : Controller
    {
        [HttpGet]
        [ExpectRoute("/program/{controller}/{action}", conventional: true)]
        public Task<OkObjectResult> Get()
        {
            return Task.FromResult(new OkObjectResult("With APIController attribute GET"));
        }

        [Route("getwithroute")]
        [ExpectRoute("/getwithroute")]
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
    [Route("api")]
    public class ListAllRoutesController(
        IEnumerable<EndpointDataSource> endpointSources
        ) : ControllerBase
    {
        private readonly IEnumerable<EndpointDataSource> _endpointSources = endpointSources;

        [HttpGet]
        [Route("allroutes")]
        [ExpectRoute("/api/allroutes")]
        public Task<OkObjectResult> ListAllRoutes()
        {
            return Task.FromResult(new OkObjectResult(PathsExporter.ListAllRoutes(_endpointSources)));
        }
    }
}