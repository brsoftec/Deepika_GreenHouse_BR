using GH.Core.BlueCode.Entity.InformationVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Post
{
    public class DataRegisCampaign
    {
        public string userid{ set; get; }
        public int age { set; get;}
        public string dob { set; get;}

        public string firstname { set; get; }

        public string email { set; get; }
        public string lastname { set; get; }

        public string country { set; get; }

        public string city { set; get; }

        public string datefollow { set; get; }

        public string gender { set; get; }


        public string dob_vault { set; get; }

        public string firstname_vault { set; get; }

        public string lastname_vault { set; get; }

        public string country_vault { set; get; }

        public string city_vault { set; get; }


        public string gender_vault { set; get; }

        public string keywords { set; get; }

        public List<FieldinformationVault> ListFieldsRegis {set; get;}
    }
}