using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Models
{
    public class SelectedBook
    {
        public enum EBookColor
        {
            Pentateuch = 0,
            Historical = 1,
            Wisdom = 2,
            Prophets = 3,
            Gospels = 5,
            Acts = 6,
            Epistles = 7,
            Revelation = 8,

        }

        public string? BookName { get; set; }

        public FontWeight FontWeight { get; set; }

        public bool IsImported { get; set; }

        public bool IsSelected { get; set; }

        public bool IsEnabled { get; set; }

        public bool HasUsfmError { get; set; }

        public string Abbreviation { get; set; }

        public Brush BackColor { get; set; }



        public float ProgressNum { get; set; }


        private EBookColor _colorText;
        public EBookColor ColorText
        {
            get => _colorText;
            set
            {
                _colorText = value;

                switch (value)
                {
                    // OT
                    case EBookColor.Pentateuch:
                        BookColor = Brushes.Cyan;
                        break;
                    case EBookColor.Historical:
                        BookColor = Brushes.Coral;
                        break;
                    case EBookColor.Wisdom:
                        BookColor = Brushes.LimeGreen;
                        break;
                    case EBookColor.Prophets:
                        BookColor = Brushes.Magenta;
                        break;

                    // NT
                    case EBookColor.Gospels:
                        BookColor = Brushes.MediumPurple;
                        break;
                    case EBookColor.Acts:
                        BookColor = Brushes.Gold;
                        break;
                    case EBookColor.Epistles:
                        BookColor = Brushes.MediumSpringGreen;
                        break;
                    case EBookColor.Revelation:
                        BookColor = Brushes.Turquoise;
                        break;

                }

            }
        }


        public SolidColorBrush? BookColor { get; set; }

        public string? BookNum { get; set; }

        public bool IsOt { get; set; }


        public static List<SelectedBook> Init()
        {
            List<SelectedBook> sb = new List<SelectedBook>();

            sb.Add(new SelectedBook
            {
                BookNum = "01",
                BookName = "Genesis",
                Abbreviation = "GEN",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "02",
                BookName = "Exodus",
                Abbreviation = "EXO",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "03",
                BookName = "Leviticus",
                Abbreviation = "LEV",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "04",
                BookName = "Numbers",
                Abbreviation = "NUM",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "05",
                BookName = "Deuteronomy",
                Abbreviation = "DEU",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "06",
                BookName = "Joshua",
                Abbreviation = "JOS",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "07",
                BookName = "Judges",
                Abbreviation = "JDG",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "08",
                BookName = "Ruth",
                Abbreviation = "RUT",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "09",
                BookName = "1 Samuel(1 Kings)",
                Abbreviation = "1SA",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "10",
                BookName = "2 Samuel(2 Kings)",
                Abbreviation = "2SA",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "11",
                BookName = "1 Kings(3 Kings)",
                Abbreviation = "1KI",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "12",
                BookName = "2 Kings(4 Kings)",
                Abbreviation = "2KI",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "13",
                BookName = "1 Chronicles",
                Abbreviation = "1CH",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "14",
                BookName = "2 Chronicles",
                Abbreviation = "2CH",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "15",
                BookName = "Ezra",
                Abbreviation = "EZR",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "16",
                BookName = "Nehemiah",
                Abbreviation = "NEH",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "17",
                BookName = "Esther",
                Abbreviation = "EST",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "18",
                BookName = "Job",
                Abbreviation = "JOB",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "19",
                BookName = "Psalms",
                Abbreviation = "PSA",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "20",
                BookName = "Proverbs",
                Abbreviation = "PRO",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "21",
                BookName = "Ecclesiastes",
                Abbreviation = "ECC",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "22",
                BookName = "Song of Solomon",
                Abbreviation = "SNG",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "23",
                BookName = "Isaiah",
                Abbreviation = "ISA",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "24",
                BookName = "Jeremiah",
                Abbreviation = "JER",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "25",
                BookName = "Lamentations",
                Abbreviation = "LAM",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "26",
                BookName = "Ezekiel",
                Abbreviation = "EZK",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "27",
                BookName = "Daniel",
                Abbreviation = "DAN",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "28",
                BookName = "Hosea",
                Abbreviation = "HOS",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "29",
                BookName = "Joel",
                Abbreviation = "JOL",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "30",
                BookName = "Amos",
                Abbreviation = "AMO",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "31",
                BookName = "Obadiah",
                Abbreviation = "OBA",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "32",
                BookName = "Jonah",
                Abbreviation = "JON",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "33",
                BookName = "Micah",
                Abbreviation = "MIC",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "34",
                BookName = "Nahum",
                Abbreviation = "NAM",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "35",
                BookName = "Habakkuk",
                Abbreviation = "HAB",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "36",
                BookName = "Zephaniah",
                Abbreviation = "ZEP",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "37",
                BookName = "Haggai",
                Abbreviation = "HAG",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "38",
                BookName = "Zechariah",
                Abbreviation = "ZEC",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "39",
                BookName = "Malachi",
                Abbreviation = "MAL",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });


            sb.Add(new SelectedBook
            {
                BookNum = "40",
                BookName = "Matthew",
                Abbreviation = "MAT",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "41",
                BookName = "Mark",
                Abbreviation = "MRK",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "42",
                BookName = "Luke",
                Abbreviation = "LUK",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "43",
                BookName = "John",
                Abbreviation = "JHN",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "44",
                BookName = "Acts",
                Abbreviation = "ACT",
                ColorText = EBookColor.Acts,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "45",
                BookName = "Romans",
                Abbreviation = "ROM",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "46",
                BookName = "1 Corinthians",
                Abbreviation = "1CO",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "47",
                BookName = "2 Corinthians",
                Abbreviation = "2CO",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "48",
                BookName = "Galatians",
                Abbreviation = "GAL",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "49",
                BookName = "Ephesians",
                Abbreviation = "EPH",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "50",
                BookName = "Philippians",
                Abbreviation = "PHP",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "51",
                BookName = "Colossians",
                Abbreviation = "COL",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "52",
                BookName = "1 Thessalonians",
                Abbreviation = "1TH",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "53",
                BookName = "2 Thessalonians",
                Abbreviation = "2TH",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "54",
                BookName = "1 Timothy",
                Abbreviation = "1TI",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "55",
                BookName = "2 Timothy",
                Abbreviation = "2TI",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "56",
                BookName = "Titus",
                Abbreviation = "TIT",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "57",
                BookName = "Philemon",
                Abbreviation = "PHM",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "58",
                BookName = "Hebrews",
                Abbreviation = "HEB",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "59",
                BookName = "James",
                Abbreviation = "JAS",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "60",
                BookName = "1 Peter",
                Abbreviation = "1PE",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "61",
                BookName = "2 Peter",
                Abbreviation = "2PE",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "62",
                BookName = "1 John",
                Abbreviation = "1JN",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "63",
                BookName = "2 John",
                Abbreviation = "2JN",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "64",
                BookName = "3 John",
                Abbreviation = "3JN",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "65",
                BookName = "Jude",
                Abbreviation = "JUD",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "66",
                BookName = "Revelation",
                Abbreviation = "REV",
                ColorText = EBookColor.Revelation,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });

            return sb;
        }
    }
}
