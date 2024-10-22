﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClearDashboard.Common.Helpers
{
    public static class BibleRefUtils
    {
        public static string GetBookNumFromBookName(string value)
        {
            Dictionary<string, string> lookup = new Dictionary<string, string>
            {
            { "GEN", "01" },
            { "EXO", "02" },
            { "LEV", "03" },
            { "NUM", "04" },
            { "DEU", "05" },
            { "JOS", "06" },
            { "JDG", "07" },
            { "RUT", "08" },
            { "1SA", "09" },
            { "2SA", "10" },
            { "1KI", "11" },
            { "2KI", "12" },
            { "1CH", "13" },
            { "2CH", "14" },
            { "EZR", "15" },
            { "NEH", "16" },
            { "EST", "17" },
            { "JOB", "18" },
            { "PSA", "19" },
            { "PRO", "20" },
            { "ECC", "21" },
            { "SNG", "22" },
            { "ISA", "23" },
            { "JER", "24" },
            { "LAM", "25" },
            { "EZK", "26" },
            { "DAN", "27" },
            { "HOS", "28" },
            { "JOL", "29" },
            { "AMO", "30" },
            { "OBA", "31" },
            { "JON", "32" },
            { "MIC", "33" },
            { "NAM", "34" },
            { "HAB", "35" },
            { "ZEP", "36" },
            { "HAG", "37" },
            { "ZEC", "38" },
            { "MAL", "39" },
            { "MAT", "40" },
            { "MRK", "41" },
            { "LUK", "42" },
            { "JHN", "43" },
            { "ACT", "44" },
            { "ROM", "45" },
            { "1CO", "46" },
            { "2CO", "47" },
            { "GAL", "48" },
            { "EPH", "49" },
            { "PHP", "50" },
            { "COL", "51" },
            { "1TH", "52" },
            { "2TH", "53" },
            { "1TI", "54" },
            { "2TI", "55" },
            { "TIT", "56" },
            { "PHM", "57" },
            { "HEB", "58" },
            { "JAS", "59" },
            { "1PE", "60" },
            { "2PE", "61" },
            { "1JN", "62" },
            { "2JN", "63" },
            { "3JN", "64" },
            { "JUD", "65" },
            { "REV", "66" },

            { "Genesis", "01" },
            { "Exodus", "02" },
            { "Leviticus", "03" },
            { "Numbers", "04" },
            { "Deuteronomy", "05" },
            { "Joshua", "06" },
            { "Judges", "07" },
            { "Ruth", "08" },
            { "1 Samuel", "09" },
            { "2 Samuel", "10" },
            { "1 Kings", "11" },
            { "2 Kings", "12" },
            { "1 Chronicles", "13" },
            { "2 Chronicles", "14" },
            { "Ezra", "15" },
            { "Nehemiah", "16" },
            { "Esther", "17" },
            { "Job", "18" },
            { "Psalms", "19" },
            { "Proverbs", "20" },
            { "Ecclesiastes", "21" },
            { "Song of Songs", "22" },
            { "Isaiah", "23" },
            { "Jeremiah", "24" },
            { "Lamentations", "25" },
            { "Ezekiel", "26" },
            { "Daniel", "27" },
            { "Hosea", "28" },
            { "Joel", "29" },
            { "Amos", "30" },
            { "Obadiah", "31" },
            { "Jonah", "32" },
            { "Micah", "33" },
            { "Nahum", "34" },
            { "Habakkuk", "35" },
            { "Zephaniah", "36" },
            { "Haggai", "37" },
            { "Zechariah", "38" },
            { "Malachi", "39" },
            { "Matthew", "40" },
            { "Mark", "41" },
            { "Luke", "42" },
            { "John", "43" },
            { "Acts", "44" },
            { "Romans", "45" },
            { "1 Corinthians", "46" },
            { "2 Corinthians", "47" },
            { "Galatians", "48" },
            { "Ephesians", "49" },
            { "Philippians", "50" },
            { "Colossians", "51" },
            { "1 Thessalonians", "52" },
            { "2 Thessalonians", "53" },
            { "1 Timothy", "54" },
            { "2 Timothy", "55" },
            { "Titus", "56" },
            { "Philemon", "57" },
            { "Hebrews", "58" },
            { "James", "59" },
            { "1 Peter", "60" },
            { "2 Peter", "61" },
            { "1 John", "62" },
            { "2 John", "63" },
            { "3 John", "64" },
            { "Jude", "65" },
            { "Revelation", "66" },

            };

            string sTmp;
            try
            {
                sTmp = lookup[value];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sTmp = "";
            }

            return sTmp;
        }


        public static Dictionary<string, string> GetBookIdDictionary()
        {
            Dictionary<string, string> lookup = new Dictionary<string, string>
            {
                { "01", "GEN"},
                { "02", "EXO"},
                { "03", "LEV"},
                { "04", "NUM"},
                { "05", "DEU"},
                { "06", "JOS"},
                { "07", "JDG"},
                { "08", "RUT"},
                { "09", "1SA"},
                { "10", "2SA"},
                { "11", "1KI"},
                { "12", "2KI"},
                { "13", "1CH"},
                { "14", "2CH"},
                { "15", "EZR"},
                { "16", "NEH"},
                { "17", "EST"},
                { "18", "JOB"},
                { "19", "PSA"},
                { "20", "PRO"},
                { "21", "ECC"},
                { "22", "SNG"},
                { "23", "ISA"},
                { "24", "JER"},
                { "25", "LAM"},
                { "26", "EZK"},
                { "27", "DAN"},
                { "28", "HOS"},
                { "29", "JOL"},
                { "30", "AMO"},
                { "31", "OBA"},
                { "32", "JON"},
                { "33", "MIC"},
                { "34", "NAM"},
                { "35", "HAB"},
                { "36", "ZEP"},
                { "37", "HAG"},
                { "38", "ZEC"},
                { "39", "MAL"},
                { "40", "MAT"},
                { "41", "MRK"},
                { "42", "LUK"},
                { "43", "JHN"},
                { "44", "ACT"},
                { "45", "ROM"},
                { "46", "1CO"},
                { "47", "2CO"},
                { "48", "GAL"},
                { "49", "EPH"},
                { "50", "PHP"},
                { "51", "COL"},
                { "52", "1TH"},
                { "53", "2TH"},
                { "54", "1TI"},
                { "55", "2TI"},
                { "56", "TIT"},
                { "57", "PHM"},
                { "58", "HEB"},
                { "59", "JAS"},
                { "60", "1PE"},
                { "61", "2PE"},
                { "62", "1JN"},
                { "63", "2JN"},
                { "64", "3JN"},
                { "65", "JUD"},
                { "66", "REV"}
            };
            return lookup;
        }
    }
}
