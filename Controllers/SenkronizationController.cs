using AnnDnWebApi.Classes;
using AnnDnWebApi.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using static AnnDnWebApi.Models.SenkronizationModel;

namespace AnnDnWebApi.Controllers
{
    [ApiAuthorize(Roles = "R")]
    public class SenkronizationController : ApiController
    {
        
        public HttpResponseMessage GetChanged()
        {
            List<ChangedModel> SenkList = new List<ChangedModel>();
            Senkronization sn = new Senkronization();
            SenkList = sn.GetChanged();
            var response=Request.CreateResponse(HttpStatusCode.OK, SenkList);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = new TimeSpan(0, 6, 0, 0)
            };
            return response;
        }
    

    [ApiAuthorize(Roles = "R,T")]
    public HttpResponseMessage GetLastServerList()
    {
        Senkronization Synch = new Senkronization();
        List<WhoIsServersModel> ret = Synch.GetLastChangedWhoIsServerList();
        return Request.CreateResponse(HttpStatusCode.OK, ret);
    }

        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetZSourceList()
        {
            Senkronization Synch = new Senkronization();
            List<zSourceModel> ret = Synch.GetZSourceList();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

        //[ApiAuthorize(Roles = "R,T")]
        //public HttpResponseMessage GetSMobilControl()
        //{
        //    Senkronization Synch = new Senkronization();
        //    List<sMobilControlModel> ret = Synch.GetSMobilControl();
        //    return Request.CreateResponse(HttpStatusCode.OK, ret);
        //}

        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetZEMailListForPrivateRegistration()
        {
            Senkronization Synch = new Senkronization();
            List<zEMailListForPrivateRegistrationModel> ret = Synch.GetZEMailListForPrivateRegistration();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetZRegistrarCompany()
        {
            Senkronization Synch = new Senkronization();
            List<zRegistrarCompanyModel> ret = Synch.GetZRegistrarCompany();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }
        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetZAuctionCompany()
        {
            Senkronization Synch = new Senkronization();
            List<zAuctionCompanyModel> ret = Synch.GetZAuctionCompany();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }
        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetZTrendTaskRssTemplate()
        {
            Senkronization Synch = new Senkronization();
            List<zTrendTaskRssTemplate> ret = Synch.GetZTrendTaskRssTemplate();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }
        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetWebApiServerBaseAddr()
        {
            Senkronization Synch = new Senkronization();
            string ret = Synch.GetWebApiServerBaseAddr();
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

        [ApiAuthorize(Roles = "R,T")]
        [Route("~/api/Senkronization/AddUsersMostUsedIANAId/{IanaId:int}"), HttpGet]
        public HttpResponseMessage AddUsersMostUsedIANAId(int IanaId)
        {
            Senkronization Synch = new Senkronization();
            try
            {
                Synch.AddUsersMostUsedIANAId(IanaId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented);

            }
            
            
        }



    }
}
