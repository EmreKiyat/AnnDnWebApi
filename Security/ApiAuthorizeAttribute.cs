using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace AnnDnWebApi.Security
{
    public class ApiAuthorizeAttribute:AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var actionRoles = Roles;
            string userId = HttpContext.Current.User.Identity.Name;
            //var Role = (string)HttpContext.Current.Items["KulRole"];
            var Role = HttpContext.Current.User.Identity.AuthenticationType;
            if (!actionRoles.Contains(Role) || Role=="0")       //kullanıcı bulunamadıysa Role=0 olan dummy bir kullanıcı oluşturmuştum ApiKeyHandler'de
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            
            //base.OnAuthorization(actionContext);
        }
    }
}