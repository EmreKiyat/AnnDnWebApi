using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using AnnDnWebApi.Models;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace AnnDnWebApi.Classes
{
    public class DomainData
    {
        readonly List<char> lstConsonants = new List<char>(new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' });
        readonly List<char> lstVowels = new List<char>(new char[] { 'a', 'e', 'i', 'o', 'u' });
        readonly List<char> lstNumbers = new List<char>(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

        string myConn = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        public DropDomainModelWebApiPaged GetFilteredDropingDomainsPaged(MatchCriteriaWithPatternModel mc, List<string> TLDs, char DropOrLastDrop,char SortBy,string SearchStr,int NumOfRecPerPage, int WhichPage, string dropDate, bool IsExcel)     //,SqlParamModel SqlParams)
        {
            int NumOfRecord = 0;
            List<DropDomainModelWebApi> ret = new List<DropDomainModelWebApi>();
            string SqlPartChars = GetSqlPartForChar(mc);
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlPatternsPart = GetSqlPartForPatterns(mc);
            List<DropDatesType> DropDates=new List<DropDatesType>();
            string _table = (DropOrLastDrop == 'D') ? "DomainsDroping" : "DomainsDropedLastWeek";

            string Sql = "select * from " + _table + " with (nolock) ";
            Sql += "where uzunluk between @MinLength and @MaxLength ";
            Sql += SqlTLDsPart;
            Sql += SqlPartChars;
            if (mc.HasSlash == 0) Sql += " and HasHyphens = 0";  //HasHyphens=1 durumunda sql'e bişi ekleme eklersen sadece slash'lı olanları getirir.
            Sql += SqlPatternsPart;
            GetSqlForSearchSortAndPaging(ref Sql, SortBy, SearchStr, dropDate, NumOfRecPerPage, WhichPage,IsExcel);
            string SqlForNumOfRec = Sql.Substring(0, Sql.IndexOf("order by") - 1).Replace("select *", "select count(Id)");
            string SqlLog = "insert into SqlLog(Sql,FromWhere,SqlPatternsPart) values (@Sql,'GetFilteredDropingDomainsPaged',@SqlPatternsPart)";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                conn.Execute(SqlLog, new { @Sql = Sql, @SqlPatternsPart = SqlForNumOfRec });
                ret = conn.Query<DropDomainModelWebApi>(Sql, mc).ToList();
                NumOfRecord= conn.Query<int>(SqlForNumOfRec, mc).FirstOrDefault();
                if (WhichPage == 1 && SortBy == '9' && SearchStr == "" && dropDate == "A")   //filtreli sorgularda drop date listesine ihtiyaç yok, zaten ilk objeyi oluştururken ilk sayfa sorgusunda almıştı
                    DropDates = conn.Query<DropDatesType>("DropAuctionOp.GetDropDates", commandType: CommandType.StoredProcedure).ToList();

                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            //OrganizeDate(ref ret);
            DropDomainModelWebApiPaged retPaged = new DropDomainModelWebApiPaged();
            retPaged.DomainList = ret;
            retPaged.NumOfRecord = NumOfRecord;

            retPaged.DropDates = DropDates.Where(x=>TLDs.Contains(x.Ext) && x.DropOrLastDrop== DropOrLastDrop).Select(y=>y.DropDate).Distinct().ToList() ;
            return retPaged;
        }


        public AuctionDomainModelWebApiPaged GetFilteredAuctionDomainsPaged(MatchCriteriaWithPatternModel mc, List<string> TLDs, char AuctionType,char SortBy, string SearchStr, int NumOfRecPerPage, int WhichPage,bool IsExcel)
        {
            int NumOfRecord = 0;
            List<AuctionDomainModelWebApi> ret = new List<AuctionDomainModelWebApi>();
            string SqlPartChars = GetSqlPartForChar(mc);
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlPatternsPart = GetSqlPartForPatterns(mc);
            string _table = (AuctionType == 'A') ? "DomainsAuctions" : "DomainsPreRelease";
            string Sql = "select * from " + _table + " with (nolock) ";
            Sql += "where uzunluk between @MinLength and @MaxLength ";
            Sql += SqlTLDsPart;
            Sql += SqlPartChars;
            if (mc.HasSlash == 0) Sql += " and HasHyphens = 0";
            Sql += SqlPatternsPart;
            GetSqlForSearchSortAndPaging(ref Sql, SortBy, SearchStr,"A", NumOfRecPerPage, WhichPage,IsExcel);   //Auction için dropDate kullanılmıyor onun için "A"->etkisiz
            string SqlForNumOfRec = Sql.Substring(0, Sql.IndexOf("order by") - 1).Replace("select *", "select count(Id)");
            string SqlLog = "insert into SqlLog(Sql,FromWhere) values (@Sql,'GetFilteredAuctionDomainsPaged')";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                conn.Execute(SqlLog, new { @Sql = Sql });
                ret = conn.Query<AuctionDomainModelWebApi>(Sql, mc).ToList();
                NumOfRecord = conn.Query<int>(SqlForNumOfRec, mc).FirstOrDefault();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            AuctionDomainModelWebApiPaged retPaged = new AuctionDomainModelWebApiPaged();
            retPaged.DomainList = ret;
            retPaged.NumOfRecord = NumOfRecord;
            return retPaged;
        }

        public DropDomainModelWebApiPaged GetDropingDomainsByKeywordsPaged(string IncludeHyphens, string IncludeNumbers, List<KeywordModel> Keywords, List<string> TLDs, char DropOrLastDrop,char SortBy, string SearchStr, int NumOfRecPerPage, int WhichPage,string dropDate, bool IsExcel) //, SqlParamModel SqlParams)
        {
            //paging, text search, order by kısımlarını clientte yapacağız-bunun için full data alınacak, IsChanged parametresi değişmediği sürece servere tekrar gelinmeyecek
            int NumOfRecord = 0;
            string _table = (DropOrLastDrop == 'D') ? "DomainsDroping" : "DomainsDropedLastWeek";
            List<DropDomainModelWebApi> ret = new List<DropDomainModelWebApi>();
            List<DropDatesType> DropDates = new List<DropDatesType>();
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlKeywordsPart = GetSqlPartForKeywords(Keywords);
            string Sql = "select * from " + _table + " with (nolock) ";
            Sql += "where ";
            Sql += SqlTLDsPart.Substring(4);    //baştaki and'i atmak için
            if (IncludeHyphens == "0") Sql += " and HasHyphens = 0";
            if (IncludeNumbers == "0") Sql += " and HasNumbers = 0";
            Sql += SqlKeywordsPart;
            GetSqlForSearchSortAndPaging(ref Sql, SortBy, SearchStr, dropDate, NumOfRecPerPage, WhichPage,IsExcel);
            string SqlForNumOfRec = Sql.Substring(0, Sql.IndexOf("order by") - 1).Replace("select *", "select count(Id)");
            string SqlLog = "insert into SqlLog(Sql,FromWhere) values (@Sql,'GetDropingDomainsByKeywordsPaged')";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                conn.Execute(SqlLog, new { @Sql = Sql });
                ret = conn.Query<DropDomainModelWebApi>(Sql).ToList();
                NumOfRecord = conn.Query<int>(SqlForNumOfRec).FirstOrDefault();
                if (WhichPage == 1 && SortBy == '9' && SearchStr == "" && dropDate == "A")   //filtreli sorgularda drop date listesine ihtiyaç yok, zaten ilk objeyi oluştururken ilk sayfa sorgusunda almıştı
                    DropDates = conn.Query<DropDatesType>("DropAuctionOp.GetDropDates", commandType: CommandType.StoredProcedure).ToList();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            DropDomainModelWebApiPaged retPaged = new DropDomainModelWebApiPaged();
            retPaged.DomainList = ret;
            retPaged.NumOfRecord = NumOfRecord;
            retPaged.DropDates = DropDates.Where(x => TLDs.Contains(x.Ext) && x.DropOrLastDrop == DropOrLastDrop).Select(y => y.DropDate).Distinct().ToList();
            return retPaged;
        }

        public int GetTableRowCount(string tableName)
        {
            string sp = "count" + tableName;
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                try
                {
                    return conn.Query<int>(sp, commandType: CommandType.StoredProcedure).First();
                }
                catch (Exception ex) { return -2; }
            }
        }

        public AuctionDomainModelWebApiPaged GetAuctionDomainsByKeywordsPaged(string IncludeHyphens, string IncludeNumbers, List<KeywordModel> Keywords, List<string> TLDs, char AuctionType, char SortBy, string SearchStr, int NumOfRecPerPage, int WhichPage,bool IsExcel) //, SqlParamModel SqlParams)
        {
            int NumOfRecord = 0;
            List<AuctionDomainModelWebApi> ret = new List<AuctionDomainModelWebApi>();
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlKeywordsPart = GetSqlPartForKeywords(Keywords);
            string _table = (AuctionType == 'A') ? "DomainsAuctions" : "DomainsPreRelease";
            string Sql = "select * from " + _table + " with (nolock) ";
            Sql += "where ";
            Sql += SqlTLDsPart.Substring(4);    //baştaki and'i atmak için
            if (IncludeHyphens == "0") Sql += " and HasHyphens = 0";
            if (IncludeNumbers == "0") Sql += " and HasNumbers = 0";
            Sql += SqlKeywordsPart;
            GetSqlForSearchSortAndPaging(ref Sql, SortBy, SearchStr,"A", NumOfRecPerPage, WhichPage,IsExcel);
            string SqlForNumOfRec = Sql.Substring(0, Sql.IndexOf("order by")-1).Replace("select *", "select count(Id)");
            string SqlLog = "insert into SqlLog(Sql,FromWhere) values (@Sql,'GetAuctionDomainsByKeywordsPaged')";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                conn.Execute(SqlLog, new { @Sql = Sql });
                ret = conn.Query<AuctionDomainModelWebApi>(Sql).ToList();
                NumOfRecord = conn.Query<int>(SqlForNumOfRec).FirstOrDefault();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            AuctionDomainModelWebApiPaged retPaged = new AuctionDomainModelWebApiPaged();
            retPaged.DomainList = ret;
            retPaged.NumOfRecord = NumOfRecord;
            return retPaged;

        }

        public AuctionDomainModelWebApi IsDomainInAuction(string Domain, string Ext)
        {
            string Sql = "IsDomainInAuction";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                return conn.Query<AuctionDomainModelWebApi>(Sql, new { Domain = @Domain, Ext = @Ext }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
        }

        public DropAuctionDateModel GetDropDate(string Domain, string Ext)
        {
            string Sql = "GetDropDate";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<DropAuctionDateModel>(Sql, new { Domain = @Domain, Ext = @Ext }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }

        private string GetSqlPartForPatterns(MatchCriteriaWithPatternModel mc)
        {
            if (mc.Patterns == null) return "";
            string StartWith = "";
            string EndWith = "";
            string DoNotMatch = "";
            foreach (MatchPatternModel Pattern in mc.Patterns)
            {
                if (Pattern.IsConsistent)
                {
                    if (Pattern.PatternType == 0) StartWith += " or Pattern like '" + Pattern.Pattern + "%'";
                    if (Pattern.PatternType == 1) EndWith += " or Pattern like '%" + Pattern.Pattern + "'";
                    if (Pattern.PatternType == 2) DoNotMatch += " and not Pattern like '%" + Pattern.Pattern + "%'";
                }
            }
            if (StartWith.Length > 1) StartWith = " and (" + StartWith.Substring(3) + ")";
            if (EndWith.Length > 1) EndWith = " and (" + EndWith.Substring(3) + ")";
            if (DoNotMatch.Length > 1) DoNotMatch = " and (" + DoNotMatch.Substring(4) + ")";
            return StartWith + EndWith + DoNotMatch;
        }
        private string GetSqlPartForTLDs(List<string> TLDs)
        {
            string SqlPart = "";
            if (TLDs.Count == 1) { SqlPart = "and Ext = '" + TLDs[0].ToString() + "' "; return SqlPart; }
            foreach (string TLD in TLDs)
            {
                SqlPart += ",'" + TLD + "'";
            }
            SqlPart = "and Ext in (" + SqlPart.Substring(1) + ") ";
            return SqlPart;
        }
        private string GetSqlPartForKeywords(List<KeywordModel> Keywords)
        {
            string SqlPart = "";
            string startW = ""; string endW = "";
            if (Keywords.Count == 1)
            {
                if (Keywords[0].KeywordType == 1) startW = "%";
                if (Keywords[0].KeywordType == 2) endW = "%";
                if (Keywords[0].KeywordType == 3) { startW = "%"; endW = "%"; }
                SqlPart = "and Domain like '" + endW + Keywords[0].Keyword.ToString() + startW + "'";
                return SqlPart;
            }
            foreach (KeywordModel Keyword in Keywords)
            {
                startW = ""; endW = "";
                if (Keyword.KeywordType == 1) startW = "%";
                if (Keyword.KeywordType == 2) endW = "%";
                if (Keyword.KeywordType == 3) { startW = "%"; endW = "%"; }
                SqlPart += " or Domain like '" + endW + Keyword.Keyword + startW + "'";
            }
            SqlPart = " and (" + SqlPart.Substring(4) + ") ";
            return SqlPart;
        }

        public string GetSqlPartForChar(MatchCriteriaWithPatternModel mc)
        {
            string SqlPart = "";
            if (mc.Consonants.Trim() == "") SqlPart += " and HasConsonants=0";
            else
            {
                var Cons = mc.Consonants.ToCharArray();
                List<char> ExludedCons = lstConsonants.Except(Cons).ToList();
                foreach (char item in ExludedCons)
                {
                    SqlPart = SqlPart + " and " + item.ToString() + "=0";
                }
            }
            if (mc.Vowels.Trim() == "") SqlPart += " and HasVowels=0";
            else
            {
                var Vowels = mc.Vowels.ToCharArray();
                List<char> ExludedVowels = lstVowels.Except(Vowels).ToList();
                foreach (char item in ExludedVowels)
                {
                    SqlPart += " and " + item.ToString() + "=0";
                }
            }
            //numbers
            if (mc.Numbers.Trim() == "") SqlPart += " and HasNumbers=0";
            else
            {
                var Numbers = mc.Numbers.ToCharArray();
                List<char> ExludedNumbers = lstNumbers.Except(Numbers).ToList();
                foreach (char item in ExludedNumbers)
                {
                    SqlPart += " and n" + item.ToString() + "=0";
                }
            }
            //***
            return SqlPart;

        }

        private void GetSqlForSearchSortAndPaging(ref string Sql, char SortBy, string SearchStr,string dropDate, int NumOfRecPerPage, int WhichPage,bool IsExcel)
        {
            //SortBy: L:Length, A:AlphabeticAsc, D:AlphabeticDesc,B:BuyNow Price, C:CurrentPrice
            if (SearchStr != null && SearchStr.Trim().Length > 0) Sql = Sql + " and Domain like '%" + SearchStr + "%'";
            if (dropDate != "A" && dropDate != "") Sql += " and CAST(DropDate as date)=Cast(CONVERT(DateTime, '"+ dropDate + "', 110) as Date)";
            bool OrderExist = (Sql.IndexOf("order by") > 0);
            string OrderPart = "";
            switch (SortBy)
            {
                case 'L':
                    OrderPart = OrderExist ? ",Length(Domain)" : " order by Len(Domain)";
                    break;
                case 'A':
                    OrderPart = OrderExist ? ",Domain" : " order by Domain";
                    break;
                case 'D':
                    OrderPart = OrderExist ? ",Domain desc" : " order by Domain desc";
                    break;
                case 'B':
                    OrderPart = OrderExist ? ",BuyNowPrice" : " order by BuyNowPrice";
                    break;
                case 'C':
                    OrderPart = OrderExist ? ",CurrentPrice" : " order by CurrentPrice";
                    break;
                default:
                    OrderPart = OrderExist ? "" : " order by Id";
                    break;
            }
            Sql += OrderPart;
            if (!IsExcel) Sql += " OFFSET " + ((WhichPage - 1) * NumOfRecPerPage).ToString() + " ROWS FETCH NEXT " + NumOfRecPerPage.ToString() + " ROWS ONLY";

        }

    }
    public class DropDatesType
    {
        public DateTime DropDate { get; set; }
        public string Ext { get; set; }
        public char DropOrLastDrop { get; set; }

    }
}