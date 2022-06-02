

namespace ClearDashboard.DataAccessLayer.Models.Helpers
{
    public static class BibleRefUtils
    {
        public static string GetVerseStrShortFromBBBCCCVVV(string BBBCCCVVV)
        {
            string verseStr = "";

            // ensure that we are dealing with a full 9 character string
            if (BBBCCCVVV.Length < 9)
            {
                BBBCCCVVV = BBBCCCVVV.PadLeft(9, '0');
            }

            // get the book lookup values
            var lookup = GetBookIdDictionary();

            // get the short book name
            if (lookup.ContainsKey(BBBCCCVVV.Substring(0,3)))
            {
                verseStr = lookup[BBBCCCVVV.Substring(0, 3)];
            }
            else
            {
                verseStr = "UNK";
            }

            // parse out the verse number
            try
            {
                int numVal = Int32.Parse(BBBCCCVVV.Substring(3, 3));
                verseStr += $" {numVal}:";
            }
            catch (FormatException e)
            {
                verseStr += " 00:";
            }

            // parse out the chapter
            try
            {
                int numVal = Int32.Parse(BBBCCCVVV.Substring(6, 3));
                verseStr += $"{numVal}";
            }
            catch (FormatException e)
            {
                verseStr += "00";
            }

            return verseStr;

        }


        public static string GetBookNumFromBookName(string value)
        {
            Dictionary<string, string> lookup = new Dictionary<string, string>
            {
            { "GEN", "001" },
            { "EXO", "002" },
            { "LEV", "003" },
            { "NUM", "004" },
            { "DEU", "005" },
            { "JOS", "006" },
            { "JDG", "007" },
            { "RUT", "008" },
            { "1SA", "009" },
            { "2SA", "010" },
            { "1KI", "011" },
            { "2KI", "012" },
            { "1CH", "013" },
            { "2CH", "014" },
            { "EZR", "015" },
            { "NEH", "016" },
            { "EST", "017" },
            { "JOB", "018" },
            { "PSA", "019" },
            { "PRO", "020" },
            { "ECC", "021" },
            { "SNG", "022" },
            { "ISA", "023" },
            { "JER", "024" },
            { "LAM", "025" },
            { "EZK", "026" },
            { "DAN", "027" },
            { "HOS", "028" },
            { "JOL", "029" },
            { "AMO", "030" },
            { "OBA", "031" },
            { "JON", "032" },
            { "MIC", "033" },
            { "NAM", "034" },
            { "HAB", "035" },
            { "ZEP", "036" },
            { "HAG", "037" },
            { "ZEC", "038" },
            { "MAL", "039" },
            { "MAT", "040" },
            { "MRK", "041" },
            { "LUK", "042" },
            { "JHN", "043" },
            { "ACT", "044" },
            { "ROM", "045" },
            { "1CO", "046" },
            { "2CO", "047" },
            { "GAL", "048" },
            { "EPH", "049" },
            { "PHP", "050" },
            { "COL", "051" },
            { "1TH", "052" },
            { "2TH", "053" },
            { "1TI", "054" },
            { "2TI", "055" },
            { "TIT", "056" },
            { "PHM", "057" },
            { "HEB", "058" },
            { "JAS", "059" },
            { "1PE", "060" },
            { "2PE", "061" },
            { "1JN", "062" },
            { "2JN", "063" },
            { "3JN", "064" },
            { "JUD", "065" },
            { "REV", "066" },

            { "Genesis", "001" },
            { "Exodus", "002" },
            { "Leviticus", "003" },
            { "Numbers", "004" },
            { "Deuteronomy", "005" },
            { "Joshua", "006" },
            { "Judges", "007" },
            { "Ruth", "008" },
            { "1 Samuel", "009" },
            { "2 Samuel", "010" },
            { "1 Kings", "011" },
            { "2 Kings", "012" },
            { "1 Chronicles", "013" },
            { "2 Chronicles", "014" },
            { "Ezra", "015" },
            { "Nehemiah", "016" },
            { "Esther", "017" },
            { "Job", "018" },
            { "Psalms", "019" },
            { "Proverbs", "020" },
            { "Ecclesiastes", "021" },
            { "Song of Songs", "022" },
            { "Isaiah", "023" },
            { "Jeremiah", "024" },
            { "Lamentations", "025" },
            { "Ezekiel", "026" },
            { "Daniel", "027" },
            { "Hosea", "028" },
            { "Joel", "029" },
            { "Amos", "030" },
            { "Obadiah", "031" },
            { "Jonah", "032" },
            { "Micah", "033" },
            { "Nahum", "034" },
            { "Habakkuk", "035" },
            { "Zephaniah", "036" },
            { "Haggai", "037" },
            { "Zechariah", "038" },
            { "Malachi", "039" },
            { "Matthew", "040" },
            { "Mark", "041" },
            { "Luke", "042" },
            { "John", "043" },
            { "Acts", "044" },
            { "Romans", "045" },
            { "1 Corinthians", "046" },
            { "2 Corinthians", "047" },
            { "Galatians", "048" },
            { "Ephesians", "049" },
            { "Philippians", "050" },
            { "Colossians", "051" },
            { "1 Thessalonians", "052" },
            { "2 Thessalonians", "053" },
            { "1 Timothy", "054" },
            { "2 Timothy", "055" },
            { "Titus", "056" },
            { "Philemon", "057" },
            { "Hebrews", "058" },
            { "James", "059" },
            { "1 Peter", "060" },
            { "2 Peter", "061" },
            { "1 John", "062" },
            { "2 John", "063" },
            { "3 John", "064" },
            { "Jude", "065" },
            { "Revelation", "066" },

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
                { "The Song of Songs", "22" },
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
                { "Tobit", "67" },
                { "Judith", "68" },
                { "Esther Greek", "69" },
                { "Wisdom of Solomon", "70" },
                { "Sirach (Ecclesiasticus)", "71" },
                { "Baruch", "72" },
                { "Letter of Jeremiah", "73" },
                { "Song of 3 Young Men", "74" },
                { "Susanna", "75" },
                { "Bel and the Dragon", "76" },
                { "1 Maccabees", "77" },
                { "2 Maccabees", "78" },
                { "3 Maccabees", "79" },
                { "4 Maccabees", "80" },
                { "1 Esdras (Greek)", "81" },
                { "2 Esdras (Latin)", "82" },
                { "Prayer of Manasseh", "83" },
                { "Psalm 151", "84" },
                { "Odes", "85" },
                { "Psalms of Solomon", "86" },
                { "Joshua A. *obsolete*", "87" },
                { "Judges B. *obsolete*", "88" },
                { "Tobit S. *obsolete*", "89" },
                { "Susanna Th. *obsolete*", "90" },
                { "Daniel Th. *obsolete*", "91" },
                { "Bel Th. *obsolete*", "92" },
                { "Extra A", "93" },
                { "Extra B", "94" },
                { "Extra C", "95" },
                { "Extra D", "96" },
                { "Extra E", "97" },
                { "Extra F", "98" },
                { "Extra G", "99" },
                { "Front Matter", "100" },
                { "Back Matter", "101" },
                { "Other Matter", "102" },
                { "3 Ezra *obsolete*", "103" },
                { "Apocalypse of Ezra", "104" },
                { "5 Ezra (Latin Prologue)", "105" },
                { "6 Ezra (Latin Epilogue)", "106" },
                { "Introduction", "107" },
                { "Concordance", "108" },
                { "Glossary", "109" },
                { "Topical Index", "110" },
                { "Names Index", "111" },
                { "Daniel Greek", "112" },
                { "Psalms 152-155", "113" },
                { "2 Baruch (Apocalypse)", "114" },
                { "Letter of Baruch", "115" },
                { "Jubilees", "116" },
                { "Enoch", "117" },
                { "1 Meqabyan", "118" },
                { "2 Meqabyan", "119" },
                { "3 Meqabyan", "120" },
                { "Reproof (Proverbs 25-31)", "121" }
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
                { "001", "GEN"},
                { "002", "EXO"},
                { "003", "LEV"},
                { "004", "NUM"},
                { "005", "DEU"},
                { "006", "JOS"},
                { "007", "JDG"},
                { "008", "RUT"},
                { "009", "1SA"},
                { "010", "2SA"},
                { "011", "1KI"},
                { "012", "2KI"},
                { "013", "1CH"},
                { "014", "2CH"},
                { "015", "EZR"},
                { "016", "NEH"},
                { "017", "EST"},
                { "018", "JOB"},
                { "019", "PSA"},
                { "020", "PRO"},
                { "021", "ECC"},
                { "022", "SNG"},
                { "023", "ISA"},
                { "024", "JER"},
                { "025", "LAM"},
                { "026", "EZK"},
                { "027", "DAN"},
                { "028", "HOS"},
                { "029", "JOL"},
                { "030", "AMO"},
                { "031", "OBA"},
                { "032", "JON"},
                { "033", "MIC"},
                { "034", "NAM"},
                { "035", "HAB"},
                { "036", "ZEP"},
                { "037", "HAG"},
                { "038", "ZEC"},
                { "039", "MAL"},
                { "040", "MAT"},
                { "041", "MRK"},
                { "042", "LUK"},
                { "043", "JHN"},
                { "044", "ACT"},
                { "045", "ROM"},
                { "046", "1CO"},
                { "047", "2CO"},
                { "048", "GAL"},
                { "049", "EPH"},
                { "050", "PHP"},
                { "051", "COL"},
                { "052", "1TH"},
                { "053", "2TH"},
                { "054", "1TI"},
                { "055", "2TI"},
                { "056", "TIT"},
                { "057", "PHM"},
                { "058", "HEB"},
                { "059", "JAS"},
                { "060", "1PE"},
                { "061", "2PE"},
                { "062", "1JN"},
                { "063", "2JN"},
                { "064", "3JN"},
                { "065", "JUD"},
                { "066", "REV"}
            };
            return lookup;
        }
    }
}
