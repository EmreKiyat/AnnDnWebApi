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
using System.Web.Http.Cors;

namespace AnnDnWebApi.Controllers
{
    //[ApiAuthorize(Roles = "R,T")]
    public class DomainsDataController : ApiController
    {
        #region Paged
        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetDropsPaged()
        {
            //Data subscription'ın geçerliliği kontrolü
            if (!IsSubscribedDataService()) return Request.CreateResponse(HttpStatusCode.UpgradeRequired, "Renew your data subscription!");
            //****
            DomainData dd = new DomainData();
            DropDomainModelWebApiPaged DropDomainList = new DropDomainModelWebApiPaged();
            var ListOfTLDs = Request.Headers.GetValues("ListOfTLDs").FirstOrDefault();
            List<string> TLDs = JsonConvert.DeserializeObject<List<string>>(ListOfTLDs);

            string IsKeyword = Request.Headers.GetValues("IsKeyword").FirstOrDefault();
            char DropOrLastDrop = Convert.ToChar(Request.Headers.GetValues("DropOrLastDrop").FirstOrDefault());
            //for paging+filters
            int NumOfRecPerPage = Convert.ToInt32(Request.Headers.GetValues("NumOfRecPerPage").FirstOrDefault());
            int WhichPage = Convert.ToInt32(Request.Headers.GetValues("WhichPage").FirstOrDefault());
            char SortBy = Convert.ToChar(Request.Headers.GetValues("SortBy").FirstOrDefault());
            string SearchStr = Request.Headers.GetValues("SearchStr").FirstOrDefault();
            
            //string KeyWord = Request.Headers.GetValues("KeyWord").FirstOrDefault();
            string dropDate = Request.Headers.GetValues("dropDate").FirstOrDefault();
            //****
            //for excel
            bool IsExcel = (Request.Headers.GetValues("IsExcel").FirstOrDefault()=="1")?true:false;

            if (IsKeyword == "0")
            {
                var MatchCriteria = Request.Headers.GetValues("MatchCriteria").FirstOrDefault();
                MatchCriteriaWithPatternModel objMC = JsonConvert.DeserializeObject<MatchCriteriaWithPatternModel>(MatchCriteria);
                DropDomainList = dd.GetFilteredDropingDomainsPaged(objMC, TLDs, DropOrLastDrop, SortBy, SearchStr, NumOfRecPerPage, WhichPage,dropDate,IsExcel);
            }
            if (IsKeyword == "1")
            {
                string IncludeHyphens = Request.Headers.GetValues("IncludeHyphens").FirstOrDefault();
                string IncludeNumbers = Request.Headers.GetValues("IncludeNumbers").FirstOrDefault();
                var Keywords = Request.Headers.GetValues("Keywords").FirstOrDefault();
                List<KeywordModel> objKeywords = JsonConvert.DeserializeObject<List<KeywordModel>>(Keywords);
                DropDomainList = dd.GetDropingDomainsByKeywordsPaged(IncludeHyphens, IncludeNumbers, objKeywords, TLDs, DropOrLastDrop, SortBy, SearchStr, NumOfRecPerPage, WhichPage, dropDate, IsExcel);
            }

            return Request.CreateResponse(HttpStatusCode.OK, DropDomainList);
            //return Request.CreateResponse(HttpStatusCode.OK, HttpContext.Current.User.Identity.Name);
        }

        [ApiAuthorize(Roles = "R,T")]
        public HttpResponseMessage GetAuctionsPaged()
        {
            //Data subscription'ın geçerliliği kontrolü
            if (!IsSubscribedDataService()) return Request.CreateResponse(HttpStatusCode.UpgradeRequired, "Renew your data subscription!");
            //****
            DomainData dd = new DomainData();
            AuctionDomainModelWebApiPaged AuctionDomainList = new AuctionDomainModelWebApiPaged();

            var ListOfTLDs = Request.Headers.GetValues("ListOfTLDs").FirstOrDefault();
            List<string> TLDs = JsonConvert.DeserializeObject<List<string>>(ListOfTLDs);

            string IsKeyword = Request.Headers.GetValues("IsKeyword").FirstOrDefault();
            char AuctionType = Convert.ToChar(Request.Headers.GetValues("AuctionType").FirstOrDefault());
            //for paging+filters
            int NumOfRecPerPage = Convert.ToInt32(Request.Headers.GetValues("NumOfRecPerPage").FirstOrDefault());
            int WhichPage = Convert.ToInt32(Request.Headers.GetValues("WhichPage").FirstOrDefault());
            char SortBy= Convert.ToChar(Request.Headers.GetValues("SortBy").FirstOrDefault());
            string SearchStr=Request.Headers.GetValues("SearchStr").FirstOrDefault();
            //****
            //for excel
            bool IsExcel = (Request.Headers.GetValues("IsExcel").FirstOrDefault() == "1") ? true : false;
            //****
            if (IsKeyword == "0")
            {
                var MatchCriteria = Request.Headers.GetValues("MatchCriteria").FirstOrDefault();
                MatchCriteriaWithPatternModel objMC = JsonConvert.DeserializeObject<MatchCriteriaWithPatternModel>(MatchCriteria);
                AuctionDomainList = dd.GetFilteredAuctionDomainsPaged(objMC, TLDs, AuctionType,SortBy,SearchStr, NumOfRecPerPage,WhichPage,IsExcel);
            }
            if (IsKeyword == "1")
            {
                string IncludeHyphens = Request.Headers.GetValues("IncludeHyphens").FirstOrDefault();
                string IncludeNumbers = Request.Headers.GetValues("IncludeNumbers").FirstOrDefault();
                var Keywords = Request.Headers.GetValues("Keywords").FirstOrDefault();
                List<KeywordModel> objKeywords = JsonConvert.DeserializeObject<List<KeywordModel>>(Keywords);
                AuctionDomainList = dd.GetAuctionDomainsByKeywordsPaged(IncludeHyphens, IncludeNumbers, objKeywords, TLDs, AuctionType, SortBy, SearchStr, NumOfRecPerPage, WhichPage,IsExcel);
            }

            return Request.CreateResponse(HttpStatusCode.OK, AuctionDomainList);
        }
        #endregion Paged

        [Route("~/api/DomainsData/GetCounts/{TableId:int}"), HttpGet]
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
        [Route("api/DomainsData/TestApi")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage TestApi()
        {
            
            return Request.CreateResponse(HttpStatusCode.OK, 42);
        }

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
