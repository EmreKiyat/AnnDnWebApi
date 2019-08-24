using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnDnWebApi.Models
{
    public class SenkronizationModel
    {
        public class ChangedModel
        {
            public string ChangedTable { get; set; }
            public int LastChangedId { get; set; }
        }

        public class WhoIsServersModel
        {
            //WhoIsServer için
            public int ID { get; set; }
            public string Server { get; set; }
            public string TLD { get; set; }
            public string ReqType { get; set; }
            public int IsActive { get; set; }
            public int Priority { get; set; }
        }


        public class zSourceModel
        {
            public int SourceId { get; set; }
            public string SourceName { get; set; }
            public string RootLink { get; set; }
            public string LinkModelDrop { get; set; }
            public string LinkModelAuction { get; set; }
            public int IsActive { get; set; }
            public int IsAuction { get; set; }
            public int IsDrop { get; set; }
            public string AEk { get; set; }
        }
        public class sMobilControlModel
        {
            public int MobilId { get; set; }
            public string Mobil { get; set; }
            public string AppName { get; set; }
            public int IsActivated { get; set; }
            public string Link { get; set; }
        }
        public class zEMailListForPrivateRegistrationModel
        {
            public int ID { get; set; }
            public string EMail { get; set; }
        }

        public class zRegistrarCompanyModel
        {
            public int RegistrarIANAId { get; set; }
            public string Registrar { get; set; }
            public string Web { get; set; }
            public string UrlSearch { get; set; }
            public string UrlReNew { get; set; }
        }

        public class zAuctionCompanyModel
        {
            public string Company { get; set; }
            public string Url { get; set; }
            public string AuchUrl { get; set; }
            public string AuctionOrRegistrant { get; set; }
            public string RegUrl { get; set; }
        }
        public class zTrendTaskRssTemplate
        {
            public string Url { get; set; }
            public string RssRoot { get; set; }
            public string RssCategory { get; set; }
        }
    }
}