using AnnDnWebApi.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AnnDnWebApi.Classes
{
    public class Kuls
    {
        string myConn = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public KulModel GetKulByProductKeyAndCHashKey(string ProductKey,string cHashKey)
        {
            string sP = "Licance.GetKulByProductKeyAndCHashKey";
            if (ProductKey == "RRRR-RRRR") sP = "Licance.GetKulByProductKeyAndCHashKeyTrial";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<KulModel>(sP,new { @ProductKey= ProductKey, @cHashKey = cHashKey }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }

        public bool IsSubscribedForData(string ProductKey,string cHashKey)
        {
            //GetCHashTrial ise cHashKey ile Lisanslı ise Product key ile subscription kontrolü yapılacak. Bunun için @Key oluşturup duruma göre bunun üzerinden gönderiyorum
            int res = 0;
            string Key = ProductKey;
            string sP = "Licance.IsSubscribedForData";
            if (ProductKey == "RRRR-RRRR") { sP = "Licance.IsSubscribedForDataForTrial"; Key = cHashKey; }
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                res = conn.Query<int>(sP, new { @Key = Key }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            return (res == 0) ? false : true;
        }

        public RegistryObject CheckRegistryFromServer(string ProductKey,string CurrentCHash)
        {
            //non registered ise trial'a CHash'yu ekleyip trail object döner, daha önce trial ise tarihi kontrol edip trail ya da nonregistered döner, registered ise zaten registered object döner
            //client'de local regObj ile karşılaştırılıp update'ler yapılacaktır.
            KulCHashModel cHash = new KulCHashModel();
            List<KulCHashModel> cHashList = new List<KulCHashModel>();
            RegistryObject KulOnServer;// = new RegistryObject();

            //ek. cHash daha önceden register oldu ise ama rg o ProductKey ile gelmediyse de registered bilgisini dönmek için 
            if (ProductKey=="RRRR-RRRR")    //mevcut lisansı Trial iken update Licance'e bastıysa bu cHash'nun lisanslı tanımı var mı diye bakılır.
            {
                string tmpProductKey = GetProductKeyByCHash(CurrentCHash)??"";
                if (tmpProductKey != "") ProductKey = tmpProductKey;
                
            }

            //
            KulOnServer = GetKulByProductKey(ProductKey);
            
            if (KulOnServer==null)      //ProductKey kullanıcılar tablosunda kayıtlı değilse
            {
                KulOnServer= new RegistryObject();
                SetNonRegisteredOrTrial(ref KulOnServer, CurrentCHash);
            }
            else
            {
                if (KulOnServer.IsRegistrated == 2)  //TRIAL
                {
                    //CHash = GetCHashTrial(CurrentCHash);
                    //if (CHash.AddedAt.AddMonths(1) < DateTime.Now) KulOnServer.IsRegistrated = 0; //trial için cHash yoksa sp tarafında yaratılıyor...
                    SetNonRegisteredOrTrial(ref KulOnServer, CurrentCHash);
                }
                
                if (KulOnServer.IsRegistrated == 1)    //Product key is Registered-CHash kontrolleri yapılacak
                {
                    cHashList = GetCHashByProductKey(ProductKey);
                    if (cHashList.Where(x=>x.cHashKey == CurrentCHash).FirstOrDefault()==null)  //clientin gönderdiği CHash bu product key için listede yok-not registered  
                                                                                                //gelen cHash registered olmadıysa=>product key'e bağlı cHash 5'den küçükse cHash ekle ve ret as registered değilse return not registered
                    {
                        if (cHashList.Count<5)  //5 CHash'ya kadar bu productKey ile kayıt yapılabilir. Bilgisayar değişikliği vs. gibi durumlar için.
                        {
                            AddThisCHash(CurrentCHash,ProductKey);
                            KulOnServer.cHashKey = CurrentCHash;
                        }
                        else  //
                        {
                            SetNonRegisteredOrTrial(ref KulOnServer, CurrentCHash);
                        }
                    }
                }
            }   //else
            KulOnServer.cHashKey = CurrentCHash;
            return KulOnServer;
        }
        private void SetNonRegisteredOrTrial(ref RegistryObject KulOnServer,string CurrentCHash)
        {
            //ProductKey = "RRRR-RRRR" ve IsRegistrated = 0 ise trial versiyon expire olmuş demektir. 
            KulCHashModel kc = new KulCHashModel();
            kc=GetCHashTrial(CurrentCHash); //trial'de de yoksa cHash'yu ekleyip trial data dönecek
            KulOnServer.IsRegistrated = 2;
            if (kc.AddedAt.AddMonths(1) < DateTime.Now) KulOnServer.IsRegistrated = 0; //
            KulOnServer.DataRegistrationDurationInMonth = 1;
            //KulOnServer.HaveDropAuth = 1;
            //KulOnServer.HaveTrends = 1;
            KulOnServer.ProductKey = "RRRR-RRRR";
            KulOnServer.RegistratedDate = kc.AddedAt;//yeni eklediyse şimdiki zaman Trial expire olduysa olmuştur, olmadıysa da olmamış tarihi döner
            KulOnServer.DataRegistratedDate = kc.AddedAt;
            
        }
        private void AddThisCHash(string CurrentCHash, string ProductKey)
        {
            string sP = "Licance.AddCHash";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                conn.Execute(sP, new { @cHashKey = CurrentCHash, @ProductKey =ProductKey}, commandType: CommandType.StoredProcedure);
            }
        }
        private KulCHashModel GetCHashTrial(string cHashKey)
        {
            string sP = "Licance.GetCHashTrialByCHashKey";  //cHash yoksa trial olarak ekleyip onu dönüyor.
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<KulCHashModel>(sP, new { @cHashKey = cHashKey }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }

        private List<KulCHashModel> GetCHashByProductKey(string ProductKey)
        {
            string sP = "Licance.GetCHashList5";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<KulCHashModel>(sP, new { @ProductKey = ProductKey }, commandType: CommandType.StoredProcedure).ToList();
            }
        }
        private RegistryObject GetKulByProductKey(string ProductKey)
        {
            string sP = "Licance.GetKulByProductKey";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<RegistryObject>(sP, new { @ProductKey = ProductKey }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }
        private string GetProductKeyByCHash(string cHashKey)
        {
            string sP = "Licance.GetProductKeyByCHash";
            using (IDbConnection conn = new SqlConnection(myConn))
            {
                conn.Open();
                return conn.Query<string>(sP, new { @cHashKey = cHashKey }, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }



    }
}