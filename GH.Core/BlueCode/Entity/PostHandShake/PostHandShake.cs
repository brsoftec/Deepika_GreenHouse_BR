using GH.Core.BlueCode.Entity.InformationVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.PostHandShake
{
    public class PostHandShake
    {
        public PostHandShake()
        {
            Fields = new List<FieldinformationVault>();
        }
       public string Id { set; get; }
       public bool IsJoin { set; get; }

       public bool isaccecpt { set; get; }

        public bool IsChange { set; get; }

        public string UserName { set; get; }

       public string UserId { set; get; }

       public string BusName { set; get; }

       public string BusId { set; get; }

       public string Jsondata { set; get; }

       public string JsondataOld { set; get; }

        public string CampaignId { set; get; }
        public string Comment { set; get; }

        public string Status { set; get; }

        public string DateUpdateJson { set; get; }
        public string DateTerminate { set; get; }
        public List<FieldinformationVault> Fields { get; set; }

    }
}