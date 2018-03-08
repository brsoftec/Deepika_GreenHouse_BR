using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GH.Core.ViewModels
{
    public class TwitterUpdateStatusModel
    {
        [Required, MaxLength(140)]
        public string Status { get; set; }
        public string InReplyToStatusId { get; set; }
        public bool? PossiblySensitive { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
        public string PlaceId { get; set; }
        public bool? DisplayCoordinates { get; set; }
        public bool? TrimUser { get; set; }
        public string MediaIds { get; set; }

        public IDictionary<string, string> GetDictionary()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            dictionary.Add("status", Status);

            if (!string.IsNullOrEmpty(InReplyToStatusId))
            {
                dictionary.Add("in_reply_to_status_id", InReplyToStatusId);
            }

            if (PossiblySensitive.HasValue)
            {
                dictionary.Add("possibly_sensitive", PossiblySensitive.Value.ToString().ToLower());
            }

            if (!string.IsNullOrEmpty(Lat) && !string.IsNullOrEmpty(Long))
            {
                dictionary.Add("lat", Lat);
                dictionary.Add("long", Long);
            }

            if (!string.IsNullOrEmpty(PlaceId))
            {
                dictionary.Add("place_id", PlaceId);
            }

            if (DisplayCoordinates.HasValue)
            {
                dictionary.Add("display_coordinates", DisplayCoordinates.Value.ToString().ToLower());
            }

            if (TrimUser.HasValue)
            {
                dictionary.Add("trim_user", TrimUser.Value.ToString().ToLower());
            }

            if (!string.IsNullOrEmpty(MediaIds))
            {
                dictionary.Add("media_ids", MediaIds);
            }            

            return dictionary;
        }
    }
}