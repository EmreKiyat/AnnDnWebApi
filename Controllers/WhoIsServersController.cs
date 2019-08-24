using AnnDnWebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using AnnDnWebApi.Classes;
using AnnDnWebApi.Security;

namespace AnnDnWebApi.Controllers
{
    [ApiAuthorize(Roles = "R")]
    public class WhoIsServersController : ApiController
    {
        //public HttpResponseMessage GetLastServerListId()
        //{
        //    //kullanıcı bu servisten dönen değer kendi son update ID'sinden farklı ise Server listi isteyecek
        //    var _myLastUpdatedServerListId = Request.Headers.GetValues("MyLastUpdatedServerListId").FirstOrDefault();
        //    int MyLastUpdatedServerListId = JsonConvert.DeserializeObject<int>(_myLastUpdatedServerListId);
        //    WhoIsServers WhoIsServ = new WhoIsServers();
        //    int ret = WhoIsServ.GetLastChangedWhoIsServerListId();
        //    return Request.CreateResponse(HttpStatusCode.OK, ret);
        //}

    }
}
