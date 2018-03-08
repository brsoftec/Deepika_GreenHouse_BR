using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;

namespace GH.Core.Models
{
    public class CoreProfile
    {
        public string id { get; set; }
        public string accountId { get; set; }
        public string displayName { get; set; }
        public string avatar { get; set; }
    }

    public class ProfileLocation
    {
        public string country { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string zipCode { get; set; }
    }

    public class MainProfile
    {
        public string id { get; set; }
        public string accountId { get; set; }
        public string displayName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string avatar { get; set; }
        public string description { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public ProfileLocation location { get; set; }

        public MainProfile(Account account)
        {
            id = account.Id.ToString();
            accountId = account.AccountId;
            displayName = account.Profile.DisplayName;
            avatar = account.Profile.PhotoUrl;
            description = account.Profile.Status;
            firstName = account.Profile.FirstName;
            lastName = account.Profile.LastName;
            dob = account.Profile.Birthdate.ToString();
            gender = account.Profile.Gender;
            location = new ProfileLocation()
            {
                country = account.Profile.Country,
                city = account.Profile.City,
                street = account.Profile.Street,
                zipCode = account.Profile.ZipPostalCode
            };
        }
    }

    public class MainBusinessProfile
    {
        public string id { get; set; }
        public string accountId { get; set; }
        public string displayName { get; set; }
        public string avatar { get; set; }
        public string publicPhone { get; set; }
        public string publicEmail { get; set; }
        public string description { get; set; }
        public string industry { get; set; }
        public string website { get; set; }
        public string workTimeString { get; set; }
        public object workTime { get; set; }
        public List<string> pictureAlbum { get; set; }
        public ProfileLocation location { get; set; }

        public MainBusinessProfile(Account account)
        {
            if (account == null) return;
            id = account.Id.ToString();
            accountId = account.AccountId;
            displayName = account.Profile.DisplayName;
            avatar = account.Profile.PhotoUrl;
            description = account.Profile.Description;
            publicPhone = account.CompanyDetails.Phone;
            publicEmail = account.CompanyDetails.Email;
            website = account.CompanyDetails.Website;
            industry = account.CompanyDetails.Industry;

            pictureAlbum = account.PictureAlbum;
            location = new ProfileLocation()
            {
                country = account.Profile.Country,
                city = account.Profile.City,
                street = account.Profile.Street,
                zipCode = account.Profile.ZipPostalCode
            };
            if (account.CompanyDetails.WorkTime != null)
            {
                workTime = BsonSerializer.Deserialize<object>(account.CompanyDetails.WorkTime);
                var wt = JObject.FromObject(workTime);

                var open247 = wt.SelectToken("open247");
                if (open247 == null || (bool) open247 == false)
                {
                    var ranges = wt.SelectToken("ranges")?.Children();
                    if (ranges != null)
                    {
                        var sb = new StringBuilder(string.Empty);
                        bool isFirst = ranges?.Count() > 1;
                        foreach (var range in ranges)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                sb.Append("\n");
                            }
                            DateTime from; DateTime to;
                            DateTime.TryParse((string)range.SelectToken("from"), out from);
                            sb.Append(from.ToString("hh:mmtt"));                       
                            DateTime.TryParse((string)range.SelectToken("to"), out from);
                            sb.Append(" - " + from.ToString("hh:mmtt"));
                            
                            sb.Append(" :");
                            var weekdays = range.SelectToken("weekdays")?.Children();

                            if (weekdays != null)
                            {
                                foreach (var weekday in weekdays)
                                {
                                    if ((bool) weekday.SelectToken("open") == true)
                                        sb.Append(" " + weekday.SelectToken("name"));
                                }
                            }
                        }


                        workTimeString = sb.ToString();
                    }
                }
                else
                {
                    workTimeString = "Open 24/7";
                }

            }
        }
    }

    public class WorkTimeDay
    {
        public DateTime from { get; set; }
    }

    public class WorkTime
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string accountId { get; set; }
        public string displayName { get; set; }
        public string avatar { get; set; }
    }
}