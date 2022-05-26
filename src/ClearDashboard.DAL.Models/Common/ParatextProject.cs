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

            {68, new ParatextBookFileName{code="TOB", abbr = "TOB", shortname ="Tobit", longname ="Tobit", fileID="68", BB="67"}},
            {69, new ParatextBookFileName{code="JDT", abbr = "JDT", shortname ="Judith", longname ="Judith", fileID="69", BB="68"}},
            {70, new ParatextBookFileName{code="ESG", abbr = "ESG", shortname ="Esther Greek", longname ="Esther Greek", fileID="70", BB="69"}},
            {71, new ParatextBookFileName{code="WIS", abbr = "WIS", shortname ="Wisdom of Solomon", longname ="Wisdom of Solomon", fileID="70", BB="70"}},

            {72, new ParatextBookFileName{code="SIR", abbr = "SIR", shortname ="Sirach (Ecclesiasticus)", longname ="Sirach (Ecclesiasticus)", fileID="72", BB="71"}},
            {73, new ParatextBookFileName{code="BAR", abbr = "BAR", shortname ="Baruch", longname ="Baruch", fileID="73", BB="72"}},
            {74, new ParatextBookFileName{code="LJE", abbr = "LJE", shortname ="Letter of Jeremiah", longname ="Letter of Jeremiah", fileID="74", BB="73"}},
            {75, new ParatextBookFileName{code="S3Y", abbr = "S3Y", shortname ="Song of 3 Young Men", longname ="Song of 3 Young Men", fileID="75", BB="74"}},
            {76, new ParatextBookFileName{code="SUS", abbr = "SUS", shortname ="Susanna", longname ="Susanna", fileID="76", BB="75"}},
            {77, new ParatextBookFileName{code="BEL", abbr = "BEL", shortname ="Bel and the Dragon", longname ="Bel and the Dragon", fileID="77", BB="76"}},
            {78, new ParatextBookFileName{code="1MA", abbr = "1MA", shortname ="1 Maccabees", longname ="1 Maccabees", fileID="78", BB="77"}},
            {79, new ParatextBookFileName{code="2MA", abbr = "2MA", shortname ="2 Maccabees", longname ="2 Maccabees", fileID="79", BB="78"}},
            {80, new ParatextBookFileName{code="3MA", abbr = "3MA", shortname ="3 Maccabees", longname ="3 Maccabees", fileID="80", BB="79"}},
            {81, new ParatextBookFileName{code="4MA", abbr = "4MA", shortname ="4 Maccabees", longname ="4 Maccabees", fileID="81", BB="80"}},

            {82, new ParatextBookFileName{code="1ES", abbr = "1ES", shortname ="1 Esdras (Greek)", longname ="1 Esdras (Greek)", fileID="82", BB="81"}},
            {83, new ParatextBookFileName{code="2ES", abbr = "2ES", shortname ="2 Esdras (Latin)", longname ="2 Esdras (Latin)", fileID="83", BB="82"}},
            {84, new ParatextBookFileName{code="MAN", abbr = "MAN", shortname ="Prayer of Manasseh", longname ="Prayer of Manasseh", fileID="84", BB="83"}},
            {85, new ParatextBookFileName{code="PS2", abbr = "PS2", shortname ="Psalm 151", longname ="Psalm 151", fileID="85", BB="84"}},
            {86, new ParatextBookFileName{code="ODA", abbr = "ODA", shortname ="Odes", longname ="Odes", fileID="86", BB="85"}},
            {87, new ParatextBookFileName{code="PSS", abbr = "PSS", shortname ="Psalms of Solomon", longname ="Psalms of Solomon", fileID="87", BB="86"}},
            {88, new ParatextBookFileName{code="JSA", abbr = "JSA", shortname ="Joshua A. *obsolete*", longname ="Joshua A. *obsolete*", fileID="88", BB="87"}},
            {89, new ParatextBookFileName{code="JDB", abbr = "JDB", shortname ="Judges B. *obsolete*", longname ="Judges B. *obsolete*", fileID="89", BB="88"}},
            {90, new ParatextBookFileName{code="TBS", abbr = "TBS", shortname ="Tobit S. *obsolete*", longname ="Tobit S. *obsolete*", fileID="90", BB="89"}},
            {91, new ParatextBookFileName{code="SST", abbr = "SST", shortname ="Susanna Th. *obsolete*", longname ="Susanna Th. *obsolete*", fileID="91", BB="90"}},

            {92, new ParatextBookFileName{code="DNT", abbr = "DNT", shortname ="Daniel Th. *obsolete*", longname ="Daniel Th. *obsolete*", fileID="92", BB="91"}},
            {93, new ParatextBookFileName{code="BLT", abbr = "BLT", shortname ="Bel Th. *obsolete*", longname ="Bel Th. *obsolete*", fileID="93", BB="92"}},
            {94, new ParatextBookFileName{code="XXA", abbr = "XXA", shortname ="Extra A", longname ="Extra A", fileID="94", BB="93"}},
            {95, new ParatextBookFileName{code="XXB", abbr = "XXB", shortname ="Extra B", longname ="Extra B", fileID="95", BB="94"}},
            {96, new ParatextBookFileName{code="XXC", abbr = "XXC", shortname ="Extra C", longname ="Extra C", fileID="96", BB="95"}},
            {97, new ParatextBookFileName{code="XXD", abbr = "XXD", shortname ="Extra D", longname ="Extra D", fileID="97", BB="96"}},
            {98, new ParatextBookFileName{code="XXE", abbr = "XXE", shortname ="Extra E", longname ="Extra E", fileID="98", BB="97"}},
            {99, new ParatextBookFileName{code="XXF", abbr = "XXF", shortname ="Extra F", longname ="Extra F", fileID="99", BB="98"}},
            {100, new ParatextBookFileName{code="XXG", abbr = "XXG", shortname ="Extra G", longname ="Extra G", fileID="100", BB="99"}},
            {101, new ParatextBookFileName{code="FRT", abbr = "FRT", shortname ="Front Matter", longname ="Front Matter", fileID="101", BB="100"}},

            {102, new ParatextBookFileName{code="BAK", abbr = "BAK", shortname ="Back Matter", longname ="Back Matter", fileID="102", BB="101"}},
            {103, new ParatextBookFileName{code="OTH", abbr = "OTH", shortname ="Other Matter", longname ="Other Matter", fileID="103", BB="102"}},
            {104, new ParatextBookFileName{code="3ES", abbr = "3ES", shortname ="3 Ezra *obsolete*", longname ="3 Ezra *obsolete*", fileID="104", BB="103"}},
            {105, new ParatextBookFileName{code="EZA", abbr = "EZA", shortname ="Apocalypse of Ezra", longname ="Apocalypse of Ezra", fileID="105", BB="104"}},
            {106, new ParatextBookFileName{code="5EZ", abbr = "5EZ", shortname ="5 Ezra (Latin Prologue)", longname ="5 Ezra (Latin Prologue)", fileID="106", BB="105"}},
            {107, new ParatextBookFileName{code="6EZ", abbr = "6EZ", shortname ="6 Ezra (Latin Epilogue)", longname ="6 Ezra (Latin Epilogue)", fileID="107", BB="106"}},
            {108, new ParatextBookFileName{code="INT", abbr = "INT", shortname ="Introduction", longname ="Introduction", fileID="108", BB="107"}},
            {109, new ParatextBookFileName{code="CNC", abbr = "CNC", shortname ="Concordance", longname ="Concordance", fileID="109", BB="108"}},
            {110, new ParatextBookFileName{code="GLO", abbr = "GLO", shortname ="Glossary", longname ="Glossary", fileID="110", BB="109"}},
            {111, new ParatextBookFileName{code="TDX", abbr = "TDX", shortname ="Topical Index", longname ="Topical Index", fileID="111", BB="110"}},

            {112, new ParatextBookFileName{code="NDX", abbr = "NDX", shortname ="Names Index", longname ="Names Index", fileID="112", BB="111"}},
            {113, new ParatextBookFileName{code="DAG", abbr = "DAG", shortname ="Daniel Greek", longname ="Daniel Greek", fileID="113", BB="112"}},
            {114, new ParatextBookFileName{code="PS3", abbr = "PS3", shortname ="Psalms 152-155", longname ="Psalms 152-155", fileID="114", BB="113"}},
            {115, new ParatextBookFileName{code="2BA", abbr = "2BA", shortname ="2 Baruch (Apocalypse)", longname ="2 Baruch (Apocalypse)", fileID="115", BB="114"}},
            {116, new ParatextBookFileName{code="LBA", abbr = "LBA", shortname ="Letter of Baruch", longname ="Letter of Baruch", fileID="116", BB="115"}},
            {117, new ParatextBookFileName{code="JUB", abbr = "JUB", shortname ="Jubilees", longname ="Jubilees", fileID="117", BB="116"}},
            {118, new ParatextBookFileName{code="ENO", abbr = "ENO", shortname ="Enoch", longname ="Enoch", fileID="118", BB="117"}},
            {119, new ParatextBookFileName{code="1MQ", abbr = "1MQ", shortname ="1 Meqabyan", longname ="1 Meqabyan", fileID="119", BB="118"}},
            {120, new ParatextBookFileName{code="2MQ", abbr = "2MQ", shortname ="2 Meqabyan", longname ="2 Meqabyan", fileID="120", BB="119"}},
            {121, new ParatextBookFileName{code="3MQ", abbr = "3MQ", shortname ="3 Meqabyan", longname ="3 Meqabyan", fileID="121", BB="120"}},

            {122, new ParatextBookFileName{code="REP", abbr = "REP", shortname ="Reproof (Proverbs 25-31)", longname ="Reproof (Proverbs 25-31)", fileID="122", BB="121"}},
            {123, new ParatextBookFileName{code="4BA", abbr = "4BA", shortname ="4 Baruch (Rest of Baruch)", longname ="4 Baruch (Rest of Baruch)", fileID="123", BB="122"}},
            {124, new ParatextBookFileName{code="LAO", abbr = "LAO", shortname ="Laodiceans", longname ="Laodiceans", fileID="124", BB="123"}},


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
