using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;
using GH.Core.BlueCode.Entity.Campaign;

namespace GH.Web.Areas.User.ViewModels
{
    public class BusinessProfileViewModel
    {
        public BusinessProfileViewModel()
        {

            ListCampaignWithoutSRFI = new List<NewFeedsViewModel>();

            ListCampaignSRFI = new List<CampaignItemForHomeFeed>();
        }
        public string Id { get; set; }
        public string BusId { get; set; }
        public string DisplayName { get; set; }
        public List<string> PictureAlbum { get; set; }
        public string Avatar { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public int NumberOfFollowers { get; set; }
        public bool Followed { get; set; }
        public bool CanFollow { get; set; }

        public AccountType AccountType { set; get; }

        public string CampaignSRFIId { set; get; }

        public BusinessPrivacy BusinessPrivacies { get; set; }

        public List<NewFeedsViewModel> ListCampaignWithoutSRFI{set;get;}

        public List<CampaignItemForHomeFeed> ListCampaignSRFI { set; get; }
        public List<NewFeedsViewModel> ListPushToVault { set; get; }
        public List<string> BusinessIdList { get; set; }
        public List<string> BusinessCampaignIdList { get; set; }
        public List<string> CampaignIdInPostList { get; set; }
    }


}