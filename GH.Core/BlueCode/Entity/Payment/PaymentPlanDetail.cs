using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Payment
{
    public class PaymentPlanDetail
    {
       public string PlanName { set; get; }
       public decimal Price  { set; get; }
       public string InteractionActives { set; get; }
       public string SyncRelationships { set; get; }
       public string BusinessUsers { set; get; }
       
    }
}