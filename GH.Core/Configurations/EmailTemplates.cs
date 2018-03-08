using GH.Core.Helpers;
using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace GH.Core.Configurations
{
    [XmlRoot("Templates")]
    [ConfigPath("~/EmailTemplates/EmailTemplates.xml")]
    public class EmailTemplates
    {
        [XmlElement(ElementName = "Template")]
        public EmailTemplate[] Templates { get; set; }

        public EmailTemplate FindByCode(string code)
        {
            return Templates.FirstOrDefault(t => t.Code == code);
        }
    }

    public class EmailTemplate
    {
        [XmlElement]
        public string Code { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public string OpenPlaceHolder { get; set; }
        [XmlElement]
        public string ClosePlaceHolder { get; set; }
        [XmlElement]
        public string Title { get; set; }
        [XmlElement]
        public string BodyTemplatePath
        {
            get
            {
                return this._bodyTemplatePath;
            }
            set
            {
                this._bodyTemplatePath = value;
                this.Body = File.ReadAllText(CommonFunctions.MapPath(this.BodyTemplatePath), Encoding.UTF8);
            }
        }

        private string _bodyTemplatePath;

        [XmlIgnore]
        public string Body { get; private set; }

        [XmlIgnore]
        public NotificationTemplate Template
        {
            get
            {
                if (_template == null)
                {
                    _template = new NotificationTemplate
                    {
                        Code = this.Code,
                        Name = this.Name,
                        Description = this.Description,
                        Type = NotificationTemplateType.Email,
                        OpenPlaceHolder = this.OpenPlaceHolder,
                        ClosePlaceHolder = this.ClosePlaceHolder,
                        Title = this.Title,
                        Body = this.Body
                    };
                }
                return _template;
            }
            private set
            {
                this._template = value;
            }
        }

        private NotificationTemplate _template;

        public NotificationContent GetEmailContent<T>(T model)
        {
            return NotificationTemplateHelper.ReplacePlaceholderFromTemplate<T>(this.Template, model);
        }
    }
}