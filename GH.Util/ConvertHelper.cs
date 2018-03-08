namespace GH.Util
{
    public static class ConvertHelper
    {
        public static int ConvertToInt(string strvalue)
        {
            int value = 0;
            int.TryParse(strvalue, out value);
            return value;
        }

        public static long ConvertToLong(string strvalue)
        {
            long value = 0;
            long.TryParse(strvalue, out value);
            return value;
        }

        public static decimal ConvertToDecimal(string strvalue)
        {
            decimal value = 0;
            decimal.TryParse(strvalue, out value);
            return value;
        }


    }
}
