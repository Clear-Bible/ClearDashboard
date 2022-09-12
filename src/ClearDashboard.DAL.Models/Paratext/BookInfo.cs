namespace ClearDashboard.DataAccessLayer.Models
{
    public class BookInfo 
    {
        public string? Code { get; set; }

        public bool InProjectScope { get; set; }

        public int Number { get; set; }


        public List<BookInfo> GenerateScriptureBookList()
        {
            var books = new List<BookInfo>();
            books.Add(new BookInfo{Code = "GEN", InProjectScope = true, Number = 1 });
            books.Add(new BookInfo{Code = "EXO", InProjectScope = true, Number = 2 });
            books.Add(new BookInfo{Code = "LEV", InProjectScope = true, Number = 3 });
            books.Add(new BookInfo{Code = "NUM", InProjectScope = true, Number = 4 });
            books.Add(new BookInfo{Code = "DEU", InProjectScope = true, Number = 5 });
            books.Add(new BookInfo{Code = "JOS", InProjectScope = true, Number = 6 });
            books.Add(new BookInfo{Code = "JDG", InProjectScope = true, Number = 7 });
            books.Add(new BookInfo{Code = "RUT", InProjectScope = true, Number = 8 });
            books.Add(new BookInfo{Code = "1SA", InProjectScope = true, Number = 9 });
            books.Add(new BookInfo{Code = "2SA", InProjectScope = true, Number = 10 });
            books.Add(new BookInfo{Code = "1KI", InProjectScope = true, Number = 11 });
            books.Add(new BookInfo{Code = "2KI", InProjectScope = true, Number = 12 });
            books.Add(new BookInfo{Code = "1CH", InProjectScope = true, Number = 13 });
            books.Add(new BookInfo{Code = "2CH", InProjectScope = true, Number = 14 });
            books.Add(new BookInfo{Code = "EZR", InProjectScope = true, Number = 15 });
            books.Add(new BookInfo{Code = "NEH", InProjectScope = true, Number = 16 });
            books.Add(new BookInfo{Code = "EST", InProjectScope = true, Number = 17 });
            books.Add(new BookInfo{Code = "JOB", InProjectScope = true, Number = 18 });
            books.Add(new BookInfo{Code = "PSA", InProjectScope = true, Number = 19 });
            books.Add(new BookInfo{Code = "PRO", InProjectScope = true, Number = 20 });
            books.Add(new BookInfo{Code = "ECC", InProjectScope = true, Number = 21 });
            books.Add(new BookInfo{Code = "SNG", InProjectScope = true, Number = 22 });
            books.Add(new BookInfo{Code = "ISA", InProjectScope = true, Number = 23 });
            books.Add(new BookInfo{Code = "JER", InProjectScope = true, Number = 24 });
            books.Add(new BookInfo{Code = "LAM", InProjectScope = true, Number = 25 });
            books.Add(new BookInfo{Code = "EZK", InProjectScope = true, Number = 26 });
            books.Add(new BookInfo{Code = "DAN", InProjectScope = true, Number = 27 });
            books.Add(new BookInfo{Code = "HOS", InProjectScope = true, Number = 28 });
            books.Add(new BookInfo{Code = "JOL", InProjectScope = true, Number = 29 });
            books.Add(new BookInfo{Code = "AMO", InProjectScope = true, Number = 30 });
            books.Add(new BookInfo{Code = "OBA", InProjectScope = true, Number = 31 });
            books.Add(new BookInfo{Code = "JON", InProjectScope = true, Number = 32 });
            books.Add(new BookInfo{Code = "MIC", InProjectScope = true, Number = 33 });
            books.Add(new BookInfo{Code = "NAM", InProjectScope = true, Number = 34 });
            books.Add(new BookInfo{Code = "HAB", InProjectScope = true, Number = 35 });
            books.Add(new BookInfo{Code = "ZEP", InProjectScope = true, Number = 36 });
            books.Add(new BookInfo{Code = "HAG", InProjectScope = true, Number = 37 });
            books.Add(new BookInfo{Code = "ZEC", InProjectScope = true, Number = 38 });
            books.Add(new BookInfo{Code = "MAL", InProjectScope = true, Number = 39 });
            books.Add(new BookInfo{Code = "MAT", InProjectScope = true, Number = 40 });
            books.Add(new BookInfo{Code = "MRK", InProjectScope = true, Number = 41 });
            books.Add(new BookInfo{Code = "LUK", InProjectScope = true, Number = 42 });
            books.Add(new BookInfo{Code = "JHN", InProjectScope = true, Number = 43 });
            books.Add(new BookInfo{Code = "ACT", InProjectScope = true, Number = 44 });
            books.Add(new BookInfo{Code = "ROM", InProjectScope = true, Number = 45 });
            books.Add(new BookInfo{Code = "1CO", InProjectScope = true, Number = 46 });
            books.Add(new BookInfo{Code = "2CO", InProjectScope = true, Number = 47 });
            books.Add(new BookInfo{Code = "GAL", InProjectScope = true, Number = 48 });
            books.Add(new BookInfo{Code = "EPH", InProjectScope = true, Number = 49 });
            books.Add(new BookInfo{Code = "PHP", InProjectScope = true, Number = 50 });
            books.Add(new BookInfo{Code = "COL", InProjectScope = true, Number = 51 });
            books.Add(new BookInfo{Code = "1TH", InProjectScope = true, Number = 52 });
            books.Add(new BookInfo{Code = "2TH", InProjectScope = true, Number = 53 });
            books.Add(new BookInfo{Code = "1TI", InProjectScope = true, Number = 54 });
            books.Add(new BookInfo{Code = "2TI", InProjectScope = true, Number = 55 });
            books.Add(new BookInfo{Code = "TIT", InProjectScope = true, Number = 56 });
            books.Add(new BookInfo{Code = "PHM", InProjectScope = true, Number = 57 });
            books.Add(new BookInfo{Code = "HEB", InProjectScope = true, Number = 58 });
            books.Add(new BookInfo{Code = "JAS", InProjectScope = true, Number = 59 });
            books.Add(new BookInfo{Code = "1PE", InProjectScope = true, Number = 60 });
            books.Add(new BookInfo{Code = "2PE", InProjectScope = true, Number = 61 });
            books.Add(new BookInfo{Code = "1JN", InProjectScope = true, Number = 62 });
            books.Add(new BookInfo{Code = "2JN", InProjectScope = true, Number = 63 });
            books.Add(new BookInfo{Code = "3JN", InProjectScope = true, Number = 64 });
            books.Add(new BookInfo{Code = "JUD", InProjectScope = true, Number = 65 });
            books.Add(new BookInfo{Code = "REV", InProjectScope = true, Number = 66 });

            return books;
        }
    }
}
