using System;

namespace AnnDnWebApi.Models
{
    public class KulModel
    {
        public string ProductKey { get; set; }  //Primary key
        public int IsMachine { get; set; }
        public int IsRegistrated { get; set; }
        public string Role { get; set; }
        public DateTime RegistratedDate { get; set; }
        public DateTime DataRegistratedDate { get; set; }
        public int DataRegistrationDurationInMonth { get; set; }
    }
    public class KulCHashModel
    {
        public int KulCHashKeyId { get; set; }
        public string ProductKey { get; set; }
        public string cHashKey { get; set; }
        public DateTime AddedAt { get; set; }
    }


    public class AuthenticationModel
    {
        //public int KulId { get; set; }
        public string ProductKey { get; set; }
        public string cHashKey { get; set; }
    }

    public class RegistryObject : AuthenticationModel
    {
        public int IsRegistrated { get; set; }  //1:Registered,2:Trial,0:not registered
        public DateTime RegistratedDate { get; set; }
        public DateTime DataRegistratedDate { get; set; }
        public int DataRegistrationDurationInMonth { get; set; }
    }

}