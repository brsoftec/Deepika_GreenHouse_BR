using GH.Core.BlueCode.Entity.Outsite;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Adapters
{
    public class OutsiteAdapter
    {
        public static OutsiteViewModel OutsiteToOutsiteViewModel(Outsite outsite)
        {
            var rs = new OutsiteViewModel();
            rs.Id = outsite.Id.ToString();
            rs.Email = outsite.Email;
            rs.Option = outsite.Option;
            rs.SendMe = outsite.SendMe;
            rs.FromUserId = outsite.FromUserId;
            rs.ListEmail = outsite.ListEmail;
            rs.FromDisplayName = outsite.FromDisplayName;
            rs.CompnentId = outsite.CompnentId;
            rs.Type = outsite.Type;
            rs.Description = outsite.Description;
            rs.DateCreate = outsite.DateCreate;
            rs.Url = outsite.Url;
            rs.Status = outsite.Status;

            return rs;
        }
        public static Outsite OutsiteViewModelToOutsite(OutsiteViewModel outsite)
        {
            var rs = new Outsite();
            if(string.IsNullOrEmpty(outsite.Id))
                rs.Id = ObjectId.GenerateNewId();
            else


            rs.Id = new ObjectId(outsite.Id);
            rs.Email = outsite.Email;
            rs.Option = outsite.Option;
            rs.SendMe = outsite.SendMe;
            rs.FromUserId = outsite.FromUserId;
            rs.ListEmail = outsite.ListEmail;
            rs.FromDisplayName = outsite.FromDisplayName;
            rs.CompnentId = outsite.CompnentId;
            rs.Type = outsite.Type;
            rs.Description = outsite.Description;
            rs.DateCreate = outsite.DateCreate;
            rs.Url = outsite.Url;
            rs.Status = outsite.Status;

            return rs;
        }
    }
}