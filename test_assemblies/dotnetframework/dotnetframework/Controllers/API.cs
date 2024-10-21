using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace dotnetframework472.Api
{

    [RoutePrefix("apiattributecontrollerprefix")]
    public class ApiAttributeController : ApiController
    {

        public string Get() => "hello";


        [HttpGet]
        [Route("apigetwithroute")]
        public string GetWithRoute() => "hello";

        [HttpGet]
        [Route("apimultipleroutes/1")]
        [Route("apimultipleroutes/2")]
        public string MultipleRoutes() => "hello";

    }

    public class ApiAttributeControllerNoRouteController : ApiController
    {
        [Route("apiattributecontrollernoroute/getWithHttpGetRoute")]
        [HttpGet] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        public string Get() => "hello";


        [Route("apiattributecontrollernoroute/getwithroute")]
        public string GetWithRoute() => "hello";
    }

    [RoutePrefix("apiinheritingcontrollerprefix")]
    public class ApiInheritingController : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("apigetwithrouteprefix")]
        public string WithRoutePrefix() => "hello";
    }

    public class ApiInheritingControllerNoRouteController : ApiController
    {
        public string Get() => "hello";

        [Route("apiInheritingControllerNoRoute/getwithroute")]
        public string GetWithRoute() => "hello";

    }

    [RoutePrefix("apiinheritingcontroller2prefix")]
    public class ApiInheritingController2Controller : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("apigetwithroute2")]
        public string GetWithRoute() => "hello";

    }

    public abstract class CustomBase : ApiController { }

    public class ApiCustomBaseInheritingController : CustomBase
    {

        [HttpGet]
        [Route("apicustombase/route")]
        public string Get() => "hello";
    }

    public class ApiInheritingController2NoRouteController : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("apiinheritingcontroller2noroute/getwithroute")]
        public string GetWithRoute() => "hello";
    }

    [RoutePrefix("apiactiveController")]
    public class ApiActiveControllerWithNoMethodRouteController : ApiController
    {
        public string HelloWorld() => "hello";
    }

    [RoutePrefix("apicontrollerComplexHttpMethods")]
    public class ApiControllerComplexHttpMethodsController : ApiController
    {

        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public string HttpMethodWithSameRouteAsAnotherButNoMethod() => "post";

        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpGet]
        [HttpDelete]
        public string HttpMethodWithSameRouteAsAnotherButGetMethod() => "get and delete";

        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpPut]
        public string HttpMethodWithSameRouteAsAnotherButPostMethod() => "put";
    }

    [RoutePrefix("apiacceptverbsscontroller")]
    public class ApiAcceptVerbsController : ApiController
    {
        [Route("apiacceptverbsroute")]
        [AcceptVerbs("GET", "POST")]
        public string AcceptVerbsRoute() => "hello";

        [AcceptVerbs("PATCH", "DELETE")]
        public string AcceptVerbsWithoutRoute() => "hello";
    }


    [RoutePrefix("apicontrollerWithPrefixButOnlyVerbsOnMethods")]
    public class ApiControllerWithPrefixButOnlyVerbsOnMethodsController : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [HttpPost]
        public string Post() => "hello";

        [HttpPut]
        public string Put() => "hello";

        [HttpDelete]
        public string Delete() => "hello";

        [HttpPatch]
        public string Patch() => "hello";

        [HttpOptions]
        public string Options() => "hello";

        [HttpHead]
        public string Head() => "hello";

    }


    public class ApiDefaultConventionalApiController : ApiController
    {
        [HttpGet]
        public string DefaultAction() => "hello";

        [HttpGet]
        public string NonDefaultAction() => "hello";
    }

    public class ApiDefaultControllerConventionalController : ApiController
    {
        [NonAction]
        [HttpGet]
        public string DefaultAction() => "hello";

        public string NonDefaultAction() => "hello";
    }

    public class ApiConventionalControllerOverriddenActionNamesController : ApiController
    {
        [HttpGet]
        [ActionName("OverridenActionName")]
        public string Get() => "hello";

        [HttpPost]
        [ActionName("OverridenActionName")]
        public string Post() => "hello";
    }


    public class ApiControllerWithNoRoutesController : ApiController { };


    [RoutePrefix("api/[controller]")]
    public class ApiControllerWithPlaceholdersController : ApiController
    {
        [Route("[action]")]
        [ActionName("OverridenActionName")]
        public string Get() => "get";

        [Route("[action]")]
        public string Post() => "post";
    }

    [Route("apicontrollerwithverbnamedactions")]
    public class ApiControllerWithVerbNamedActionsController : ApiController
    {
        public string Get() => "get";

        public string Post() => "post";

        public string Put() => "put";

        public string Delete() => "delete";

        public string Patch() => "patch";

        public string Options() => "options";

        public string Head() => "head";
    }

    [RoutePrefix("apicontrollerwithrouteandprefixcontrollerprefix")]
    [Route("apicontrollerwithrouteandprefixcontrollerroute")]
    public class ApiControllerWithRouteAndPrefixController : ApiController
    {
        [Route("apiwithroute")]
        public string Get() => "hello";

        [HttpPost]
        public string P() => "hello";
    }

}