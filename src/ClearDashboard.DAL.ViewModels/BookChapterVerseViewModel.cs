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
        public string? Book => Entity?.BookNum.ToString().PadLeft(3, '0');

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
            var verseLocationId = verseId.ToString().PadLeft(9, '0');
            var bookNumStr = verseLocationId.Substring(0, 3);
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

            string chapterIdText = verseLocationId.Substring(3, 3);
            if (int.TryParse(chapterIdText, out Int32 chapterNum))
            {
                Chapter = chapterNum;
            }
            else
            {
                return false;
            }

            var verseIdText = verseLocationId.Substring(6, 3);
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
            var bookBB = bookNum.ToString().PadLeft(3, '0');
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
            { "Colossians", "51" },
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