using GH.Core.BlueCode.Entity.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class ChartDataCampaignModel: TransactionalInformation
    {
        public string Startdate { set; get; }

        public string CamapignId { set; get; }

        public List<DataRegisCampaign> Data { set; get; }
    }
}