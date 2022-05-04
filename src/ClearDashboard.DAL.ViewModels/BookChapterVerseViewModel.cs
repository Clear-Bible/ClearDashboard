using System.Reflection.Metadata;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Helpers;
using static System.String;

namespace ClearDashboard.DAL.ViewModels
{
    public class BookChapterVerseViewModel : ViewModelBase<BookChapterVerse>
    {
        public BookChapterVerseViewModel() : base()
        {

        }

        public BookChapterVerseViewModel(BookChapterVerse entity) : base(entity)
        {

        }


        public List<string>? BibleBookList
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

        public List<int>? ChapterNumbers
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

        public List<int>? VerseNumbers
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
        public string? Book => Entity?.BookNum.ToString().PadLeft(2, '0');

        public string? BookName
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
                    NotifyOfPropertyChange(nameof(VerseLocationId));
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
                    NotifyOfPropertyChange(nameof(VerseLocationId));
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
                NotifyOfPropertyChange(nameof(VerseLocationId));
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
        public string VerseLocationId => Concat(Book, ChapterIdText, VerseIdText);



        /// <summary>
        /// Based on the properties of this object, it returns the complete verse location ID. Modified function for compatibility.
        /// </summary>
        /// <returns>A string with the Clear.Bible format of verse location ID.</returns>
        public string GetVerseId()
        {
            return VerseLocationId;
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
            var verseLocationId = verseId.ToString().PadLeft(8, '0');
            var bookNumStr = verseLocationId.Substring(0, 2);
            // Test each parse, and only return a TRUE if they all are parsed.
            if (int.TryParse(bookNumStr, out var bookNum))
            {
                // The book number for use in the array used in the pull down list.
                BookNum = bookNum;
            }
            else
            {
                return false;
            }

            string chapterIdText = verseLocationId.Substring(2, 3);
            if (int.TryParse(chapterIdText, out Int32 chapterNum))
            {
                Chapter = chapterNum;
            }
            else
            {
                return false;
            }

            var verseIdText = verseLocationId.Substring(5, 3);
            if (int.TryParse(verseIdText, out int verseNum))
            {
                Verse = verseNum;

                if (verseNum == 0)
                {
                    Verse = 1;
                }
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
            var books = BibleRefUtils.GetBookIdDictionary();
            var bookBB = bookNum.ToString().PadLeft(2, '0');
            if (books.ContainsKey(bookBB))
            {
                return books[bookBB];
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
        public int GetIntBookNumFromBookName(string? bookName)
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


        public static string GetFullBookNameFromBookNum(string value)
        {
            var lookup = new Dictionary<string, string>
            {
                { "01", "Genesis"},
                { "02", "Exodus"},
                { "03", "Leviticus"},
                { "04", "Numbers"},
                { "05", "Deuteronomy"},
                { "06", "Joshua"},
                { "07", "Judges"},
                { "08", "Ruth"},
                { "09", "1 Samuel"},
                { "10", "2 Samuel"},
                { "11", "1 Kings"},
                { "12", "2 Kings"},
                { "13", "1 Chronicles"},
                { "14", "2 Chronicles"},
                { "15", "Ezra"},
                { "16", "Nehemiah"},
                { "17", "Esther"},
                { "18", "Job"},
                { "19", "Psalms"},
                { "20", "Proverbs"},
                { "21", "Ecclesiastes"},
                { "22", "Song of Songs"},
                { "23", "Isaiah"},
                { "24", "Jeremiah"},
                { "25", "Lamentations"},
                { "26", "Ezekiel"},
                { "27", "Daniel"},
                { "28", "Hosea"},
                { "29", "Joel"},
                { "30", "Amos"},
                { "31", "Obadiah"},
                { "32", "Jonah"},
                { "33", "Micah"},
                { "34", "Nahum"},
                { "35", "Habakkuk"},
                { "36", "Zephaniah"},
                { "37", "Haggai"},
                { "38", "Zechariah"},
                { "39", "Malachi"},
                { "40", "Matthew"},
                { "41", "Mark"},
                { "42", "Luke"},
                { "43", "John"},
                { "44", "Acts"},
                { "45", "Romans"},
                { "46", "1 Corinthians"},
                { "47", "2 Corinthians"},
                { "48", "Galatians"},
                { "49", "Ephesians"},
                { "50", "Philippians"},
                { "51", "Colossians"},
                { "52", "1 Thessalonians"},
                { "53", "2 Thessalonians"},
                { "54", "1 Timothy"},
                { "55", "2 Timothy"},
                { "56", "Titus"},
                { "57", "Philemon"},
                { "58", "Hebrews"},
                { "59", "James"},
                { "60", "1 Peter"},
                { "61", "2 Peter"},
                { "62", "1 John"},
                { "63", "2 John"},
                { "64", "3 John"},
                { "65", "Jude"},
                { "66", "Revelation"},

            };

            var bookName = string.Empty;
            try
            {
                bookName = lookup[value];
            }
            catch (Exception ex)
            {
                // no-op
                // TODO:  How to report?
            }

            return bookName;
        }

        public static string GetBookNumFromBookName(string? value)
        {
            var lookup = new Dictionary<string?, string>
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

            var bookNum = string.Empty;
            try
            {
                bookNum = lookup[value];
            }
            catch (Exception ex)
            {
              // TODO: how to report
               
            }

            return bookNum;
        }
    }
}