namespace ClearDashboard.DataAccessLayer.Models
{
    public class BookInfo 
    {
        public string? Code { get; set; }

        public bool InProjectScope { get; set; }

        public int Number { get; set; }


        public static List<BookInfo> GenerateScriptureBookList()
        {
            var books = new List<BookInfo>
            {
                new() { Code = "GEN", InProjectScope = true, Number = 1 },
                new () { Code = "EXO", InProjectScope = true, Number = 2 },
                new (){Code = "LEV", InProjectScope = true, Number = 3 },
                new (){Code = "NUM", InProjectScope = true, Number = 4 },
                new (){Code = "DEU", InProjectScope = true, Number = 5 },
                new (){Code = "JOS", InProjectScope = true, Number = 6 },
                new (){Code = "JDG", InProjectScope = true, Number = 7 },
                new (){Code = "RUT", InProjectScope = true, Number = 8 },
                new (){Code = "1SA", InProjectScope = true, Number = 9 },
                new (){Code = "2SA", InProjectScope = true, Number = 10 },
                new (){Code = "1KI", InProjectScope = true, Number = 11 },
                new (){Code = "2KI", InProjectScope = true, Number = 12 },
                new (){Code = "1CH", InProjectScope = true, Number = 13 },
                new (){Code = "2CH", InProjectScope = true, Number = 14 },
                new (){Code = "EZR", InProjectScope = true, Number = 15 },
                new (){Code = "NEH", InProjectScope = true, Number = 16 },
                new (){Code = "EST", InProjectScope = true, Number = 17 },
                new (){Code = "JOB", InProjectScope = true, Number = 18 },
                new (){Code = "PSA", InProjectScope = true, Number = 19 },
                new (){Code = "PRO", InProjectScope = true, Number = 20 },
                new (){Code = "ECC", InProjectScope = true, Number = 21 },
                new (){Code = "SNG", InProjectScope = true, Number = 22 },
                new (){Code = "ISA", InProjectScope = true, Number = 23 },
                new (){Code = "JER", InProjectScope = true, Number = 24 },
                new (){Code = "LAM", InProjectScope = true, Number = 25 },
                new (){Code = "EZK", InProjectScope = true, Number = 26 },
                new (){Code = "DAN", InProjectScope = true, Number = 27 },
                new (){Code = "HOS", InProjectScope = true, Number = 28 },
                new (){Code = "JOL", InProjectScope = true, Number = 29 },
                new (){Code = "AMO", InProjectScope = true, Number = 30 },
                new (){Code = "OBA", InProjectScope = true, Number = 31 },
                new (){Code = "JON", InProjectScope = true, Number = 32 },
                new (){Code = "MIC", InProjectScope = true, Number = 33 },
                new (){Code = "NAM", InProjectScope = true, Number = 34 },
                new (){Code = "HAB", InProjectScope = true, Number = 35 },
                new (){Code = "ZEP", InProjectScope = true, Number = 36 },
                new (){Code = "HAG", InProjectScope = true, Number = 37 },
                new (){Code = "ZEC", InProjectScope = true, Number = 38 },
                new (){Code = "MAL", InProjectScope = true, Number = 39 },
                new (){Code = "MAT", InProjectScope = true, Number = 40 },
                new (){Code = "MRK", InProjectScope = true, Number = 41 },
                new (){Code = "LUK", InProjectScope = true, Number = 42 },
                new (){Code = "JHN", InProjectScope = true, Number = 43 },
                new (){Code = "ACT", InProjectScope = true, Number = 44 },
                new (){Code = "ROM", InProjectScope = true, Number = 45 },
                new (){Code = "1CO", InProjectScope = true, Number = 46 },
                new (){Code = "2CO", InProjectScope = true, Number = 47 },
                new (){Code = "GAL", InProjectScope = true, Number = 48 },
                new (){Code = "EPH", InProjectScope = true, Number = 49 },
                new (){Code = "PHP", InProjectScope = true, Number = 50 },
                new (){Code = "COL", InProjectScope = true, Number = 51 },
                new (){Code = "1TH", InProjectScope = true, Number = 52 },
                new (){Code = "2TH", InProjectScope = true, Number = 53 },
                new (){Code = "1TI", InProjectScope = true, Number = 54 },
                new (){Code = "2TI", InProjectScope = true, Number = 55 },
                new (){Code = "TIT", InProjectScope = true, Number = 56 },
                new (){Code = "PHM", InProjectScope = true, Number = 57 },
                new (){Code = "HEB", InProjectScope = true, Number = 58 },
                new (){Code = "JAS", InProjectScope = true, Number = 59 },
                new (){Code = "1PE", InProjectScope = true, Number = 60 },
                new (){Code = "2PE", InProjectScope = true, Number = 61 },
                new (){Code = "1JN", InProjectScope = true, Number = 62 },
                new (){Code = "2JN", InProjectScope = true, Number = 63 },
                new (){Code = "3JN", InProjectScope = true, Number = 64 },
                new (){Code = "JUD", InProjectScope = true, Number = 65 },
                new (){Code = "REV", InProjectScope = true, Number = 66 }
            };

            return books;
        }
    }
}
