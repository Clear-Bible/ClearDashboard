using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class BibleBookScope
    {
        public static bool IsBibleBook(string BookID)
        {
            var dic = GenerateBookDictionary();
            if (dic.ContainsKey(BookID))
            {
                return true;
            }
            return false;
        }

        public static Dictionary<string, string> GenerateBookDictionary()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "GEN", "GEN" },
                { "EXO", "EXO" },
                { "LEV", "LEV" },
                { "NUM", "NUM" },
                { "DEU", "DEU" },
                { "JOS", "JOS" },
                { "JDG", "JDG" },
                { "RUT", "RUT" },
                { "1SA", "1SA" },
                { "2SA", "2SA" },
                { "1KI", "1KI" },
                { "2KI", "2KI" },
                { "1CH", "1CH" },
                { "2CH", "2CH" },
                { "EZR", "EZR" },
                { "NEH", "NEH" },
                { "EST", "EST" },
                { "JOB", "JOB" },
                { "PSA", "PSA" },
                { "PRO", "PRO" },
                { "ECC", "ECC" },
                { "SNG", "SNG" },
                { "ISA", "ISA" },
                { "JER", "JER" },
                { "LAM", "LAM" },
                { "EZK", "EZK" },
                { "DAN", "DAN" },
                { "HOS", "HOS" },
                { "JOL", "JOL" },
                { "AMO", "AMO" },
                { "OBA", "OBA" },
                { "JON", "JON" },
                { "MIC", "MIC" },
                { "NAM", "NAM" },
                { "HAB", "HAB" },
                { "ZEP", "ZEP" },
                { "HAG", "HAG" },
                { "ZEC", "ZEC" },
                { "MAL", "MAL" },
                { "MAT", "MAT" },
                { "MRK", "MRK" },
                { "LUK", "LUK" },
                { "JHN", "JHN" },
                { "ACT", "ACT" },
                { "ROM", "ROM" },
                { "1CO", "1CO" },
                { "2CO", "2CO" },
                { "GAL", "GAL" },
                { "EPH", "EPH" },
                { "PHP", "PHP" },
                { "COL", "COL" },
                { "1TH", "1TH" },
                { "2TH", "2TH" },
                { "1TI", "1TI" },
                { "2TI", "2TI" },
                { "TIT", "TIT" },
                { "PHM", "PHM" },
                { "HEB", "HEB" },
                { "JAS", "JAS" },
                { "1PE", "1PE" },
                { "2PE", "2PE" },
                { "1JN", "1JN" },
                { "2JN", "2JN" },
                { "3JN", "3JN" },
                { "JUD", "JUD" },
                { "REV", "REV" }
            };

            return dic;
        }
    }
}
