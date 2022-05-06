using SIL.Scripture;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class ParatextProject 
    {
        public Dictionary<int, ParatextBookFileName> BookNames = new Dictionary<int, ParatextBookFileName>
        {
            {01, new ParatextBookFileName{code="GEN", abbr = "Gen", shortname ="Genesis", longname ="Genesis", fileID="01", BB="01"}},
            {02, new ParatextBookFileName{code="EXO", abbr = "Exod", shortname ="Exodus", longname ="Exodus", fileID="02", BB="02"}},
            {03, new ParatextBookFileName{code="LEV", abbr = "Lev", shortname ="Leviticus", longname ="Leviticus", fileID="03", BB="03"}},
            {04, new ParatextBookFileName{code="NUM", abbr = "Num", shortname ="Numbers", longname ="Numbers", fileID="04", BB="04"}},
            {05, new ParatextBookFileName{code="DEU", abbr = "Deut", shortname ="Deuteronomy", longname ="Deuteronomy", fileID="05", BB="05"}},
            {06, new ParatextBookFileName{code="JOS", abbr = "Josh", shortname ="Joshua", longname ="Joshua", fileID="06", BB="06"}},
            {07, new ParatextBookFileName{code="JDG", abbr = "Judg", shortname ="Judges", longname ="Judges", fileID="07", BB="07"}},
            {08, new ParatextBookFileName{code="RUT", abbr = "Ruth", shortname ="Ruth", longname ="Ruth", fileID="08", BB="08"}},
            {09, new ParatextBookFileName{code="1SA", abbr = "1 Sam", shortname ="1 Samuel", longname ="1 Samuel", fileID="09", BB="09"}},
            {10, new ParatextBookFileName{code="2SA", abbr = "2 Sam", shortname ="2 Samuel", longname ="2 Samuel", fileID="10", BB="10"}},
            {11, new ParatextBookFileName{code="1KI", abbr = "1 Kgs", shortname ="1 Kings", longname ="1 Kings", fileID="11", BB="11"}},
            {12, new ParatextBookFileName{code="2KI", abbr = "2 Kgs", shortname ="2 Kings", longname ="2 Kings", fileID="12", BB="12"}},
            {13, new ParatextBookFileName{code="1CH", abbr = "1 Chr", shortname ="1 Chronicles", longname ="1 Chronicles", fileID="13", BB="13"}},
            {14, new ParatextBookFileName{code="2CH", abbr = "2 Chr", shortname ="2 Chronicles", longname ="2 Chronicles", fileID="14", BB="14"}},
            {15, new ParatextBookFileName{code="EZR", abbr = "Ezra", shortname ="Ezra", longname ="Ezra", fileID="15", BB="15"}},
            {16, new ParatextBookFileName{code="NEH", abbr = "Neh", shortname ="Nehemiah", longname ="Nehemiah", fileID="16", BB="16"}},
            {17, new ParatextBookFileName{code="EST", abbr = "Esth", shortname ="Esther", longname ="Esther", fileID="17", BB="17"}},
            {18, new ParatextBookFileName{code="JOB", abbr = "Job", shortname ="Job", longname ="Job", fileID="18", BB="18"}},
            {19, new ParatextBookFileName{code="PSA", abbr = "Ps(s)", shortname ="Psalms", longname ="Psalms", fileID="19", BB="19"}},
            {20, new ParatextBookFileName{code="PRO", abbr = "Prov", shortname ="Proverbs", longname ="Proverbs", fileID="20", BB="20"}},
            {21, new ParatextBookFileName{code="ECC", abbr = "Eccl", shortname ="Ecclesiastes", longname ="Ecclesiastes", fileID="21", BB="21"}},
            {22, new ParatextBookFileName{code="SNG", abbr = "Song", shortname ="Song of Songs", longname ="The Song of Songs", fileID="22", BB="22"}},
            {23, new ParatextBookFileName{code="ISA", abbr = "Isa", shortname ="Isaiah", longname ="Isaiah", fileID="23", BB="23"}},
            {24, new ParatextBookFileName{code="JER", abbr = "Jer", shortname ="Jeremiah", longname ="Jeremiah", fileID="24", BB="24"}},
            {25, new ParatextBookFileName{code="LAM", abbr = "Lam", shortname ="Lamentations", longname ="Lamentations", fileID="25", BB="25"}},
            {26, new ParatextBookFileName{code="EZK", abbr = "Ezek", shortname ="Ezekiel", longname ="Ezekiel", fileID="26", BB="26"}},
            {27, new ParatextBookFileName{code="DAN", abbr = "Dan", shortname ="Daniel", longname ="Daniel", fileID="27", BB="27"}},
            {28, new ParatextBookFileName{code="HOS", abbr = "Hos", shortname ="Hosea", longname ="Hosea", fileID="28", BB="28"}},
            {29, new ParatextBookFileName{code="JOL", abbr = "Joel", shortname ="Joel", longname ="Joel", fileID="29", BB="29"}},
            {30, new ParatextBookFileName{code="AMO", abbr = "Amos", shortname ="Amos", longname ="Amos", fileID="30", BB="30"}},
            {31, new ParatextBookFileName{code="OBA", abbr = "Obad", shortname ="Obadiah", longname ="Obadiah", fileID="31", BB="31"}},
            {32, new ParatextBookFileName{code="JON", abbr = "Jonah", shortname ="Jonah", longname ="Jonah", fileID="32", BB="32"}},
            {33, new ParatextBookFileName{code="MIC", abbr = "Micah", shortname ="Mic", longname ="Micah", fileID="33", BB="33"}},
            {34, new ParatextBookFileName{code="NAM", abbr = "Nah", shortname ="Nahum", longname ="Nahum", fileID="34", BB="34"}},
            {35, new ParatextBookFileName{code="HAB", abbr = "Hab", shortname ="Habakkuk", longname ="Habakkuk", fileID="35", BB="35"}},
            {36, new ParatextBookFileName{code="ZEP", abbr = "Zeph", shortname ="Zephaniah", longname ="Zephaniah", fileID="36", BB="36"}},
            {37, new ParatextBookFileName{code="HAG", abbr = "Hag", shortname ="Haggai", longname ="Haggai", fileID="37", BB="37"}},
            {38, new ParatextBookFileName{code="ZEC", abbr = "Zech", shortname ="Zechariah", longname ="Zechariah", fileID="38", BB="38"}},
            {39, new ParatextBookFileName{code="MAL", abbr = "Mal", shortname ="Malachi", longname ="Malachi", fileID="39", BB="39"}},
            // 40 - intentionally omitted
            {41, new ParatextBookFileName{code="MAT", abbr = "Matt", shortname ="Matthew", longname ="Matthew", fileID="41", BB="40"}},
            {42, new ParatextBookFileName{code="MRK", abbr = "Mark", shortname ="Mark", longname ="Mark", fileID="42", BB="41"}},
            {43, new ParatextBookFileName{code="LUK", abbr = "Luke", shortname ="Luke", longname ="Luke", fileID="43", BB="42"}},
            {44, new ParatextBookFileName{code="JHN", abbr = "John", shortname ="John", longname ="John", fileID="44", BB="43"}},
            {45, new ParatextBookFileName{code="ACT", abbr = "Acts", shortname ="Acts", longname ="Acts", fileID="45", BB="44"}},
            {46, new ParatextBookFileName{code="ROM", abbr = "Rom", shortname ="Romans", longname ="Romans", fileID="46", BB="45"}},
            {47, new ParatextBookFileName{code="1CO", abbr = "1 Cor", shortname ="1 Corinthians", longname ="1 Corinthians", fileID="47", BB="46"}},
            {48, new ParatextBookFileName{code="2CO", abbr = "2 Cor", shortname ="2 Corinthians", longname ="2 Corinthians", fileID="48", BB="47"}},
            {49, new ParatextBookFileName{code="GAL", abbr = "Galatians", shortname ="Galatians", longname ="Galatians", fileID="49", BB="48"}},
            {50, new ParatextBookFileName{code="EPH", abbr = "Eph", shortname ="Ephesians", longname ="Ephesians", fileID="50", BB="49"}},
            {51, new ParatextBookFileName{code="PHP", abbr = "Phil", shortname ="Philippians", longname ="Philippians", fileID="51", BB="50"}},
            {52, new ParatextBookFileName{code="COL", abbr = "Col", shortname ="Colossians", longname ="Colossians", fileID="52", BB="51"}},
            {53, new ParatextBookFileName{code="1TH", abbr = "1 Thess", shortname ="1 Thessalonians", longname ="1 Thessalonians", fileID="53", BB="52"}},
            {54, new ParatextBookFileName{code="2TH", abbr = "2 Thess", shortname ="2 Thessalonians", longname ="2 Thessalonians", fileID="54", BB="53"}},
            {55, new ParatextBookFileName{code="1TI", abbr = "1 Tim", shortname ="1 Timothy", longname ="1 Timothy", fileID="55", BB="54"}},
            {56, new ParatextBookFileName{code="2TI", abbr = "2 Tim", shortname ="2 Timothy", longname ="2 Timothy", fileID="56", BB="55"}},
            {57, new ParatextBookFileName{code="TIT", abbr = "Titus", shortname ="Titus", longname ="Titus", fileID="57", BB="56"}},
            {58, new ParatextBookFileName{code="PHM", abbr = "Phlm", shortname ="Philemon", longname ="Philemon", fileID="58", BB="57"}},
            {59, new ParatextBookFileName{code="HEB", abbr = "Heb", shortname ="Hebrews", longname ="Hebrews", fileID="59", BB="58"}},
            {60, new ParatextBookFileName{code="JAS", abbr = "Jas", shortname ="James", longname ="James", fileID="60", BB="59"}},
            {61, new ParatextBookFileName{code="1PE", abbr = "1 Pet", shortname ="1 Peter", longname ="1 Peter", fileID="61", BB="60"}},
            {62, new ParatextBookFileName{code="2PE", abbr = "2 Pet", shortname ="2 Peter", longname ="2 Peter", fileID="62", BB="61"}},
            {63, new ParatextBookFileName{code="1JN", abbr = "1 John", shortname ="1 John", longname ="1 John", fileID="63", BB="62"}},
            {64, new ParatextBookFileName{code="2JN", abbr = "2 John", shortname ="2 John", longname ="2 John", fileID="64", BB="63"}},
            {65, new ParatextBookFileName{code="3JN", abbr = "3 John", shortname ="3 John", longname ="3 John", fileID="65", BB="64"}},
            {66, new ParatextBookFileName{code="JUD", abbr = "Jude", shortname ="Jude", longname ="Jude", fileID="66", BB="65"}},
            {67, new ParatextBookFileName{code="REV", abbr = "Rev", shortname ="Revelation", longname ="Revelation", fileID="67", BB="66"}},
        };


        public DirectoryType DirectoryType { get; set; } = DirectoryType.Project;


        public string FullName { get; set; }

        public CorpusType CorpusType { get; set; }

      
        public string Guid { get; set; }

        
        public string Language { get; set; }

    
        public string Encoding { get; set; }

      
        public string LanguageIsoCode { get; set; }

      
        public TranslationInfo TranslationInfo { get; set; }

        public TranslationInfo BaseTranslation { get; set; }

       
        public string DefaultFont { get; set; }

       
        public string NormalizationForm { get; set; }

      
        public string Name { get; set; }

       
        public string Copyright { get; set; }

      
        public string ProjectPath { get; set; }

        public bool IsRTL { get; set; } = false;
        public string DirectoryPath { get; set; }
        public string ClearEngineDirectoryPath { get; set; }
        public string FileNamePrePart { get; set; }
        public string FileNamePostPart { get; set; }
        public string FileNameBookNameForm { get; set; }
        public string BooksPresent { get; set; }

        public bool HasCustomVRSfile { get; set; } = false;
        public string CustomVRSfilePath { get; set; } = "";

        private int _versification;
        public int Versification
        {
            get => _versification;
            set
            {
                _versification = value;
                switch (_versification)
                {
                    case 0:
                        ScrVers = ScrVersType.Unknown;
                        break;
                    case 1:
                        ScrVers = ScrVersType.Original;
                        break;
                    case 2:
                        ScrVers = ScrVersType.Septuagint;
                        break;
                    case 3:
                        ScrVers = ScrVersType.Vulgate;
                        break;
                    case 4:
                        ScrVers = ScrVersType.English;
                        break;
                    case 5:
                        ScrVers = ScrVersType.RussianProtestant;
                        break;
                    case 6:
                        ScrVers = ScrVersType.RussianOrthodox;
                        break;

                }
            }
        }

        public ScrVersType ScrVers { get; set; }
        public List<ParatextBook> BooksList { get; set; } = new List<ParatextBook>();

    }
}
