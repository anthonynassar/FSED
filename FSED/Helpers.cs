using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSED
{
    public static class Helpers
    {
        public static string DetectDateTimeFormat(string dateTime)
        {
            string[] formats = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "dd-MM-yy HH:mm:ss tt", "dd/MM/yyyy HH:mm:ss",
                                 "yyyy:MM:dd HH:mm:ss", "dd-MM-yy hh:mm:ss tt", "M/d/yyyy hh:mm:ss tt",
                "M/d/yyyy h:mm:ss tt", "M/d/yyyy H:mm:ss tt", "yyyy'-'MM'-'dd HH':'mm':'ss'Z'", "yyyy-MM-dd HH:mm:ssZ"};

            DateTime dateValue;

            foreach (string dateStringFormat in formats)
            {
                if (DateTime.TryParseExact(dateTime, dateStringFormat,
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out dateValue))
                    //Console.WriteLine("Converted '{0}' to {1}.", dateStringFormat, dateValue.ToString("yyyy-MM-dd"));                
                    return dateStringFormat;
            }
            return "ErrorFormat";
        }

        public static bool CompareTimeIntervals(Interval outside, Interval inside)
        {
            if (outside.BorneInf.CompareTo(inside.BorneInf) <= 0 && outside.BorneSup.CompareTo(inside.BorneSup) >= 0)
                return true;

            return false;
        }

        public static string GetSpecificValue(string currentSSAddress, string granularity)
        {
            string[] values = currentSSAddress.Split(':');
            switch (granularity)
            {
                case "Country":
                    return values[0];
                case "City":
                    return values[1];
                case "Department":
                    return values[3];
                case "Street":
                    return values[4];
                default:
                    return "";
            }
        }
    }
}
