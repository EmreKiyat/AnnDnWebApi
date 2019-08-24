using AnnDnWebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using AnnDnWebApi.Classes;
using AnnDnWebApi.Security;
using System;

namespace AnnDnWebApi.Controllers
{
[ApiAuthorize(Roles = "R,T")] 
    public class DroppedDomainsController : ApiController
    {
        
        //[Route("~/api/DroppedDomains/{KulId:int}"),HttpGet]
        //public HttpResponseMessage Get(int KulId)
        //{
        //    Kuls kuls = new Classes.Kuls();
        //    kuls.CreateProductKeys();
        //    return Request.CreateResponse(HttpStatusCode.OK, "ddd");
        //}

               
        public HttpResponseMessage GetDrops()
        {
            //Data subscription'ın geçerliliği kontrolü
            if (!IsSubscribedDataService()) return Request.CreateResponse(HttpStatusCode.UpgradeRequired, "Renew your data subscription!");
            //****
            DropDomain dd = new DropDomain();
            List<DropDomainModelWebApi> DropDomainList = new List<DropDomainModelWebApi>();
            var ListOfTLDs = Request.Headers.GetValues("ListOfTLDs").FirstOrDefault();
            List<string> TLDs = JsonConvert.DeserializeObject<List<string>>(ListOfTLDs);

            string IsKeyword= Request.Headers.GetValues("IsKeyword").FirstOrDefault();
            char DropOrLastDrop= Convert.ToChar(Request.Headers.GetValues("DropOrLastDrop").FirstOrDefault());
            if (IsKeyword=="0")
            {
                var MatchCriteria = Request.Headers.GetValues("MatchCriteria").FirstOrDefault();
                MatchCriteriaWithPatternModel objMC = JsonConvert.DeserializeObject<MatchCriteriaWithPatternModel>(MatchCriteria);
                DropDomainList= dd.GetFilteredDropingDomains(objMC, TLDs, DropOrLastDrop);
            }
            if (IsKeyword == "1")
            {
                string IncludeHyphens = Request.Headers.GetValues("IncludeHyphens").FirstOrDefault();
                string IncludeNumbers = Request.Headers.GetValues("IncludeNumbers").FirstOrDefault();
                var Keywords = Request.Headers.GetValues("Keywords").FirstOrDefault();
                List<KeywordModel> objKeywords = JsonConvert.DeserializeObject<List<KeywordModel>>(Keywords);
                DropDomainList = dd.GetDropingDomainsByKeywords(IncludeHyphens, IncludeNumbers,objKeywords, TLDs, DropOrLastDrop);
            }

            return Request.CreateResponse(HttpStatusCode.OK, DropDomainList);
            //return Request.CreateResponse(HttpStatusCode.OK, HttpContext.Current.User.Identity.Name);

        }


        [Route("~/api/DroppedDomains/GetCounts/{TableId:int}"), HttpGet]
        public HttpResponseMessage GetCounts(int TableId)
        {
            string TableName = "";
            if (TableId == 1) TableName = "DomainsDroping";
            if (TableId == 2) TableName = "DomainsAuctions";
            if (TableId == 3) TableName = "DomainsPreRelease";
            if (TableId == 4) TableName = "DomainsDropedLastWeek";
            DropDomain dd = new DropDomain();
            
            return Request.CreateResponse(HttpStatusCode.OK, dd.GetTableRowCount(TableName));
        }

        #region NonPaged
        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetAuctions()
        {
            //Data subscription'ın geçerliliği kontrolü
            if (!IsSubscribedDataService()) return Request.CreateResponse(HttpStatusCode.UpgradeRequired, "Renew your data subscription!");
            //****
            DropDomain dd = new DropDomain();
            List<AuctionDomainModelWebApi> AuctionDomainList = new List<AuctionDomainModelWebApi>();

            var ListOfTLDs = Request.Headers.GetValues("ListOfTLDs").FirstOrDefault();
            List<string> TLDs = JsonConvert.DeserializeObject<List<string>>(ListOfTLDs);

            string IsKeyword = Request.Headers.GetValues("IsKeyword").FirstOrDefault();
            char AuctionType = Convert.ToChar(Request.Headers.GetValues("AuctionType").FirstOrDefault());
            if (IsKeyword == "0")
            {
                var MatchCriteria = Request.Headers.GetValues("MatchCriteria").FirstOrDefault();
                MatchCriteriaWithPatternModel objMC = JsonConvert.DeserializeObject<MatchCriteriaWithPatternModel>(MatchCriteria);
                AuctionDomainList = dd.GetFilteredAuctionDomains(objMC, TLDs, AuctionType);
            }
            if (IsKeyword == "1")
            {
                string IncludeHyphens = Request.Headers.GetValues("IncludeHyphens").FirstOrDefault();
                string IncludeNumbers = Request.Headers.GetValues("IncludeNumbers").FirstOrDefault();
                var Keywords = Request.Headers.GetValues("Keywords").FirstOrDefault();
                List<KeywordModel> objKeywords = JsonConvert.DeserializeObject<List<KeywordModel>>(Keywords);
                AuctionDomainList = dd.GetAuctionDomainsByKeywords(IncludeHyphens, IncludeNumbers, objKeywords, TLDs, AuctionType);
            }

            return Request.CreateResponse(HttpStatusCode.OK, AuctionDomainList);
        }

        [Route("~/api/DroppedDomains/IsDomainInAuction/{Domain}/{Ext}"), HttpGet]
        public HttpResponseMessage IsDomainInAuction(string Domain,string Ext)
        {
            DropDomain dd = new DropDomain();
            try
            {
                AuctionDomainModelWebApi ret = new AuctionDomainModelWebApi();
                ret = dd.IsDomainInAuction(Domain, Ext);
                if (ret==null) return Request.CreateResponse(HttpStatusCode.NotFound, "Domain not in Auction");
                else
                    return Request.CreateResponse(HttpStatusCode.OK,ret);
            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, Ex.InnerException);
            }
            
        }

        [Route("~/api/DroppedDomains/GetDropDate/{Domain}/{Ext}"), HttpGet]
        public HttpResponseMessage GetDropDate(string Domain, string Ext)
        {
            DropAuctionDateModel ret;
            DropDomain dd = new DropDomain();
            try
            {
                ret = dd.GetDropDate(Domain, Ext); 
                if (ret == null) return Request.CreateResponse(HttpStatusCode.NotFound, "Domain not in DomainsDroping");
                else
                    return Request.CreateResponse(HttpStatusCode.OK, ret);
            }
            catch (Exception Ex)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable, Ex.InnerException);
            }

        }
        #endregion NonPaged

        private bool IsSubscribedDataService()
        {
            //Data subscription'ın geçerliliği kontrolü
            string cHashKey = Request.Headers.GetValues("cHashKey").FirstOrDefault();
            string ProductKey = Request.Headers.GetValues("ProductKey").FirstOrDefault();
            Kuls Kuls = new Kuls();
            return Kuls.IsSubscribedForData(ProductKey, cHashKey);

            //***************
        }
    }
}
