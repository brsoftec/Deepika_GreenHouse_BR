using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using GH.Core.Models;
using GH.Core.Services;
using GH.Web.Areas.User.ViewModels;
using Microsoft.AspNet.Identity;
using NLog;

namespace GH.Web.Areas.User.Controllers
{


    public class CoreController : Controller
    {
        public string UserId;
        public string AccountId;
        public Account Account;

        static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public CoreController()
        {
        }
    }
}