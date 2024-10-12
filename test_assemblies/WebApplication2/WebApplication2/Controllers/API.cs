using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace dotnetframework472.Api
{
    
    [Route("apiattributecontrollerprefix2")]
    [Route("apiattributecontrollerprefix")]
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

    public class ApiAttributeControllerNoRoute : ApiController
    {   
        [Route("apiattributecontrollernoroute/getWithHttpGetRoute")]
        [HttpGet] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        public string Get() => "hello";


        [Route("apiattributecontrollernoroute/getwithroute")]
        public string GetWithRoute() => "hello";
    }

    [Route("apiinheritingcontrollerprefix")]
    [Route("apiinheritingcontrollerprefix2")]
    public class ApiInheritingController : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("apigetwithrouteprefix")]
        public string GetWithRoute() => "hello";
    }

    public class ApiInheritingControllerNoRoute : ApiController
    {
        public string Get() => "hello";

        [Route("apiInheritingControllerNoRoute/getwithroute")]
        public string GetWithRoute() => "hello";

    }

    [Route("apiinheritingcontroller2prefix")]
    [Route("apiinheritingcontroller2prefix2")]
    public class ApiInheritingController2 : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("getwithroute")]
        public string GetWithRoute() => "hello";

    }

    public abstract class CustomBase : ApiController {}

    public class ApiCustomBaseInheritingController : CustomBase
    {

        [HttpGet]
        [Route("apicustombase/route")]
        public string Get() => "hello";
    }

    public class ApiInheritingController2NoRoute : ApiController
    {
        [HttpGet]
        public string Get() => "hello";

        [Route("apiinheritingcontroller2noroute/getwithroute")]
        public string GetWithRoute() => "hello";
    }

    [Route("apiactiveController")]
    [Route("apiactiveController2")]
    public class ApiActiveControllerWithNoMethodRoute : ApiController
    {
        public string HelloWorld() => "hello";
    }

    [Route("apicontrollerComplexHttpMethods")]
    [Route("apicontrollerComplexHttpMethods2")]
    public class ApiControllerComplexHttpMethods : ApiController
    {
        
        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public string HttpMethodWithSameRouteAsAnotherButNoMethod() => "hello";
            
        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public string HttpMethodWithSameRouteAsAnotherButGetMethod() => "hello";
        
        [Route("apiHttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpPost]
        public string HttpMethodWithSameRouteAsAnotherButPostMethod() => "hello";

    }

    [Route("apiacceptverbscontroller")]
    public class ApiAcceptVerbsController : ApiController
    {
        [Route("apiacceptverbsroute")]
        [AcceptVerbs("GET", "POST")]
        public string AcceptVerbsRoute() => "hello";

        [AcceptVerbs("PATCH", "DELETE")]
        public string AcceptVerbsWithoutRoute() => "hello";
    }


    [Route("apicontrollerWithPrefixButOnlyVerbsOnMethods")]
    public class ApiControllerWithPrefixButOnlyVerbsOnMethods : ApiController
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
        [Route("test/asd")]
        public string  DefaultAction() => "hello";

        [HttpGet]
        public string NonDefaultAction() => "hello";
    }

    public class ApiDefaultControllerConventional : ApiController
    {
        [NonAction]
        [HttpGet]
        public string DefaultAction() => "hello";

        public string NonDefaultAction() => "hello";
    }

    public class ApiConventionalControllerOverriddenActionNames : ApiController
    {
        [HttpGet]
        [ActionName("OverridenActionName")]
        public string Get() => "hello";

        [HttpPost]
        [ActionName("OverridenActionName")]
        public string Post() => "hello";
    }


    public class ApiControllerWithNoRoutes : ApiController {};


    [Route("api/[controller]")]
    public class ApiControllerWithPlaceholders : ApiController
    {
        [Route("[action]")]
        [ActionName("OverridenActionName")]
        public string Get() => "hello";
        
        [Route("[action]")]
        public string Post() => "hello";
    }
}