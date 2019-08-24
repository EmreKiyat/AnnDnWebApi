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
    public class DropDomain
    {
        readonly List<char> lstConsonants = new List<char>(new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' });
        readonly List<char> lstVowels = new List<char>(new char[] { 'a', 'e', 'i', 'o', 'u' });
        readonly List<char> lstNumbers = new List<char>(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

        string myConn =ConfigurationManager.ConnectionStrings["conn"].ConnectionString ;

        //public List<DropDomainModel> GetFilteredDropingDomainsWithKulId(int KulId)
        //{

        //}

        public List<DropDomainModelWebApi> GetFilteredDropingDomains(MatchCriteriaWithPatternModel mc, List<string> TLDs,char DropOrLastDrop)     //,SqlParamModel SqlParams)
        {
            List<DropDomainModelWebApi> ret = new List<DropDomainModelWebApi>();
            string SqlPartChars = GetSqlPartForChar(mc);
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlPatternsPart = GetSqlPartForPatterns(mc);
            string _table = (DropOrLastDrop=='D')? "DomainsDroping" : "DomainsDropedLastWeek";
            
            string Sql = "select * from "+_table +" with (nolock) ";
            Sql += "where uzunluk between @MinLength and @MaxLength ";
            Sql += SqlTLDsPart;
            Sql += SqlPartChars;
            if (mc.HasSlash == 0) Sql += " and HasHyphens = 0";  //HasHyphens=1 durumunda sql'e bişi ekleme eklersen sadece slash'lı olanları getirir.
            Sql += SqlPatternsPart;
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                //string SqlLog = "insert into SqlLog(Sql,SqlPatternsPart) values (@Sql,@SqlPatternsPart)";
                //conn.Execute(SqlLog, new { @Sql = Sql, @SqlPatternsPart = SqlPatternsPart });
                ret = conn.Query<DropDomainModelWebApi>(Sql, mc).ToList();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            //OrganizeDate(ref ret);
            return ret;
        }

        public List<AuctionDomainModelWebApi> GetFilteredAuctionDomains(MatchCriteriaWithPatternModel mc, List<string> TLDs,char AuctionType) 
        {
            List<AuctionDomainModelWebApi> ret = new List<AuctionDomainModelWebApi>();
            string SqlPartChars = GetSqlPartForChar(mc);
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlPatternsPart = GetSqlPartForPatterns(mc);
            string _table = (AuctionType == 'A') ? "DomainsAuctions" : "DomainsPreRelease";
            string Sql = "select * from "+_table+" with (nolock) ";
            Sql += "where uzunluk between @MinLength and @MaxLength ";
            Sql += SqlTLDsPart;
            Sql += SqlPartChars;
            if (mc.HasSlash == 0) Sql += " and HasHyphens = 0";  
            Sql += SqlPatternsPart;
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                ret = conn.Query<AuctionDomainModelWebApi>(Sql, mc).ToList();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            //OrganizeDate(ref ret);
            return ret;
        }

        public List<DropDomainModelWebApi> GetDropingDomainsByKeywords(string IncludeHyphens,string IncludeNumbers, List<KeywordModel> Keywords, List<string> TLDs,char DropOrLastDrop) //, SqlParamModel SqlParams)
        {
            //paging, text search, order by kısımlarını clientte yapacağız-bunun için full data alınacak, IsChanged parametresi değişmediği sürece servere tekrar gelinmeyecek
            string _table = (DropOrLastDrop == 'D') ? "DomainsDroping" : "DomainsDropedLastWeek";
            List<DropDomainModelWebApi> ret = new List<DropDomainModelWebApi>();
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlKeywordsPart = GetSqlPartForKeywords(Keywords);
            string Sql = "select * from "+ _table + " with (nolock) ";
            Sql += "where ";
            Sql += SqlTLDsPart.Substring(4);    //baştaki and'i atmak için
            if (IncludeHyphens == "0") Sql += " and HasHyphens = 0";
            if (IncludeNumbers == "0") Sql += " and HasNumbers = 0";
            Sql += SqlKeywordsPart;

                using (IDbConnection conn = new SqlConnection(myConn))
                {
                    conn.Open();
                //try
                //{
                //conn.Execute(SqlLog, new { @Sql = Sql });
                    ret= conn.Query<DropDomainModelWebApi>(Sql).ToList();
                    //    }
                    //    catch (Exception ex) { int a = 1; }
                }
            //OrganizeDate(ref ret);
            return ret;
        }

        public int GetTableRowCount(string tableName)
        {
            string sp = "count"+tableName;
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

        public List<AuctionDomainModelWebApi> GetAuctionDomainsByKeywords(string IncludeHyphens, string IncludeNumbers, List<KeywordModel> Keywords, List<string> TLDs,char AuctionType) //, SqlParamModel SqlParams)
        {
            //paging, text search, order by kısımlarını clientte yapacağız-bunun için full data alınacak, IsChanged parametresi değişmediği sürece servere tekrar gelinmeyecek
            List<AuctionDomainModelWebApi> ret = new List<AuctionDomainModelWebApi>();
            string SqlTLDsPart = GetSqlPartForTLDs(TLDs);
            string SqlKeywordsPart = GetSqlPartForKeywords(Keywords);
            string _table = (AuctionType == 'A') ? "DomainsAuctions" : "DomainsPreRelease";
            string Sql = "select * from "+ _table +" with (nolock) ";
            Sql += "where ";
            Sql += SqlTLDsPart.Substring(4);    //baştaki and'i atmak için
            if (IncludeHyphens == "0") Sql += " and HasHyphens = 0";
            if (IncludeNumbers == "0") Sql += " and HasNumbers = 0";
            Sql += SqlKeywordsPart;

            //string SqlLog = "insert into SqlLog(Sql) values (@Sql)";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                //conn.Execute(SqlLog, new { @Sql = Sql });
                ret = conn.Query<AuctionDomainModelWebApi>(Sql).ToList();
                //    }
                //    catch (Exception ex) { int a = 1; }
            }
            //OrganizeDate(ref ret);
            return ret;
        }

        public AuctionDomainModelWebApi IsDomainInAuction(string Domain, string Ext)
        {
            string Sql = "IsDomainInAuction";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                //try
                //{
                return conn.Query<AuctionDomainModelWebApi>(Sql, new {Domain=@Domain,Ext=@Ext}, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
            return StartWith + EndWith + DoNotMatch ;
        }
        private string GetSqlPartForTLDs(List<string> TLDs)
        {
            string SqlPart = "";
            if (TLDs.Count == 1) { SqlPart = "and Ext = '"+TLDs[0].ToString()+"' "; return SqlPart; }
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
            string startW = ""; string endW =  ""; 
            if (Keywords.Count == 1) {
                if (Keywords[0].KeywordType == 1) startW = "%";
                if (Keywords[0].KeywordType == 2) endW = "%";
                if (Keywords[0].KeywordType == 3) { startW = "%"; endW = "%"; }
                SqlPart = "and Domain like '"+endW+ Keywords[0].Keyword.ToString()+startW+"'";
                return SqlPart;
            }
            foreach (KeywordModel Keyword in Keywords)
            {
                startW = "";endW = "";
                if (Keyword.KeywordType == 1) startW = "%";
                if (Keyword.KeywordType == 2) endW = "%";
                if (Keyword.KeywordType == 3) { startW = "%"; endW = "%"; }
                SqlPart += " or Domain like '" + endW + Keyword.Keyword + startW +"'";
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
    }
}