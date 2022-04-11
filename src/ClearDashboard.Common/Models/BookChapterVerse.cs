using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClearDashboard.Common.Models
{
    /// <summary>
    /// Clear.Bible verse location information and conversion.
    /// </summary>
    public class BookChapterVerse : INotifyPropertyChanged
    {
        private List<string> _bibleBookList;

        public List<string> BibleBookList
        {
            get { return _bibleBookList; }
            set
            {
                _bibleBookList = value;
                OnPropertyChanged(nameof(BibleBookList));
            }
        }

        private List<int> _chapterNumbers = new();
        public List<int> ChapterNumbers
        {
            get => _chapterNumbers;
            set
            {
                _chapterNumbers = value;
                OnPropertyChanged(nameof(ChapterNumbers));
            }
        }

        private List<int> _verseNumbers = new();
        public List<int> VerseNumbers
        {
            get => _verseNumbers;
            set
            {
                _verseNumbers = value;
                OnPropertyChanged(nameof(VerseNumbers));
            }
        }

        private Int32 _bookNum { get; set; } = 1;
        /// <summary>
        /// The Book ID number as an int.
        /// </summary>
        public Int32 BookNum
        {
            get => _bookNum;
            set
            {
                if (_bookNum != value)
                {
                    _bookNum = value;
                    _bookName = BookId2BookName(value);
                    OnPropertyChanged(nameof(BookNum));
                    OnPropertyChanged(nameof(BookName));
                    OnPropertyChanged(nameof(VerseLocationId));
                }
            }
        }

        /// <summary>
        /// The Book ID number as a padded string. Automatically calculated from BookNum.
        /// </summary>
        public string Book => BookNum.ToString().PadLeft(2, '0');

        private string _bookName { get; set; } = "Genesis";
        /// <summary>
        /// The book name. Defaults to Genesis
        /// </summary>
        public string BookName
        {
            get => _bookName;
            set
            {
                if (_bookName != value) // To avoid a loop.
                {
                    _bookName = value;
                    _bookNum = GetIntBookNumFromBookName(value);
                    OnPropertyChanged(nameof(BookNum));
                    OnPropertyChanged(nameof(BookName));
                    OnPropertyChanged(nameof(VerseLocationId));
                }
            }
        }

        private int? _chapter { get; set; } = 1;
        /// <summary>
        /// Chapter number as an int.
        /// </summary>
        public int? Chapter
        {
            get => _chapter;
            set
            {
                if (_chapter != value)
                {
                    if (value == 0)
                    {
                        value = 1;
                    }

                    _chapter = value;
                    OnPropertyChanged(nameof(Chapter));
                    OnPropertyChanged(nameof(VerseLocationId));
                }
            }
        }

        /// <summary>
        /// Chapter number as padded text. Automatically calculated from Chapter.
        /// </summary>
        public string ChapterIdText => Chapter.ToString().PadLeft(3, '0');

        private int? _verse { get; set; } = 1;
        /// <summary>
        /// The verse number as an int.
        /// </summary>
        public int? Verse
        {
            get => _verse;
            set
            {
                if (_verse != value)
                {
                    if (value == 0)
                    {
                        value = 1;
                    }

                    _verse = value;
                    OnPropertyChanged(nameof(Verse));
                    OnPropertyChanged(nameof(VerseLocationId));
                }
            }
        }

        /// <summary>
        /// Verse ID as a string. Automatically calculated from Verse.
        /// </summary>
        public string VerseIdText => Verse.ToString().PadLeft(3, '0');

        /// <summary>
        /// A string with the Clear.Bible format of verse location ID. It is automatically calculated. 
        /// </summary>
        public string VerseLocationId => string.Concat(Book, ChapterIdText, VerseIdText);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
            // Convert the number into a string we can parse.
            string _VerseLocationId = verseId.ToString().PadLeft(8, '0');
            string _BookNumStr = _VerseLocationId.Substring(0, 2);
            // Test each parse, and only return a TRUE if they all are parsed.
            if (Int32.TryParse(_BookNumStr, out Int32 bookNum))
            {
                // The book number for use in the array used in the pull down list.
                BookNum = bookNum;
            }
            else
            {
                return false;
            }

            string _ChapterIdText = _VerseLocationId.Substring(2, 3);
            if (Int32.TryParse(_ChapterIdText, out Int32 chapterNum))
            {
                Chapter = chapterNum;
            }
            else
            {
                return false;
            }

            string _VerseIdText = _VerseLocationId.Substring(5, 3);
            if (Int32.TryParse(_VerseIdText, out Int32 verseNum))
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
        /// <param name="IncommingVerseLocation">Paratext verse ID</param>
        /// <returns>Success of parsing IncommingVerseLocation.</returns>
        public bool VerseLocationConverter(int IncommingVerseLocation)
        {
            return this.SetVerseFromId(IncommingVerseLocation.ToString());
        }


        /// <summary>
        /// Sets the Bible BookName of this object.
        /// </summary>
        /// <param name="bookNum">The number you want set.</param>
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
            List<string> _bibleBooks = new List<string>();
            _bibleBooks.Add(String.Empty); // Offset by one.
            _bibleBooks.Add("Genesis");
            _bibleBooks.Add("Exodus");
            _bibleBooks.Add("Leviticus");
            _bibleBooks.Add("Numbers");
            _bibleBooks.Add("Deuteronomy");
            _bibleBooks.Add("Joshua");
            _bibleBooks.Add("Judges");
            _bibleBooks.Add("Ruth");
            _bibleBooks.Add("1 Samuel (1 Kings)");
            _bibleBooks.Add("2 Samuel (2 Kings)");
            _bibleBooks.Add("1 Kings (3 Kings)");
            _bibleBooks.Add("2 Kings (4 Kings)");
            _bibleBooks.Add("1 Chronicles");
            _bibleBooks.Add("2 Chronicles");
            _bibleBooks.Add("Ezra");
            _bibleBooks.Add("Nehemiah");
            _bibleBooks.Add("Esther");
            _bibleBooks.Add("Job");
            _bibleBooks.Add("Psalms");
            _bibleBooks.Add("Proverbs");
            _bibleBooks.Add("Ecclesiastes");
            _bibleBooks.Add("Song of Songs");
            _bibleBooks.Add("Isaiah");
            _bibleBooks.Add("Jeremiah");
            _bibleBooks.Add("Lamentations");
            _bibleBooks.Add("Ezekiel");
            _bibleBooks.Add("Daniel");
            _bibleBooks.Add("Hosea");
            _bibleBooks.Add("Joel");
            _bibleBooks.Add("Amos");
            _bibleBooks.Add("Obadiah");
            _bibleBooks.Add("Jonah");
            _bibleBooks.Add("Micah");
            _bibleBooks.Add("Nahum");
            _bibleBooks.Add("Habakkuk");
            _bibleBooks.Add("Zephaniah");
            _bibleBooks.Add("Haggai");
            _bibleBooks.Add("Zechariah");
            _bibleBooks.Add("Malachi");
            _bibleBooks.Add("Matthew");
            _bibleBooks.Add("Mark");
            _bibleBooks.Add("Luke");
            _bibleBooks.Add("John");
            _bibleBooks.Add("Acts");
            _bibleBooks.Add("Romans");
            _bibleBooks.Add("1 Corinthians");
            _bibleBooks.Add("2 Corinthians");
            _bibleBooks.Add("Galatians");
            _bibleBooks.Add("Ephesians");
            _bibleBooks.Add("Philippians");
            _bibleBooks.Add("Colossians");
            _bibleBooks.Add("1 Thessalonians");
            _bibleBooks.Add("2 Thessalonians");
            _bibleBooks.Add("1 Timothy");
            _bibleBooks.Add("2 Timothy");
            _bibleBooks.Add("Titus");
            _bibleBooks.Add("Philemon");
            _bibleBooks.Add("Hebrews");
            _bibleBooks.Add("James");
            _bibleBooks.Add("1 Peter");
            _bibleBooks.Add("2 Peter");
            _bibleBooks.Add("1 John");
            _bibleBooks.Add("2 John");
            _bibleBooks.Add("3 John");
            _bibleBooks.Add("Jude");
            _bibleBooks.Add("Revelation");

            if (isParatext)
            {
                // Add a blank book between the OT and NT to match the Paratext format.
                _bibleBooks.Insert(40, String.Empty);
            }

            return _bibleBooks;
        }

        /// <summary>
        /// Given a Bible book number, it returns an English book name.
        /// </summary>
        /// <param name="bookNum">Bible book number as an int.</param>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>An English Bible book name.</returns>
        public static string BookId2BookName(int bookNum, bool isParatext = false)
        {
            if (isParatext)
            {
                List<string> _bibleBooks = BibleBookListEn(isParatext);

                // If the number passed is greater than the number of books.
                if (_bibleBooks.Count > bookNum)
                {
                    // Grab a book by it's index in the list.
                    return _bibleBooks[bookNum];
                }

                return string.Empty;
            }

            return GetFullBookNameFromBookNum(bookNum.ToString().PadLeft(2, '0'));
        }

        /// <summary>
        /// Returns a Bible book number when given an English Bible book name.
        /// </summary>
        /// <param name="bookName">The English name of the book you are looking for. A partial name or acronym works too.</param>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>Returns a Bible book number as an int.</returns>
        public int GetIntBookNumFromBookName(string bookName, bool isParatext = false)
        {
            if (isParatext)
            {
                List<string> _bibleBooks = BibleBookListEn(isParatext);
                if (_bibleBooks.Contains(bookName))
                {
                    return _bibleBooks.IndexOf(bookName);
                }

                // A direct match was not found. Try a partial match.
                string foundIt = _bibleBooks.Find(f => f.Contains(bookName));
                if (!string.IsNullOrEmpty(foundIt))
                {
                    BookName = foundIt; // This triggers another lookup, but this time it should be found.
                }

                _bibleBooks.ForEach(book =>
                {
                    if (bookName.StartsWith(book))
                    {
                        BookName = book; // This triggers another lookup, but this time it should be found.
                    }
                });

                return 0;
            }

            if (bookName.Contains("("))
            {
                bookName = bookName.Remove(bookName.IndexOf('(')).Trim();
            }

            return int.Parse(GetBookNumFromBookName(bookName));
        }


        /// <summary>
        /// A list of acronyms of the books of the Bible in English.
        /// </summary>
        /// <param name="isParatext">If true, it adds a blank book name between the OT and NT to match the format Paratext uses.</param>
        /// <returns>A list of acronyms of the books of the Bible in English.</returns>
        public static List<string> BibleBookAcronymListEn(bool isParatext = false)
        {
            List<string> _bibleBooks = new List<string>();
            _bibleBooks.Add(String.Empty); // Offset by one.
            _bibleBooks.Add("GEN");
            _bibleBooks.Add("EXO");
            _bibleBooks.Add("LEV");
            _bibleBooks.Add("NUM");
            _bibleBooks.Add("DEU");
            _bibleBooks.Add("JOS");
            _bibleBooks.Add("JDG");
            _bibleBooks.Add("RUT");
            _bibleBooks.Add("1SA");
            _bibleBooks.Add("2SA");
            _bibleBooks.Add("1KI");
            _bibleBooks.Add("2KI");
            _bibleBooks.Add("1CH");
            _bibleBooks.Add("2CH");
            _bibleBooks.Add("EZR");
            _bibleBooks.Add("NEH");
            _bibleBooks.Add("EST");
            _bibleBooks.Add("JOB");
            _bibleBooks.Add("PSA");
            _bibleBooks.Add("PRO");
            _bibleBooks.Add("ECC");
            _bibleBooks.Add("SNG");
            _bibleBooks.Add("ISA");
            _bibleBooks.Add("JER");
            _bibleBooks.Add("LAM");
            _bibleBooks.Add("EZK");
            _bibleBooks.Add("DAN");
            _bibleBooks.Add("HOS");
            _bibleBooks.Add("JOL");
            _bibleBooks.Add("AMO");
            _bibleBooks.Add("OBA");
            _bibleBooks.Add("JON");
            _bibleBooks.Add("MIC");
            _bibleBooks.Add("NAM");
            _bibleBooks.Add("HAB");
            _bibleBooks.Add("ZEP");
            _bibleBooks.Add("HAG");
            _bibleBooks.Add("ZEC");
            _bibleBooks.Add("MAL");
            _bibleBooks.Add("MAT");
            _bibleBooks.Add("MRK");
            _bibleBooks.Add("LUK");
            _bibleBooks.Add("JHN");
            _bibleBooks.Add("ACT");
            _bibleBooks.Add("ROM");
            _bibleBooks.Add("1CO");
            _bibleBooks.Add("2CO");
            _bibleBooks.Add("GAL");
            _bibleBooks.Add("EPH");
            _bibleBooks.Add("PHP");
            _bibleBooks.Add("COL");
            _bibleBooks.Add("1TH");
            _bibleBooks.Add("2TH");
            _bibleBooks.Add("1TI");
            _bibleBooks.Add("2TI");
            _bibleBooks.Add("TIT");
            _bibleBooks.Add("PHM");
            _bibleBooks.Add("HEB");
            _bibleBooks.Add("JAS");
            _bibleBooks.Add("1PE");
            _bibleBooks.Add("2PE");
            _bibleBooks.Add("1JN");
            _bibleBooks.Add("2JN");
            _bibleBooks.Add("3JN");
            _bibleBooks.Add("JUD");
            _bibleBooks.Add("REV");

            if (isParatext)
            {
                // Add a blank book between the OT and NT to match the Paratext format.
                _bibleBooks.Insert(40, string.Empty);
            }

            return _bibleBooks;
        }


        public static string GetFullBookNameFromBookNum(string value)
        {
            Dictionary<string, string> lookup = new Dictionary<string, string>
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

    }
}
