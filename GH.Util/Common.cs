using System;

namespace GH.Util
{
    public class Common
    {
        public  const string CountriesKey = "CountriesKey";
        public const string  CitiesKey = "CitiesKey";
        public const string  RegionsKey = "RegionsKey";
        public const string PaymentPlanKey = "PaymentPlanKey";

        public const string ID = "0";
        public static bool IsRunningOnWebEnv()
        {
            return System.Web.HttpContext.Current != null;
          
        }
        public static int CalculateAge(DateTime birthDay)
        {
            int years = DateTime.Now.Year - birthDay.Year;

            if ((birthDay.Month > DateTime.Now.Month) || (birthDay.Month == DateTime.Now.Month && birthDay.Day > DateTime.Now.Day))
                years--;

            return years;
        }

        public static int ConvertToInt(string value)
        {
            int newvalue = 0;
            int.TryParse(value, out newvalue);
            return newvalue;

        }

    }
}
