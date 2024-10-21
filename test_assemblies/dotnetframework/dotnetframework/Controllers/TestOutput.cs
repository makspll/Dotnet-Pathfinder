using System.Collections.Generic;
using System.Web.Http;
using TestUtils;


namespace dotnetframework.Controllers
{
    [RoutePrefix("api")]
    public class ListAllRoutesController : ApiController
    {

        [HttpGet]
        [Route("attributeroutes")]
        public List<RouteInfo> Get() => PathExporter.ListAllRoutes(false, true);

        [HttpGet]
        [Route("conventionalroutes")]
        public List<RouteInfo> ListConventionalRoutes() => PathExporter.ListAllRoutes(true, false);
    }
}