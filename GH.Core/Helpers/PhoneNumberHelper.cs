using GH.Core.Exceptions;
using GH.Core.ViewModels;
using libphonenumber;
using System;

using System.Linq;
using System.Text;
using System.Web;
using GH.Core.Models;


namespace GH.Core.Helpers
{
    public class PhoneNumberHelper
    {
        public static string GetFormatedPhoneNumber(string phoneNumber)
        {
            PhoneNumber phone = null;
            try
            {
                phoneNumber = "+" + phoneNumber.Trim('+');
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
                phone = phoneUtil.Parse(phoneNumber, "ZZ");
            }
            catch (Exception)
            {
                // throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
                return null;
            }

            if (!phone.IsValidNumber)
            {
//                throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
                return null;
            }
            else if (phone.NumberType != PhoneNumberUtil.PhoneNumberType.MOBILE && phone.NumberType != PhoneNumberUtil.PhoneNumberType.FIXED_LINE_OR_MOBILE)
            {
               // throw new CustomException(ErrorCode.PHONE_NUMBER_MOBILE_REQUIRED, GH.Lang.Regit.Phone_Number_Error_Mobile_Required);
                return null;
            }
            else
            {
                return phone.Format(libphonenumber.PhoneNumberUtil.PhoneNumberFormat.E164);
            }

        }
        public static string CheckPhoneNumber(string phoneNumber)
        {
            PhoneNumber phone = null;

            try
            {
                phoneNumber = "+" + phoneNumber.Trim('+');
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
                phone = phoneUtil.Parse(phoneNumber, "ZZ");
            }
            catch (Exception)
            {
                throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
            }

         
                if(!phone.IsValidNumber)
                throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
          //
            return phone.Format(libphonenumber.PhoneNumberUtil.PhoneNumberFormat.E164);

        }
        public static ValidPhoneViewModel ValidPhoneNumber(string lstPhoneNumber)
        {
            var rs = new ValidPhoneViewModel();
            rs.ValidPhone = true;
            rs.PhoneNumber = "";
            string[] phones = lstPhoneNumber.Split(',');
           
          
            if (phones.Length == 0)
                rs.ValidPhone = false;
            for (int i = 0; i< phones.Length; i++ )
            {
                try
                {
                    //  phoneNumber = "+" + phoneNumber.Trim('+');
                    PhoneNumber phone = null;
                    PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
                    phone = phoneUtil.Parse(phones[i], "ZZ");

                    if (!phone.IsValidNumber)
                    {
                        rs.ValidPhone = false;
                        rs.PhoneNumber = phones[i];

                    }
                       
                }
                catch (Exception)
                {
                    rs.ValidPhone = false;
                    rs.PhoneNumber = phones[i];
                }
            }
           
                return rs;
            //
        

        }
        public static PhoneViewModel GetPhoneCode(string phoneNumber)
        {
            PhoneNumber phone = null;
            var result = new PhoneViewModel();
            try
            {
                phoneNumber = "+" + phoneNumber.Trim('+');
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
                phone = phoneUtil.Parse(phoneNumber, "ZZ");
                result.CodeCountry = phone.CountryCode.ToString();
                result.PhoneNumber = phone.NationalNumber.ToString();
            }
            catch (Exception)
            {
                throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
            }


            if (!phone.IsValidNumber)
                throw new CustomException(ErrorCode.PHONE_NUMBER_INVALID_FORMAT, GH.Lang.Regit.Phone_Number_Error_Invalid);
            //
           
         
            return result;

        }        
        public static FuncResult GetFormattedPhone(string phoneNumber)
        {
            PhoneNumber phone = null;
            var result = new FormattedPhone();
            try
            {
                phoneNumber = "+" + phoneNumber.Trim('+');
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
                phone = phoneUtil.Parse(phoneNumber, "ZZ");
                result.CountryCode = phone.CountryCode.ToString();
                result.PhoneNumber = phone.NationalNumber.ToString();
            }
            catch (Exception)
            {
                return new ErrResult("phone.format.invalid");
            }

            if (!phone.IsValidNumber)
                return new ErrResult("phone.format.invalid");

            return new OkResult("phone.format.ok", result);

        }
        public static string EncodePhoneNumber(string number)
        {
            if (string.IsNullOrEmpty(number)) return string.Empty;
            StringBuilder builder = new StringBuilder();
            var length = number.Length;
            for (int i = 0; i < length - 3; i++)
            {
                builder.Append("*");
            }
            builder.Append(number.Substring(length - 3));
            return builder.ToString();
        }

        public static string EncodePinCode(string number)
        {
            if (string.IsNullOrEmpty(number))
                number = "1111";
            StringBuilder builder = new StringBuilder();
            var length = number.Length;
            for (int i = 0; i < length; i++)
            {
                builder.Append("*");
            }
            builder.Append(number.Substring(length));
            return builder.ToString();
        }
    }
}