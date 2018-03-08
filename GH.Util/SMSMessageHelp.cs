using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH.Util
{
    public class SMSMessageHelp
    {
        public static string CreateSMSMessagePin()
        {
            return new Random().Next(0, 9999).ToString();
        }
    }
}
