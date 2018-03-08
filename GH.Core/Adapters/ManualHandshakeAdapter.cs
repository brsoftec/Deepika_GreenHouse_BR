using GH.Core.BlueCode.Entity.ManualHandshake;
using GH.Core.Services;
using GH.Core.ViewModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Adapters
{
    public static class ManualHandshakeAdapter
    {
        public static ManualHandshakeViewModel ModelToViewModel(ManualHandshake model)
        {
            var rs = new ManualHandshakeViewModel();
            var accountService = new AccountService();
            if (string.IsNullOrEmpty(model.accountId))
                return rs;
            rs.Id = model.Id.ToString();
            rs.accountId = model.accountId;
            rs.description = model.description;
            rs.email = model.email;

            var account = accountService.GetByAccountId(model.accountId);
            rs.name = account.Profile.DisplayName ?? account.Profile.FirstName + ' ' + account.Profile.LastName;

            rs.toEmail = model.toEmail;
            rs.toName = model.toName;
            rs.expiry.indefinite = model.expiry.indefinite;
            rs.expiry.date = model.expiry.date;

            rs.synced = model.synced;
            rs.status = model.status;

           
            if (!string.IsNullOrEmpty(model.toAccountId))
            {
                var toAccount = accountService.GetByAccountId(model.toAccountId);
                if (toAccount != null)
                {
                    rs.toAccountId = model.toAccountId;
                    rs.toEmail = toAccount.Profile.Email;
                    rs.toName = toAccount.Profile.DisplayName ?? toAccount.Profile.FirstName + ' ' + toAccount.Profile.LastName;
                }
            }
            if (model.fields.Count > 0)
            {
                foreach (var item in model.fields)
                {
                    var field = new FieldManualHandshakeViewModel();

                    field.label = item.label;
                    field.jsPath = item.jsPath;
                    field.selected = item.selected;
                    if (field != null)
                        rs.fields.Add(field);
                }

            }

            try
            {
                rs.notifyFormat = model.notifyFormat;
            }
            catch { }
            return rs;
        }

        public static ManualHandshake ViewModelToModel(ManualHandshakeViewModel vm)
        {
            var rs = new ManualHandshake();
            if (!string.IsNullOrEmpty(vm.Id))
                rs.Id = new ObjectId(vm.Id);
            rs.accountId = vm.accountId;
            rs.name = vm.name;
            rs.email = vm.email;
            rs.description = vm.description;
            rs.expiry.indefinite = vm.expiry.indefinite;
            rs.expiry.date = vm.expiry.date;
            if (!string.IsNullOrEmpty(vm.toAccountId))
                rs.toAccountId = vm.toAccountId;

            rs.toEmail = vm.toEmail;
            rs.toAccountId = vm.toAccountId;
            rs.toName = vm.toName;

            rs.synced = vm.synced;
            rs.status = vm.status;
            if (vm.fields.Count > 0)
            {
                foreach (var item in vm.fields)
                {
                    var field = new FieldManualHandshake();

                    field.label = item.label;
                    field.jsPath = item.jsPath;
                    field.selected = item.selected;
                    if (field != null)
                        rs.fields.Add(field);
                }

            }
            try
            {
                rs.notifyFormat = vm.notifyFormat;
            }
            catch { }
            return rs;

        }
    }
}