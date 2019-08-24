using AnnDnWebApi.Classes;
using AnnDnWebApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AnnDnWebApi.Controllers
{

    //[ApiAuthorize(Roles = "R,T")]
    public class LicanceController : ApiController
    {
        [Route("~/api/Licance/CheckRegistryFromServer/"), HttpGet]
        public HttpResponseMessage CheckRegistryFromServer()
        {
            try
            {
                string CurrentCHash = Request.Headers.GetValues("CurrentCHash").FirstOrDefault();
                string ProductKey = Request.Headers.GetValues("ProductKey").FirstOrDefault();
                Kuls kuls = new Kuls();
                RegistryObject retRegistryObject = new RegistryObject();
                retRegistryObject = kuls.CheckRegistryFromServer(ProductKey, CurrentCHash);
                return Request.CreateResponse(HttpStatusCode.OK, retRegistryObject);
            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, Ex.Message);
            }

        }


    }
}
