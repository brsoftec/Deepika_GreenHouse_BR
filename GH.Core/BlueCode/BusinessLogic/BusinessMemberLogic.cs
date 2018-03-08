using GH.Core.BlueCode.DataAccess;
using GH.Core.BlueCode.Entity.Post;
using GH.Core.BlueCode.Entity.Profile;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using GH.Core.Models;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class BusinessMemberLogic : IBusinessMemberLogic
    {
        private readonly MongoRepository<BusinessMember> _businessMemberRepos = new MongoRepository<BusinessMember>();
        private readonly MongoRepository<Post> _postRepos = new MongoRepository<Post>();

        public bool AddBusinessMember(Account userAcount, Account businessUserAccount,string dateadd="")
        {

            var member = new Follower
            {
                UserId = userAcount.AccountId,
                Name = userAcount.Profile.DisplayName,
                Age = (userAcount.Profile.Birthdate.HasValue) ? (DateTime.Now.Year - userAcount.Profile.Birthdate.Value.Year) : 0,
                Gender = userAcount.Profile.Gender,
                CountryName = userAcount.Profile.Country,
                CityName = userAcount.Profile.City,
//                FollowedDate = !string.IsNullOrEmpty(dateadd)? dateadd : DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                FollowedDate = !string.IsNullOrEmpty(dateadd)? dateadd : DateTime.Now.ToString("o"),
                Status = EnumFollowType.Following
            };

            try
            {

                var businessMember =
                    _businessMemberRepos.Single(b => b.BusinessUserId.Equals(businessUserAccount.AccountId));
                if (businessMember == null)
                {
                    businessMember = new BusinessMember();
                    businessMember.Id = ObjectId.GenerateNewId();
                    businessMember.BusinessUserId = businessUserAccount.AccountId;
                    businessMember.BusinessName = businessUserAccount.Profile.DisplayName;
                    var members = new List<Follower>();
                    members.Add(member);
                    businessMember.Members = members;
                    _businessMemberRepos.Add(businessMember);
                }
                else
                {
                    List<Follower> members = null;
                    if (businessMember.Members == null)
                    {
                        members = new List<Follower>();
                        members.Add(member);
                    }
                    else
                    {
                        var existUser = businessMember.Members.FirstOrDefault(x => x.UserId == userAcount.AccountId);
                        if (existUser == null)
                        {
                            members = businessMember.Members.ToList();
                            members.Add(member);
                        }
                        else
                        {
                            members = businessMember.Members.ToList();
                            member = members.SingleOrDefault(m =>
                                m.UserId.Equals(userAcount.AccountId) && m.Status.Equals(EnumFollowType.Unfollow));
                            if (member != null)
                            {
                                member.Status = EnumFollowType.Following;
                                member.FollowedDate = !string.IsNullOrEmpty(dateadd)
                                    ? dateadd
                                    : DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                businessMember.Members = members;
                            }
                        }
                    }

                    businessMember.Members = members;
                    _businessMemberRepos.Update(businessMember);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool RemoveBusinessMember(Account userAcount, Account businessUserAccount)
        {
            //Cancel member
            var businessMember = _businessMemberRepos.Single(b => b.BusinessUserId.Equals(businessUserAccount.AccountId));
            if (businessMember != null)
            {
                try
                {
                    if (businessMember.Members != null)
                    {
                        var members = businessMember.Members.ToList();
                        var member = members.SingleOrDefault(m =>
                            m.UserId.Equals(userAcount.AccountId) && m.Status.Equals(EnumFollowType.Following));
                        if (member != null)
                        {
                            member.Status = EnumFollowType.Unfollow;
                            member.UnFollowedDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            businessMember.Members = members;
                        }
                    }

                    _businessMemberRepos.Update(businessMember);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerable<Follower> GetMembersOfBusiness(string businessUserId, string memberStatus = null)
        {
            var businessMember = _businessMemberRepos.Single(b => b.BusinessUserId.Equals(businessUserId));
            if (businessMember != null)
            {
                if (!string.IsNullOrEmpty(memberStatus))
                {
                    return businessMember.Members.Where(m => m.Status.Equals(memberStatus));
                }
                else
                {
                    return businessMember.Members;
                }
            }
            return null;
        }

        public IEnumerable<ShortProfile> GetFollowingBusinesses(string userId)
        {
            var businessMembers = _businessMemberRepos.Many(m => m.Members != null && m.Members.Any(f => f.UserId.Equals(userId) && f.Status == EnumFollowType.Following));
            if (businessMembers != null)
            {
                return businessMembers.Select(p => new ShortProfile
                {
                    Id = p.BusinessUserId,
                    Name = p.BusinessName
                }).AsEnumerable();
            }

            return null;
        }

    }
}
