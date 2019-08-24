using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AnnDnWebApi.Classes;
using AnnDnWebApi.Models;
using Newtonsoft.Json;

namespace AnnDnWebApi.Security
{
    public class ApiKeyHandler:DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Authorize gerek olmayan api'ler için AuthJson'da ProductKey olarak 0 gönderiyorum, gereksiz yere GetKulByPr... sp'sini çalıştırmıyorum böylece
            //var AuthObj=request.Headers.GetValues("AuthJson").FirstOrDefault();
            AuthenticationModel comingAuth = new AuthenticationModel();
            comingAuth.cHashKey = request.Headers.GetValues("cHashKey").FirstOrDefault();
            comingAuth.ProductKey = request.Headers.GetValues("ProductKey").FirstOrDefault();
            //comingAuth = JsonConvert.DeserializeObject<AuthenticationModel>(AuthObj); //Deserializasyondan kurtulmak için AuthJson yerine cHash ve Product key'leri ayrı ayrı göndereceğim. 27.9.2018

            KulModel Kul = new KulModel();
            Kuls Kuls = new Kuls();
            if (comingAuth.ProductKey != "0") Kul = Kuls.GetKulByProductKeyAndCHashKey(comingAuth.ProductKey, comingAuth.cHashKey.ToString()); else Kul = null;
            var principal0 = new ClaimsPrincipal(new GenericIdentity("0000", "0"));
            if (Kul != null)
            {
                var principal = new ClaimsPrincipal(new GenericIdentity(Kul.ProductKey.ToString(), Kul.Role));
                HttpContext.Current.User = principal;
            } else HttpContext.Current.User = principal0;
            return base.SendAsync(request, cancellationToken);
        }
    }
}