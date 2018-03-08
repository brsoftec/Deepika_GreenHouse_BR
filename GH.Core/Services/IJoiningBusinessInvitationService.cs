﻿using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface IJoiningBusinessInvitationService
    {
        List<Account> SearchUserForInvitation(string keyword, ObjectId businessAccountId, int? start = null, int? length = null);
        List<JoiningBusinessInvitation> GetAllJoiningBusinessInivations(ObjectId accountId);
        List<JoiningBusinessInvitation> GetAllWorkflowInvitationsFromBusiness(ObjectId businessId);
        JoiningBusinessInvitation Invite(ObjectId from, ObjectId to, List<string> roles, string inviteId = null);
        void AcceptInvitation(ObjectId invitationId);
        void DenyInvitation(ObjectId invitationId);
        void Accept(ObjectId invitationId, ObjectId accountId);
        void Deny(ObjectId invitationId, ObjectId accountId);
        void RemoveInvitation(ObjectId invitationId);
     
    }
}