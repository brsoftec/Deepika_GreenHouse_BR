using GH.Core.BlueCode.Entity.Common;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.InformationVault;
using GH.Core.Models;
using GH.Core.ViewModels;
using GH.Core.BlueCode.Entity.ManualHandshake;

namespace GH.Core.BlueCode.BusinessLogic
{
   public  interface IInfomationVaultBusinessLogic
    {
        void AddInformationVault(string userId);
        string GetJsonFromInformationvaultId(string id);
        void SaveInformationVault(string id, string vaultinformationjson);
        void UpdateInformationVaultById(string userId, List<KeyValue> keyValues);
        void UpdateInformationVaultById(string userId, List<FieldinformationVault> keyValues);

        void UpdateInfoFieldById(string userId, InfoField infofield);

       
        string GetInformationVaultJsonByUserId(string userid);
        BsonDocument GetInformationVaultByUserId(string userId);

        #region version2
        string GetFormVaultByAccountId(string accountId, string formName);
        void UpdateFormByAccountId(string accountId, string formName, string formString);
        string GetVaultJsonByUserId(string userId);
        void SaveVault(string informationVaultId, string informationVaultJson);

        DocumentVault GetDocumentByAccountId(string accountId);
        string CheckFileNameDocument(string accountId, string fileName);
        void StartByAccountId(string accountId, BasicProfile basicPro);

        Document InsertDocumentFieldByAccountId(string accountId, Document document);
        BsonDocument GetVaultByUserId(string userId);
        FormVault GetBasicFormVaultByUserId(string userId);
        List<FieldinformationVault> getValueFieldInformationvault(string accountId, List<FieldinformationVault> fields, bool defaultForm = true);
        #endregion version2

        void CheckManualHandshake(string accountId, string informationVaultId, string informationVaultJson);
        string EmailInviteManualHandshake(ManualHandshake manualHandshake);

        void CheckManualHandshakes(string accountId, BsonDocument oldVault);
    }
}
