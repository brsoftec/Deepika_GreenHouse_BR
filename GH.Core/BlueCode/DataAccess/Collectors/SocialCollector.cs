using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GH.Core.Extensions;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Services;

namespace GH.Core.Collectors
{
    public class SocialCollector : ISocialCollector
    {
        private IFacebookCollector _facebokService;
        private ITwitterCollector _twitterService;

        public SocialCollector(GreenHouseDbContext context)
        {
            _facebokService = new FacebookCollector();
            _twitterService = new TwitterCollector();
        }


    }
}