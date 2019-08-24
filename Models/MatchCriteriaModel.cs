using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnDnWebApi.Models
{
    public class MatchCriteriaWithPatternModel
    {
        //public int MatchCriteriaId { get; set; }
        //public string MatchCriteria { get; set; }
        public string Consonants { get; set; }
        public string Vowels { get; set; }
        public string Numbers { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int HasSlash { get; set; }
        //public string MainRegEx { get; set; }
        public List<MatchPatternModel> Patterns { get; set; }
    }

    public class KeywordModel
    {
        public string Keyword { get; set; }
        public int KeywordType { get; set; }    //1:begin with, 2:end with, 3:include
        public bool IsConsistent { get; set; }  //true:white,false:red
    }

    public class MatchPatternModel
    {
        public int PatternId { get; set; }
        public string Pattern { get; set; }
        public int PatternType { get; set; }    //0:Begin With, 1:End With, 2: Do not use
        public string Header { get; set; }
        public string Baglac { get; set; }
        public bool IsConsistent { get; set; }
        //public string RegEx { get; set; }
    }

    public class BaseDomainModel
    {
        public int Id { get; set; }
        public string Domain { get; set; }  //0-77
        public string Ext { get; set; }
        public int SourceId { get; set; }
        public string SourceKey { get; set; }
    }
        public class DropDomainModelWebApi:BaseDomainModel
    {
        //public DateTime DropDatePT { get; set; }
        //public DateTime DropDateET { get; set; }
        //public DateTime DropDateUTC { get; set; }   //Universal Time - Greenwich
        public DateTime DropDate { get; set; }
        public string UtcOrEtcOrPt { get; set; }
    }
    public class DropDomainModelWebApiPaged
    {
        public List<DropDomainModelWebApi> DomainList { get; set; }
        public int NumOfRecord { get; set; }
        public List<DateTime> DropDates { get; set; }   //artık tüm data gelmediği için distinct alarak DropDates combosunu dolduramıyorum, Sonuç: serverden gelecek
    }
    public class AuctionDomainModelWebApi:BaseDomainModel
    {
        //public DateTime AuctionEndDatePT { get; set; }
        //public DateTime AuctionEndDateET { get; set; }
        //public DateTime AuctionEndDateUTC { get; set; }   //Universal Time - Greenwich
        public DateTime AuctionEndDate { get; set; }
        public string UtcOrEtcOrPt { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal BuyNowPrice { get; set; }
    }
    public class AuctionDomainModelWebApiPaged
    {
        public List<AuctionDomainModelWebApi> DomainList { get; set; }
        public int NumOfRecord { get; set; }
    }
    public class SqlParamModel
    {
        public int Offset { get; set; }
        public int NumOfWordsPerPage { get; set; }
        public string OrderBy { get; set; }
        public string SearchText { get; set; }
    }
}