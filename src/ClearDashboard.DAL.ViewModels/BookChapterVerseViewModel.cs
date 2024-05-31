using System.Reflection.Metadata;
using ClearDashboard.DataAccessLayer.Models;
using static System.String;

namespace ClearDashboard.DAL.ViewModels
{
    public class BookChapterVerseViewModel : ViewModelBase<BookChapterVerse>
    {
        public bool VerseChangeInProgress { get; set; }
        public bool ChapterChangeInProgress { get; set; }
        public bool BookChangeInProgress { get; set; }


#nullable disable
        public BookChapterVerseViewModel() : base()
        {

        }

        public BookChapterVerseViewModel(BookChapterVerse entity) : base(entity)
        {

        }

        public List<string> BibleBookList
        {
            get => Entity?.BibleBookList;
            set
            {
                if (Entity != null)
                {
                    Entity.BibleBookList = value;
                }
                NotifyOfPropertyChange();
            }
        }

        public string BookAbbr
        {
            get => Entity?.BookAbbr;
            set
            {
                if (Entity != null)
                {
                    Entity.BookAbbr = value;
                }
                NotifyOfPropertyChange();
            }
        }

        public List<int> ChapterNumbers
        {
            get => Entity?.ChapterNumbers;
            set
            {
                if (Entity != null)
                {
                    Entity.ChapterNumbers = value;
                }
                NotifyOfPropertyChange();
            }
        }

        public List<int> VerseNumbers
        {
            get => Entity?.VerseNumbers;
            set
            {
                if (Entity != null)
                {
                    Entity.VerseNumbers = value;
                }
                NotifyOfPropertyChange();
            }
        }


        public int BookNum
        {
            get => Entity.BookNum;
            set
            {
                if (Entity != null)
                {
                    Entity.BookNum = value;
                }
                NotifyOfPropertyChange();
            }
        }


        /// <summary>
        /// The Book ID number as a padded string. Automatically calculated from BookStr.
        /// </summary>
        public string Book => Entity?.BookNum.ToString().PadLeft(3, '0');

        public string BookName
        {
            get => Entity?.BookName;
            set
            {
                if (Entity != null && Entity?.BookName != value) 
                {
                    Entity.BookName = value;
                    BookNum = GetIntBookNumFromBookName(value);
                    NotifyOfPropertyChange(nameof(Book));
                    NotifyOfPropertyChange(nameof(BookName));
                    NotifyOfPropertyChange(nameof(BBBCCCVVV));
                }
            }
        }

        /// <summary>
        /// Chapter number as an int.
        /// </summary>
        public int? Chapter
        {
            get => Entity.Chapter;
            set
            {
                if (Entity.Chapter != value)
                {
                    if (value == 0)
                    {
                        value = 1;
                    }

                    Entity.Chapter = value;
                    NotifyOfPropertyChange(nameof(Chapter));
                    NotifyOfPropertyChange(nameof(BBBCCCVVV));
                }
            }
        }

        public int? ChapterNum => Chapter;


        /// <summary>
        /// Chapter number as padded text. Automatically calculated from Chapter.
        /// </summary>
        public string ChapterIdText => Chapter.ToString().PadLeft(3, '0');

        /// <summary>
        /// Chapter number as padded text. Automatically calculated from Chapter.
        /// </summary>
        public string ChapterText => Chapter.ToString();

        private int? _verse { get; set; } = 0;

        /// <summary>
        /// The verse number as an int.
        /// </summary>
        public int? Verse
        {
            get => Entity?.Verse;
            set
            {
                if (Entity != null)
                {
                    Entity.Verse = value;
                }
                NotifyOfPropertyChange(nameof(Verse));
                NotifyOfPropertyChange(nameof(VerseNum));
                NotifyOfPropertyChange(nameof(VerseIdText));
                NotifyOfPropertyChange(nameof(BBBCCCVVV));
                NotifyOfPropertyChange(nameof(BBBCCCVVVasInteger));
            }
        }


        public int? VerseNum => Verse;

        /// <summary>
        /// Verse ID as a string. Automatically calculated from Verse.
        /// </summary>
        public string VerseIdText => Verse.ToString().PadLeft(3, '0');

        /// <summary>
        /// A string with the Clear.Bible format of verse location ID. It is automatically calculated. 
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string BBBCCCVVV => Concat(Book, ChapterIdText, VerseIdText);

        public int BBBCCCVVVasInteger => Convert.ToInt32(BBBCCCVVV);

        public int GetBBBCCCVVV()
        {
            return Convert.ToInt32(BBBCCCVVV);
        }

        /// <summary>
        /// Based on the properties of this object, it returns the complete verse location ID. Modified function for compatibility.
        /// </summary>
        /// <returns>A string with the Clear.Bible format of verse location ID.</returns>
        public string GetVerseId()
        {
            if (BookAbbr != "")
            {
                var bbb = GetBookNumFromBookName(BookAbbr);
                SetVerseFromId(bbb + ChapterIdText + VerseIdText);
            }

            return BBBCCCVVV;
        }

        /// <summary>
        /// Uses a Paratext verse ID to set this object's properties.
        /// </summary>
        /// <param name="verseId">Paratext verse ID</param>
        /// <returns>Success of parsing IncommingVerseLocation.</returns>
        public bool SetVerseFromId(string verseId)
        {
            if (verseId is null)
            {
                return false;
            }

            // Convert the number into a string we can parse.
            var verseLocationId = verseId.PadLeft(9, '0');

            var bookNumStr = verseLocationId.Substring(0, 3);
            var chapterIdText = verseLocationId.Substring(3, 3);
            var verseIdText = verseLocationId.Substring(6, 3);

            if (verseIdText == "000")
            {
                verseIdText = "001";
                verseLocationId = bookNumStr + chapterIdText + verseIdText;
            }

            if (verseLocationId == BBBCCCVVV)
            {
                return false;
            }
            
            if (verseIdText != "001")
            {
                VerseChangeInProgress = true;
            }
            else if (chapterIdText != "001")
            {
                ChapterChangeInProgress = true;
            }
            else
            {
                BookChangeInProgress = true;
            }

            // Test each parse, and only return a TRUE if they all are parsed.
            if (int.TryParse(bookNumStr, out var bookNum))
            {
                // The book number for use in the array used in the pull down list.
                BookNum = bookNum;
                BookName = GetShortBookNameFromBookNum(bookNumStr);
                BookAbbr = BookName;
                BookChangeInProgress = false;
            }
            else
            {
                return false;
            }

            if (int.TryParse(chapterIdText, out Int32 chapterNum))
            {
                Chapter = chapterNum;
                ChapterChangeInProgress = false;
            }
            else
            {
                return false;
            }

            if (int.TryParse(verseIdText, out int verseNum))
            {
                Verse = verseNum;

                if (verseNum == 0)
                {
                    Verse = 1;
                }

                VerseChangeInProgress = false;
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Just like SetVerseFromId except that it accepts an int.
        /// </summary>
        /// <param name="incomingVerseLocation">Paratext verse ID</param>
        /// <returns>Success of parsing IncommingVerseLocation.</returns>
        public bool VerseLocationConverter(int incomingVerseLocation)
        {
            return SetVerseFromId(incomingVerseLocation.ToString());
        }


        /// <summary>
        /// Sets the Bible BookName of this object.
        /// </summary>
        /// <param name="bookNum">The number you want set.</param>
        /// <param name="isParatext"></param>
        public void SetThisBookName(int bookNum, bool isParatext = false)
        {
            if (isParatext && bookNum > 39)
            {
                bookNum--;
            }

            this.BookNum = bookNum;
        }

        /// <summary>
        /// A list of books of the Bible in English.
        /// </summary>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>A list of books of the Bible in English.</returns>
        public static List<string> BibleBookListEn(bool isParatext = false)
        {
            var bibleBooks = new List<string>
            {
                Empty, // Offset by one.
                "Genesis",
                "Exodus",
                "Leviticus",
                "Numbers",
                "Deuteronomy",
                "Joshua",
                "Judges",
                "Ruth",
                "1 Samuel (1 Kings)",
                "2 Samuel (2 Kings)",
                "1 Kings (3 Kings)",
                "2 Kings (4 Kings)",
                "1 Chronicles",
                "2 Chronicles",
                "Ezra",
                "Nehemiah",
                "Esther",
                "Job",
                "Psalms",
                "Proverbs",
                "Ecclesiastes",
                "Song of Songs",
                "Isaiah",
                "Jeremiah",
                "Lamentations",
                "Ezekiel",
                "Daniel",
                "Hosea",
                "Joel",
                "Amos",
                "Obadiah",
                "Jonah",
                "Micah",
                "Nahum",
                "Habakkuk",
                "Zephaniah",
                "Haggai",
                "Zechariah",
                "Malachi",
                "Matthew",
                "Mark",
                "Luke",
                "John",
                "Acts",
                "Romans",
                "1 Corinthians",
                "2 Corinthians",
                "Galatians",
                "Ephesians",
                "Philippians",
                "Colossians",
                "1 Thessalonians",
                "2 Thessalonians",
                "1 Timothy",
                "2 Timothy",
                "Titus",
                "Philemon",
                "Hebrews",
                "James",
                "1 Peter",
                "2 Peter",
                "1 John",
                "2 John",
                "3 John",
                "Jude",
                "Revelation"
            };

            return bibleBooks;
        }

        /// <summary>
        /// Given a Bible book number, it returns an English book name.
        /// </summary>
        /// <param name="bookNum">Bible book number as an int.</param>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>An English Bible book name.</returns>
        public static string BookId2BookName(int bookNum, bool isParatext = false)
        {
            var books = GetBookIdDictionary();
            var bookBBB = bookNum.ToString().PadLeft(3, '0');
            if (books.ContainsKey(bookBBB))
            {
                return books[bookBBB];
            }

            return string.Empty;
        }

        public string GetVerseRefAbbreviated()
        {
            var bookShort = BibleBookAcronymListEn();
            if (bookShort.Count > BookNum)
            {
                return $"{bookShort[BookNum]}-{Chapter}:{Verse}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a Bible book number when given an English Bible book name.
        /// </summary>
        /// <param name="bookName">The English name of the book you are looking for. A partial name or acronym works too.</param>
        /// <returns>Returns a Bible book number as an int.</returns>
        public int GetIntBookNumFromBookName(string bookName)
        {
            var number = GetBookNumFromBookName(bookName);
            if (IsNumeric(number))
            {
                return Convert.ToInt32(number);
            }

            return 0;
        }

        public bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        /// <summary>
        /// A list of acronyms of the books of the Bible in English.
        /// </summary>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>A list of acronyms of the books of the Bible in English.</returns>
        public static List<string> BibleBookAcronymListEn(bool isParatext = false)
        {
            var bibleBooks = new List<string>
            {
                Empty, // Offset by one.
                "GEN",
                "EXO",
                "LEV",
                "NUM",
                "DEU",
                "JOS",
                "JDG",
                "RUT",
                "1SA",
                "2SA",
                "1KI",
                "2KI",
                "1CH",
                "2CH",
                "EZR",
                "NEH",
                "EST",
                "JOB",
                "PSA",
                "PRO",
                "ECC",
                "SNG",
                "ISA",
                "JER",
                "LAM",
                "EZK",
                "DAN",
                "HOS",
                "JOL",
                "AMO",
                "OBA",
                "JON",
                "MIC",
                "NAM",
                "HAB",
                "ZEP",
                "HAG",
                "ZEC",
                "MAL",
                "MAT",
                "MRK",
                "LUK",
                "JHN",
                "ACT",
                "ROM",
                "1CO",
                "2CO",
                "GAL",
                "EPH",
                "PHP",
                "COL",
                "1TH",
                "2TH",
                "1TI",
                "2TI",
                "TIT",
                "PHM",
                "HEB",
                "JAS",
                "1PE",
                "2PE",
                "1JN",
                "2JN",
                "3JN",
                "JUD",
                "REV"
            };

            if (isParatext)
            {
                // Add a blank book between the OT and NT to match the Paratext format.
                bibleBooks.Insert(40, Empty);
            }

            return bibleBooks;
        }

        public static Dictionary<string, string> GetFullBookNameFromBookNum()
        {
            var lookup = new Dictionary<string, string>
            {
                { "001", "Genesis"},
                { "002", "Exodus"},
                { "003", "Leviticus"},
                { "004", "Numbers"},
                { "005", "Deuteronomy"},
                { "006", "Joshua"},
                { "007", "Judges"},
                { "008", "Ruth"},
                { "009", "1 Samuel"},
                { "010", "2 Samuel"},
                { "011", "1 Kings"},
                { "012", "2 Kings"},
                { "013", "1 Chronicles"},
                { "014", "2 Chronicles"},
                { "015", "Ezra"},
                { "016", "Nehemiah"},
                { "017", "Esther"},
                { "018", "Job"},
                { "019", "Psalms"},
                { "020", "Proverbs"},
                { "021", "Ecclesiastes"},
                { "022", "Song of Songs"},
                { "023", "Isaiah"},
                { "024", "Jeremiah"},
                { "025", "Lamentations"},
                { "026", "Ezekiel"},
                { "027", "Daniel"},
                { "028", "Hosea"},
                { "029", "Joel"},
                { "030", "Amos"},
                { "031", "Obadiah"},
                { "032", "Jonah"},
                { "033", "Micah"},
                { "034", "Nahum"},
                { "035", "Habakkuk"},
                { "036", "Zephaniah"},
                { "037", "Haggai"},
                { "038", "Zechariah"},
                { "039", "Malachi"},
                { "040", "Matthew"},
                { "041", "Mark"},
                { "042", "Luke"},
                { "043", "John"},
                { "044", "Acts"},
                { "045", "Romans"},
                { "046", "1 Corinthians"},
                { "047", "2 Corinthians"},
                { "048", "Galatians"},
                { "049", "Ephesians"},
                { "050", "Philippians"},
                { "051", "Colossians"},
                { "052", "1 Thessalonians"},
                { "053", "2 Thessalonians"},
                { "054", "1 Timothy"},
                { "055", "2 Timothy"},
                { "056", "Titus"},
                { "057", "Philemon"},
                { "058", "Hebrews"},
                { "059", "James"},
                { "060", "1 Peter"},
                { "061", "2 Peter"},
                { "062", "1 John"},
                { "063", "2 John"},
                { "064", "3 John"},
                { "065", "Jude"},
                { "066", "Revelation"},

            };

            return lookup;
        }

        public static string GetShortBookNameFromBookNum(string value)
        {
            var bookList = GetBookIdDictionary();
            if (bookList.ContainsKey(value))
            {
                return bookList[value];
            }

            return "";
        }

        public static Dictionary<string, string> GetBookIdDictionary()
        {
            Dictionary<string, string> lookup = new Dictionary<string, string>
            {
                { "001", "GEN" },
                { "002", "EXO" },
                { "003", "LEV" },
                { "004", "NUM" },
                { "005", "DEU" },
                { "006", "JOS" },
                { "007", "JDG" },
                { "008", "RUT" },
                { "009", "1SA" },
                { "010", "2SA" },
                { "011", "1KI" },
                { "012", "2KI" },
                { "013", "1CH" },
                { "014", "2CH" },
                { "015", "EZR" },
                { "016", "NEH" },
                { "017", "EST" },
                { "018", "JOB" },
                { "019", "PSA" },
                { "020", "PRO" },
                { "021", "ECC" },
                { "022", "SNG" },
                { "023", "ISA" },
                { "024", "JER" },
                { "025", "LAM" },
                { "026", "EZK" },
                { "027", "DAN" },
                { "028", "HOS" },
                { "029", "JOL" },
                { "030", "AMO" },
                { "031", "OBA" },
                { "032", "JON" },
                { "033", "MIC" },
                { "034", "NAM" },
                { "035", "HAB" },
                { "036", "ZEP" },
                { "037", "HAG" },
                { "038", "ZEC" },
                { "039", "MAL" },
                { "040", "MAT" },
                { "041", "MRK" },
                { "042", "LUK" },
                { "043", "JHN" },
                { "044", "ACT" },
                { "045", "ROM" },
                { "046", "1CO" },
                { "047", "2CO" },
                { "048", "GAL" },
                { "049", "EPH" },
                { "050", "PHP" },
                { "051", "COL" },
                { "052", "1TH" },
                { "053", "2TH" },
                { "054", "1TI" },
                { "055", "2TI" },
                { "056", "TIT" },
                { "057", "PHM" },
                { "058", "HEB" },
                { "059", "JAS" },
                { "060", "1PE" },
                { "061", "2PE" },
                { "062", "1JN" },
                { "063", "2JN" },
                { "064", "3JN" },
                { "065", "JUD" },
                { "066", "REV" },
                { "067", "TOB" },
                { "068", "JDT" },
                { "069", "ESG" },
                { "070", "WIS" },
                { "071", "SIR" },
                { "072", "BAR" },
                { "073", "LJE" },
                { "074", "S3Y" },
                { "075", "SUS" },
                { "076", "BEL" },
                { "077", "1MA" },
                { "078", "2MA" },
                { "079", "3MA" },
                { "080", "4MA" },
                { "081", "1ES" },
                { "082", "2ES" },
                { "083", "MAN" },
                { "084", "PS2" },
                { "085", "ODA" },
                { "086", "PSS" },
                { "087", "JDA" },
                { "088", "JDB" },
                { "089", "TBS" },
                { "090", "SST" },
                { "091", "DNT" },
                { "092", "BLT" },
                { "093", "XXA" },
                { "094", "XXB" },
                { "095", "XXC" },
                { "096", "XXD" },
                { "097", "XXE" },
                { "098", "XXF" },
                { "099", "XXG" },
                { "100", "FRT" },
                { "101", "BAK" },
                { "102", "OTH" },
                { "103", "3ES" },
                { "104", "EZA" },
                { "105", "5EZ" },
                { "106", "6EZ" },
                { "107", "INT" },
                { "108", "CNC" },
                { "109", "GLO" },
                { "110", "TDX" },
                { "111", "NDX" },
                { "112", "DAG" },
                { "113", "PS3" },
                { "114", "2BA" },
                { "115", "LBA" },
                { "116", "JUB" },
                { "117", "ENO" },
                { "118", "1MQ" },
                { "119", "2MQ" },
                { "120", "3MQ" },
                { "121", "REP" },
            };
            return lookup;
        }

        public static string GetBookNumFromBookName(string value)
        {
            var lookup = new Dictionary<string, string>
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
            { "TOB", "067" },
            { "JDT", "068" },
            { "ESG", "069" },
            { "WIS", "070" },
            { "SIR", "071" },
            { "BAR", "072" },
            { "LJE", "073" },
            { "S3Y", "074" },
            { "SUS", "075" },
            { "BEL", "076" },
            { "1MA", "077" },
            { "2MA", "078" },
            { "3MA", "079" },
            { "4MA", "080" },
            { "1ES", "081" },
            { "2ES", "082" },
            { "MAN", "083" },
            { "PS2", "084" },
            { "ODA", "085" },
            { "PSS", "086" },
            { "JDA", "087" },
            { "JDB", "088" },
            { "TBS", "089" },
            { "SST", "090" },
            { "DNT", "091" },
            { "BLT", "092" },
            { "XXA", "093" },
            { "XXB", "094" },
            { "XXC", "095" },
            { "XXD", "096" },
            { "XXE", "097" },
            { "XXF", "098" },
            { "XXG", "099" },
            { "FRT", "100" },
            { "BAK", "101" },
            { "OTH", "102" },
            { "3ES", "103" },
            { "EZA", "104" },
            { "5EZ", "105" },
            { "6EZ", "106" },
            { "INT", "107" },
            { "CNC", "108" },
            { "GLO", "109" },
            { "TDX", "110" },
            { "NDX", "111" },
            { "DAG", "112" },
            { "PS3", "113" },
            { "2BA", "114" },
            { "LBA", "115" },
            { "JUB", "116" },
            { "ENO", "117" },
            { "1MQ", "118" },
            { "2MQ", "119" },
            { "3MQ", "120" },
            { "REP", "121" },

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
            { "Tobit", "067" },
            { "Judith", "068" },
            { "Esther Greek", "069" },
            { "Wisdom of Solomon", "070" },
            { "Sirach (Ecclesiasticus)", "071" },
            { "Baruch", "072" },
            { "Letter of Jeremiah", "073" },
            { "Song of 3 Young Men", "074" },
            { "Susanna", "075" },
            { "Bel and the Dragon", "076" },
            { "1 Maccabees", "077" },
            { "2 Maccabees", "078" },
            { "3 Maccabees", "079" },
            { "4 Maccabees", "080" },
            { "1 Esdras (Greek)", "081" },
            { "2 Esdras (Latin)", "082" },
            { "Prayer of Manasseh", "083" },
            { "Psalm 151", "084" },
            { "Odes", "085" },
            { "Psalms of Solomon", "086" },
            { "Joshua A. *obsolete*", "087" },
            { "Judges B. *obsolete*", "088" },
            { "Tobit S. *obsolete*", "089" },
            { "Susanna Th. *obsolete*", "090" },
            { "Daniel Th. *obsolete*", "091" },
            { "Bel Th. *obsolete*", "092" },
            { "Extra A", "093" },
            { "Extra B", "094" },
            { "Extra C", "095" },
            { "Extra D", "096" },
            { "Extra E", "097" },
            { "Extra F", "098" },
            { "Extra G", "099" },
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

            var bookNum = string.Empty;
            try
            {
                bookNum = lookup[value];
            }
            catch (Exception ex)
            {
              // TODO: how to report
              Console.WriteLine(ex.Message);
              bookNum = "";

            }

            return bookNum;
        }

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
            if (lookup.ContainsKey(BBBCCCVVV.Substring(0, 3)))
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
            catch (FormatException)
            {
                verseStr += " 00:";
            }

            // parse out the chapter
            try
            {
                int numVal = Int32.Parse(BBBCCCVVV.Substring(6, 3));
                verseStr += $"{numVal}";
            }
            catch (FormatException)
            {
                verseStr += "00";
            }

            return verseStr;

        }

        public static string GetVerseStrLongFromBBBCCCVVV(string BBBCCCVVV)
        {
            string verseStr = "";

            // ensure that we are dealing with a full 9 character string
            if (BBBCCCVVV.Length < 9)
            {
                BBBCCCVVV = BBBCCCVVV.PadLeft(9, '0');
            }

            // get the book lookup values
            var lookup = GetFullBookNameFromBookNum();

            // get the short book name
            if (lookup.ContainsKey(BBBCCCVVV.Substring(0, 3)))
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
            catch (FormatException)
            {
                verseStr += " 00:";
            }

            // parse out the chapter
            try
            {
                int numVal = Int32.Parse(BBBCCCVVV.Substring(6, 3));
                verseStr += $"{numVal}";
            }
            catch (FormatException)
            {
                verseStr += "00";
            }

            return verseStr;

        }
    }
}