using GH.Core.Extensions;
using GH.Core.Helpers;
using GH.Core.Migrations;
using GH.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;

namespace GH.Core.Services
{
    public class DbMigrationService : IDbMigrationService
    {
        public void UpdateDatabase()
        {
            GreenHouseMongoDatabase.UpdateDatabase();
            UpdateSecurityQuestions();
            var migratorConfig = new Configuration();
            var dbMigrator = new DbMigrator(migratorConfig);
            dbMigrator.Update();
        }

        private void UpdateSecurityQuestions()
        {
            var path = string.Empty;
            if(HttpContext.Current!= null)
            {
                path = CommonFunctions.MapPath("~/Areas/User/Configs/SecurityQuestions.json");
            }
            else
            {
                var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                path = Path.Combine(appDir, @"..\..\..\GH.Web\Areas\User\Configs\SecurityQuestions.json");
            }
            var questionsString = File.ReadAllText(path);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var questions = serializer.Deserialize<SecurityQuestion[]>(questionsString);
            ISecurityQuestionService _service = new SecurityQuestionService();
            foreach (var item in questions)
            {
                var exist = _service.GetByCode(item.Code);
                if (exist == null)
                {
                    _service.AddSecurityQuestion(item);
                }
                else
                {
                    _service.UpdateSecurityQuestion(item);
                }
            }
        }
    }
}