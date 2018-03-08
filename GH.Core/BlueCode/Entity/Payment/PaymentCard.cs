using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Payment
{
    public class PaymentCard
    {
       public string Id { set; get; }
       public string userId { set; get; }

       public string cardtype { set; get; }

       public string cardname { set; get; }

        public string cardnumber { set; get; }

        public string expiredmonth { set; get; }

       public string expiredyear { set; get; }

       public bool isdefault { set; get; }

       public string cardsecuritycode { set; get; }

       public string Jsondata { set; get; }

       public string idcreditcard { set; get; }

    }
}