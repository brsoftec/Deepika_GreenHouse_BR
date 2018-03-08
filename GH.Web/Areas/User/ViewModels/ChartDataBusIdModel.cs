using GH.Core.BlueCode.Entity.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ChartDataBusIdModel : TransactionalInformation
    {
        public string Startdate { set; get; }

        public string BusId { set; get; }

        public List<DataRegisCampaign> Data { set; get; }
    }
}