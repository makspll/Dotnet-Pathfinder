
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using MVCWithLatestBootstrapTemplate;
using TestUtils;

namespace dotnetframework472
{

    [System.Web.Mvc.RoutePrefix("api")]
    public class ListAllRoutesController : Controller
    {


        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("attributeroutes")]
        public async Task<ActionResult> ListAttributeRoutes()
        {
            // return await Task.FromResult(Json(new RouteInfo(new List<string>(), new List<string>()), JsonRequestBehavior.AllowGet));
            return await Task.FromResult(Json(PathExporter.ListAllRoutes(RouteConfig.AllRoutes, true), JsonRequestBehavior.AllowGet));

        }

        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("conventionalroutes")]
        public async Task<ActionResult> ListConventionalRoutes()
        {
            return await Task.FromResult(Json(PathExporter.ListAllRoutes(RouteConfig.AllRoutes, true, false), JsonRequestBehavior.AllowGet));
        }
    }
}