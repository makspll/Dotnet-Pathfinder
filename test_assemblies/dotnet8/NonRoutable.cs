using Microsoft.AspNetCore.Mvc;
using TestUtils;

namespace dotnet8
{
    [Route("api/withoutApiControllerAttributeWithRoutes")]
    public class ControllerWithoutApiControllerAttributeAndRoutes
    {
        [HttpGet]
        [Route("hello")]
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ControllerWithoutApiControllerAttributeAndRoutes"));
        }
    }

    public class ControllerWithHttpMethodAndRouteConfig
    {
        [HttpGet]
        public Task<OkObjectResult> HelloWorld()
        {
            return Task.FromResult(new OkObjectResult("ControllerWithHttpMethodAndRouteConfig"));
        }
    }

}