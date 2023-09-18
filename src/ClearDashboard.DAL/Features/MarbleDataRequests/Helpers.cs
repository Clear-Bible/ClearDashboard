using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public static class Helpers
    {
        /// <summary>
        /// A function that takes in a book number and translate that to a book name
        /// </summary>
        /// <param name="bookNum"></param>
        /// <returns></returns>
        public static string GetFilenameFromMarbleBook(int bookNum)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            dic.Add(1, "GEN");
            dic.Add(2, "EXO");
            dic.Add(3, "LEV");
            dic.Add(4, "NUM");
            dic.Add(5, "DEU");
            dic.Add(6, "JOS");
            dic.Add(7, "JDG");
            dic.Add(8, "RUT");
            dic.Add(9, "1SA");
            dic.Add(10, "2SA");
            dic.Add(11, "1KI");
            dic.Add(12, "2KI");
            dic.Add(13, "1CH");
            dic.Add(14, "2CH");
            dic.Add(15, "EZR");
            dic.Add(16, "NEH");
            dic.Add(17, "EST");
            dic.Add(18, "JOB");
            dic.Add(19, "PSA");
            dic.Add(20, "PRO");
            dic.Add(21, "ECC");
            dic.Add(22, "SNG");
            dic.Add(23, "ISA");
            dic.Add(24, "JER");
            dic.Add(25, "LAM");
            dic.Add(26, "EZK");
            dic.Add(27, "DAN");
            dic.Add(28, "HOS");
            dic.Add(29, "JOL");
            dic.Add(30, "AMO");
            dic.Add(31, "OBA");
            dic.Add(32, "JON");
            dic.Add(33, "MIC");
            dic.Add(34, "NAM");
            dic.Add(35, "HAB");
            dic.Add(36, "ZEP");
            dic.Add(37, "HAG");
            dic.Add(38, "ZEC");
            dic.Add(39, "MAL");
            dic.Add(40, "MAT");
            dic.Add(41, "MRK");
            dic.Add(42, "LUK");
            dic.Add(43, "JHN");
            dic.Add(44, "ACT");
            dic.Add(45, "ROM");
            dic.Add(46, "1CO");
            dic.Add(47, "2CO");
            dic.Add(48, "GAL");
            dic.Add(49, "EPH");
            dic.Add(50, "PHP");
            dic.Add(51, "COL");
            dic.Add(52, "1TH");
            dic.Add(53, "2TH");
            dic.Add(54, "1TI");
            dic.Add(55, "2TI");
            dic.Add(56, "TIT");
            dic.Add(57, "PHM");
            dic.Add(58, "HEB");
            dic.Add(59, "JAS");
            dic.Add(60, "1PE");
            dic.Add(61, "2PE");
            dic.Add(62, "1JN");
            dic.Add(63, "2JN");
            dic.Add(64, "3JN");
            dic.Add(65, "JUD");
            dic.Add(66, "REV");

            if (dic.ContainsKey(bookNum))
            {
                return dic[bookNum];
            }

            return "";
        }

        
    }
}
