using Microsoft.AspNetCore.Mvc;
using TestUtils;

namespace dotnet8
{
    [Route("api/withoutApiControllerAttributeWithRoutes")]
    public class ControllerWithoutApiControllerAttributeAndRoutes
    {
        [HttpGet]
        [Route("hello")]
        [ExpectNoRoute]
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ControllerWithoutApiControllerAttributeAndRoutes"));
        }
    }

    public class ControllerWithHttpMethodAndRouteConfig
    {
        [HttpGet]
        [ExpectNoRoute]
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ControllerWithHttpMethodAndRouteConfig"));
        }
    }

}