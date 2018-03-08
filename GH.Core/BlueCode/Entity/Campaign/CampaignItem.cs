using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Campaign
{
    public class Age
    {
        public Age() {
            max = "85";
            min = "13";

        }
        public string min { get; set; }
        public string max { get; set; }
    }

    public class Location
    {
        public Location()
        {
            country = "";
            region = "";
            city = "";

        }
        public string country { get; set; }
        public string region { get; set; }
        public string city { get; set; }
    }

    public class Spend
    {
        public Spend() {

            type = "Daily";
            money = new Random().Next(1, 10).ToString();
            currentcy = "USD";
            effectiveDate = "2016-10-11";
            endDate = "2016-10-11";
        }
        public string type { get; set; }
        public string money { get; set; }
        public string currentcy { get; set; }
        public string effectiveDate { get; set; }
        public string endDate { get; set; }
      
    }


    public class Criteria
    {
        public Criteria() {
            age = new Age();
            location = new Location();
            gender = "All";
            locationtype = "Global";
            spend = new Spend();
            keywords = new List<object>();
            estimatedReach = new Random().Next(1, 10).ToString();
            targetNetwork = "";
        }
        public Age age { get; set; }
        public string gender { get; set; }
        public Location location { get; set; }
        public string locationtype { set; get; }
        public string estimatedReach { set; get; }
        
        public string targetNetwork { get; set; }
        public Spend spend { get; set; }
        public string residenceStatus { get; set; }
        public List<object> keywords { get; set; }
    }

    public class QrCode
    {
        public QrCode()
        {
            content = "";
            image = "";
            PublicURL = "";
            AllowCreateQrCode = false;
        }


        public string PublicURL { get; set; }
        public string content { get; set; }
        public string image { get; set; }
        public bool AllowCreateQrCode { get; set; }
        
    }

    public class Event
    {
        public Event()
        {
          starttime = "1969-12-31T17:27:00.000Z";
          startdate = "2016-10-11";
          endtime = "1970-01-01T05:27:00.000Z";
          enddate = "2016-10-11";
          location = ExampleDataHelper.GetRandomLocation();
          theme = ExampleDataHelper.GetRandomListKeywords("colour", 1)[0];
        }
        public string starttime { get; set; }
        public string startdate { get; set; }
        public string endtime { get; set; }
        public string enddate { get; set; }
        public string location { get; set; }
        public string theme { get; set; }
    }

    public class CampaignEvent
    {
        public CampaignEvent()
        {
            @event = new Event();

            criteria = new Criteria();
            qrCode = new QrCode();
            fields = new List<FieldinformationVault>();
            followerIds = new List<object>();
            image = ExampleDataHelper.GetRandomImageForCampaign();
            participants = "";
            boostAdvertising = "";
            currentViewers = "";
            targetLink = "";
            usercodetype = "Free";
            usercodecurrentcy = "USD";
            usercode = "";
           
        }

       
        public string type { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string registrationType { get; set; }
        public Event @event { get; set; }
        public Criteria criteria { get; set; }
        public string usercodetype { get; set; }
        public string usercode { get; set; }
        public string usercodecurrentcy { get; set; }
        public string flashAdvertising { get; set; }
        public string boostAdvertising { get; set; }
        public string days { get; set; }
        public string addToBusinessPage { get; set; }
        public string image { get; set; }
        public QrCode qrCode { get; set; }
        public string targetLink { get; set; }
        public string termsAndConditionsFile { get; set; }
        public string currentViewers { get; set; }
        public string expectedViewers { get; set; }
        public string participants { get; set; }
        public string commentsFromSupervisor { get; set; }
        public string commentsEvent { get; set; }
        public string commentsCriteria { get; set; }
        public string commentsBudgetTime { get; set; }
        public string commentsMedia { get; set; }
        public List<FieldinformationVault> fields { get; set; }
        public List<object> followerIds { get; set; }

    }


    public class CampaignAdvertising
    {
        public CampaignAdvertising() {
            criteria = new Criteria();
            qrCode = new QrCode();
            image = ExampleDataHelper.GetRandomImageForCampaign();
            participants = "";
            boostAdvertising = "";
            currentViewers = "";
            targetLink = "";
            
        }
        public string type { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Criteria criteria { get; set; }
        public string flashAdvertising { get; set; }
        public string boostAdvertising { get; set; }
        public string days { get; set; }
        public string addToBusinessPage { get; set; }
        public string image { get; set; }
        public QrCode qrCode { get; set; }
        public string targetLink { get; set; }
        public string currentViewers { get; set; }
        public string expectedViewers { get; set; }
        public string participants { get; set; }
        public string commentsFromSupervisor { get; set; }
        public string commentsCriteria { get; set; }
        public string commentsBudgetTime { get; set; }
        public string commentsMedia { get; set; }
    }
    
    public class CampaignRegistration
    {
        public CampaignRegistration() {
           
            criteria = new Criteria();
            qrCode = new QrCode();
            fields = new List<FieldinformationVault>();
            followerIds = new List<object>();
            image = ExampleDataHelper.GetRandomImageForCampaign();
            registrationType = "Event-Based";
            participants = "";
            boostAdvertising = "";
            currentViewers = "";
            targetLink = "";
            usercodetype = "Free";
            usercodecurrentcy = "USD";
            usercode = "";
        }
        public string type { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string registrationType { get; set; }
        public Criteria criteria { get; set; }
        public string usercodetype { get; set; }
        public string usercode { get; set; }
        public string usercodecurrentcy { get; set; }
        public string flashAdvertising { get; set; }
        public string boostAdvertising { get; set; }
        public string days { get; set; }
        public string addToBusinessPage { get; set; }
        public string image { get; set; }
        public QrCode qrCode { get; set; }
        public string targetLink { get; set; }
        public string termsAndConditionsFile { get; set; }
        public string currentViewers { get; set; }
       

        public string expectedViewers { get; set; }
        public string participants { get; set; }
        public string commentsFromSupervisor { get; set; }
        public string commentsCriteria { get; set; }
        public string commentsBudgetTime { get; set; }
        public string commentsMedia { get; set; }
        public List<FieldinformationVault> fields { get; set; }
        public List<object> followerIds { get; set; }
    }
}