using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnDnWebApi.Models
{
//    public class ThreeDateModel
//    {
//        public DateTime DropDate { get; set; }
//        public string UtcOtEtcOrPt { get; set; }
//        //public DateTime DropDateET { get; set; }
//}

    public class DropAuctionDateModel
    {
        public DateTime DropDate { get; set; }
        public string UtcOrEtcOrPt { get; set; }

    }
}