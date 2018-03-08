using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.InformationVault
{
    public sealed class EnumVaultGroup
    {
        public static readonly string Basic = "basicInformation";
        public static readonly string BasicName = "Basic information";
        public static readonly string Contact = "contact";
        public static readonly string Address = "groupAddress";
        public static readonly string Financial = "groupFinancial";
        public static readonly string Government = "groupGovernmentID";
        public static readonly string Family = "family";
        public static readonly string Pet = "pet";
        public static readonly string Membership = "membership";
        public static readonly string Employment = "employment";
        public static readonly string Education = "education";
        public static readonly string Others = "others";
        public static readonly string Document = "document";
    }
    public sealed class EnumJsPathVaultForm
    {
        public static readonly string JsBasic = ".basicInformation";
        public static readonly string JsContact = ".contact";

        public static readonly string JsAddress = ".address";

        public static readonly string JsCurrentAddress = ".address.currentAddress";
        public static readonly string JsDeliveryAddress = ".address.deliveryAddress";
        public static readonly string JsBillingAddress = ".address.billingAddress";
        public static readonly string JsMailingAddress = ".address.mailingAddress";
        public static readonly string Jspobox = ".address.pobox";

        public static readonly string JsFinancial = ".financial";

        public static readonly string JsBankAccount = ".financial.bankAccount";
        public static readonly string JsInvestment = ".financial.investment";
        public static readonly string JsInsurance = ".financial.insurance";
        public static readonly string JsMasterCard = ".financial.masterCard";
        public static readonly string JsVisaCard = ".financial.visaCard";
        public static readonly string JsBankCard = ".financial.bankCard";
        public static readonly string JsPaypal = ".financial.paypal";

        public static readonly string JsGovernmentID = ".governmentID";

        public static readonly string JsBirthCertificate = "governmentID.birthCertificate";
        public static readonly string JsDriverLicenseCard = ".governmentID.driverLicenseCard";
        public static readonly string JsHealthCard = ".governmentID.healthCard";
        public static readonly string JsMedicalBenefitCard = ".governmentID.medicalBenefitCard";
        public static readonly string JsPassportID = ".governmentID.passportID";
        public static readonly string JsNationalID = ".governmentID.nationalID";
        public static readonly string JsCustomGovernmentID = ".governmentID.CustomGovernmentID";
        public static readonly string JsPermanentResidenceCard = ".governmentID.permanentResidenceCard";
        public static readonly string JsSocialInsuranceCard = ".governmentID.socialInsuranceCard";
        public static readonly string JsTaxID = ".governmentID.taxID";

        public static readonly string JsFamily = ".family";
        public static readonly string JsPet = ".pet";
        public static readonly string JsMembership = ".membership";
        public static readonly string JsEmployment = ".employment";
        public static readonly string JsEducation = ".education";
        public static readonly string JsOthers = ".others";
        public static readonly string JsDocument = ".document";
        public static readonly string JsCustom = "Custom";
    }
    public sealed class EnumListJsPathVaultForm
    {
        public static readonly List<string> ListJsPathVaultForm = new List<string>()
        {
            ".basicInformation", ".contact", ".address.currentAddress", ".address.deliveryAddress", ".address.billingAddress",
            ".address.mailingAddress", ".address.pobox",
            ".governmentID.birthCertificate", ".governmentID.driverLicenseCard", ".governmentID.healthCard", ".governmentID.medicalBenifitCard",
            ".governmentID.passportID", ".governmentID.permanentResidenceIdCard", ".governmentID.taxCard", ".governmentID.nationalID",
            ".family", ".membership", ".employment", ".education", ".others", "Custom" };
        public static readonly List<string> Basic = new List<string>()
        {
        "title", "firstName", "middleName", "lastName", "alias", "dob", "gender", "location", "sexuality", "languages",
        "state", "city", "ethnicity", "religion", "disability", "note", "relationshipStatus"
        };

    }
   
}

             