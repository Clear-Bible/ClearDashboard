using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class SelectedBook 
    {
        public enum EBookColor : int
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

        public bool IsSelected { get; set; }

        public bool IsEnabled { get; set; }

       
        public Color BackColor { get; set; }


        
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
                        BookColor = Color.Cyan;
                        break;
                    case EBookColor.Historical:
                        BookColor = Color.Coral;
                        break;
                    case EBookColor.Wisdom:
                        BookColor = Color.LimeGreen;
                        break;
                    case EBookColor.Prophets:
                        BookColor = Color.Magenta;
                        break;

                    // NT
                    case EBookColor.Gospels:
                        BookColor = Color.MediumPurple;
                        break;
                    case EBookColor.Acts:
                        BookColor = Color.Gold;
                        break;
                    case EBookColor.Epistles:
                        BookColor = Color.MediumSpringGreen;
                        break;
                    case EBookColor.Revelation:
                        BookColor = Color.Turquoise;
                        break;

                }
             
            }
        }

     
        public Color BookColor { get; set; }

        public string? BookNum { get; set; }

        public bool IsOt { get; set; }
     

        public static List<SelectedBook> Init()
        {
            List<SelectedBook> sb = new List<SelectedBook>();

            sb.Add(new SelectedBook
            {
                BookNum = "01",
                BookName = "Genesis",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "02",
                BookName = "Exodus",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "03",
                BookName = "Leviticus",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "04",
                BookName = "Numbers",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "05",
                BookName = "Deuteronomy",
                ColorText = EBookColor.Pentateuch,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "06",
                BookName = "Joshua",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "07",
                BookName = "Judges",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "08",
                BookName = "Ruth",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "09",
                BookName = "1 Samuel(1 Kings)",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "10",
                BookName = "2 Samuel(2 Kings)",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "11",
                BookName = "1 Kings(3 Kings)",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "12",
                BookName = "2 Kings(4 Kings)",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "13",
                BookName = "1 Chronicles",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "14",
                BookName = "2 Chronicles",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "15",
                BookName = "Ezra",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "16",
                BookName = "Nehemiah",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "17",
                BookName = "Esther",
                ColorText = EBookColor.Historical,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "18",
                BookName = "Job",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "19",
                BookName = "Psalms",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "20",
                BookName = "Proverbs",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "21",
                BookName = "Ecclesiastes",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "22",
                BookName = "Song of Solomon",
                ColorText = EBookColor.Wisdom,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "23",
                BookName = "Isaiah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "24",
                BookName = "Jeremiah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "25",
                BookName = "Lamentations",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "26",
                BookName = "Ezekiel",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "27",
                BookName = "Daniel",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "28",
                BookName = "Hosea",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "29",
                BookName = "Joel",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "30",
                BookName = "Amos",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "31",
                BookName = "Obadiah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "32",
                BookName = "Jonah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "33",
                BookName = "Micah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "34",
                BookName = "Nahum",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "35",
                BookName = "Habakkuk",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "36",
                BookName = "Zephaniah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "37",
                BookName = "Haggai",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "38",
                BookName = "Zechariah",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "39",
                BookName = "Malachi",
                ColorText = EBookColor.Prophets,
                IsOt = true,
                IsEnabled = false,
                IsSelected = true,
            });


            sb.Add(new SelectedBook
            {
                BookNum = "40",
                BookName = "Matthew",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "41",
                BookName = "Mark",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "42",
                BookName = "Luke",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "43",
                BookName = "John",
                ColorText = EBookColor.Gospels,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "44",
                BookName = "Acts",
                ColorText = EBookColor.Acts,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "45",
                BookName = "Romans",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "46",
                BookName = "1 Corinthians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "47",
                BookName = "2 Corinthians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "48",
                BookName = "Galatians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "49",
                BookName = "Ephesians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "50",
                BookName = "Philippians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "51",
                BookName = "Colossians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "52",
                BookName = "1 Thessalonians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "53",
                BookName = "2 Thessalonians",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "54",
                BookName = "1 Timothy",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "55",
                BookName = "2 Timothy",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "56",
                BookName = "Titus",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "57",
                BookName = "Philemon",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "58",
                BookName = "Hebrews",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "59",
                BookName = "James",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "60",
                BookName = "1 Peter",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "61",
                BookName = "2 Peter",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "62",
                BookName = "1 John",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "63",
                BookName = "2 John",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "64",
                BookName = "3 John",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "65",
                BookName = "Jude",
                ColorText = EBookColor.Epistles,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });
            sb.Add(new SelectedBook
            {
                BookNum = "66",
                BookName = "Revelation",
                ColorText = EBookColor.Revelation,
                IsOt = false,
                IsEnabled = false,
                IsSelected = true,
            });

            return sb;
        }
    }
}
