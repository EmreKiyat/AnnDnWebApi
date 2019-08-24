using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using AnnDnWebApi.Models;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using static AnnDnWebApi.Models.SenkronizationModel;
using System.Web.Http;
using System.Web.Http.Description;

namespace AnnDnWebApi.Classes
{
    public class Senkronization:ApiController
    {
        string myConn = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public List<ChangedModel> GetChanged()
        {
            string sp = "synch.GetChanged";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                try
                {
                    return conn.Query<ChangedModel>(sp, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { return new List<ChangedModel> { new ChangedModel { ChangedTable = "x", LastChangedId = -1 } }; }
            }
        }

        public List<WhoIsServersModel> GetLastChangedWhoIsServerList()
        {
            string sp = "synch.GetWhoIsServerList";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<WhoIsServersModel>(sp,CommandType.StoredProcedure).ToList();
            }
        }

        public List<zSourceModel> GetZSourceList()
        {
            string sp = "synch.GetZSourceList";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<zSourceModel>(sp, CommandType.StoredProcedure).ToList();
            }
        }

        //public List<sMobilControlModel> GetSMobilControl()
        //{
        //    string sp = "synch.GetSMobilControl";
        //    using (IDbConnection conn = new SqlConnection(myConn))
        //    {
        //        conn.Open();
        //        return conn.Query<sMobilControlModel>(sp, CommandType.StoredProcedure).ToList();
        //    }
        //}
        public List<zEMailListForPrivateRegistrationModel> GetZEMailListForPrivateRegistration()
        {
            string sp = "synch.GetZEMailListForPrivateRegistration";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<zEMailListForPrivateRegistrationModel>(sp, CommandType.StoredProcedure).ToList();
            }
        }
        public List<zRegistrarCompanyModel> GetZRegistrarCompany()
        {
            string sp = "synch.GetZRegistrarCompany";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<zRegistrarCompanyModel>(sp, CommandType.StoredProcedure).ToList();
            }
        }
        public List<zAuctionCompanyModel> GetZAuctionCompany()
        {
            string sp = "synch.GetZAuctionCompany";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<zAuctionCompanyModel>(sp, CommandType.StoredProcedure).ToList();
            }
        }
        public List<zTrendTaskRssTemplate> GetZTrendTaskRssTemplate()
        {
            string sp = "synch.GetZTrendTaskRssTemplate";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<zTrendTaskRssTemplate>(sp, CommandType.StoredProcedure).ToList();
            }
        }

        public string GetWebApiServerBaseAddr()
        {
            string sp = "synch.GetWebApiServerBaseAddr";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<string>(sp, CommandType.StoredProcedure).FirstOrDefault();
            }
        }
                  

        public void AddUsersMostUsedIANAId(int IanaId)
        {
            string sp = "synch.AddUsersMostUsedIANAId";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                conn.Execute(sp, new { @IanaId=IanaId }, commandType: CommandType.StoredProcedure);
                //return Ok();
            }
        }


    }
}