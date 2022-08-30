using System;
using System.Text.RegularExpressions;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class Helpers
    {
        /// <summary>
        /// Find the textID and convert it to standard, 12-digit string format
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static string CheckManuscriptId(long s)
        {
            string id = s.ToString();

            if (id.Length == 11)
            {
                // prefix it with a leading zero
                id = "0" + id;
                id = id.Substring(id.Length - 12, 12);
            }

            return id;
        }

        /// <summary>
        /// Find the textID and convert it to standard, 11-digit string format
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static string CheckTranslationId(long s)
        {
            string id = s.ToString();

            if (id.Length == 10)
            {
                // prefix it with a leading zero
                id = "0" + id;
                id = id.Substring(id.Length - 11, 11);
            }

            return id;
        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }


        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// return a string that has valid filename characters
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string SanitizeFileName(string fileName)
        {
            Regex InvalidFileRegex = new Regex(string.Format("[{0}]", Regex.Escape(@"<>:""/\|?*")));
            return InvalidFileRegex.Replace(fileName, string.Empty);
        }
    }
}
