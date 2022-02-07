using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using SIL.Scripture;

namespace ClearDashboard.Common.Models
{
    public class ParatextProject : INotifyPropertyChanged
    {
        public enum eProjectType
        {
            Unknown,
            Standard,
            Resource,
            BackTranslation,
            Auxiliary,
            Daughter,
            MarbleResource,
        }

        public enum eDirType
        {
            Project,
            Resources
        }

        public Dictionary<int, ParatextBookName> BookNames = new Dictionary<int, ParatextBookName>
        {
            {01, new ParatextBookName{code="GEN", abbr = "Gen", shortname ="Genesis", longname ="Genesis"}},
            {02, new ParatextBookName{code="EXO", abbr = "Exod", shortname ="Exodus", longname ="Exodus"}},
            {03, new ParatextBookName{code="LEV", abbr = "Lev", shortname ="Leviticus", longname ="Leviticus"}},
            {04, new ParatextBookName{code="NUM", abbr = "Num", shortname ="Numbers", longname ="Numbers"}},
            {05, new ParatextBookName{code="DEU", abbr = "Deut", shortname ="Deuteronomy", longname ="Deuteronomy"}},
            {06, new ParatextBookName{code="JOS", abbr = "Josh", shortname ="Joshua", longname ="Joshua"}},
            {07, new ParatextBookName{code="JDG", abbr = "Judg", shortname ="Judges", longname ="Judges"}},
            {08, new ParatextBookName{code="RUT", abbr = "Ruth", shortname ="Ruth", longname ="Ruth"}},
            {09, new ParatextBookName{code="1SA", abbr = "1 Sam", shortname ="1 Samuel", longname ="1 Samuel"}},
            {10, new ParatextBookName{code="2SA", abbr = "2 Sam", shortname ="2 Samuel", longname ="2 Samuel"}},
            {11, new ParatextBookName{code="1KI", abbr = "1 Kgs", shortname ="1 Kings", longname ="1 Kings"}},
            {12, new ParatextBookName{code="2KI", abbr = "2 Kgs", shortname ="2 Kings", longname ="2 Kings"}},
            {13, new ParatextBookName{code="1CH", abbr = "1 Chr", shortname ="1 Chronicles", longname ="1 Chronicles"}},
            {14, new ParatextBookName{code="2CH", abbr = "2 Chr", shortname ="2 Chronicles", longname ="2 Chronicles"}},
            {15, new ParatextBookName{code="EZR", abbr = "Ezra", shortname ="Ezra", longname ="Ezra"}},
            {16, new ParatextBookName{code="NEH", abbr = "Neh", shortname ="Nehemiah", longname ="Nehemiah"}},
            {17, new ParatextBookName{code="EST", abbr = "Esth", shortname ="Esther", longname ="Esther"}},
            {18, new ParatextBookName{code="JOB", abbr = "Job", shortname ="Job", longname ="Job"}},
            {19, new ParatextBookName{code="PSA", abbr = "Ps(s)", shortname ="Psalms", longname ="Psalms"}},
            {20, new ParatextBookName{code="PRO", abbr = "Prov", shortname ="Proverbs", longname ="Proverbs"}},
            {21, new ParatextBookName{code="ECC", abbr = "Eccl", shortname ="Ecclesiastes", longname ="Ecclesiastes"}},
            {22, new ParatextBookName{code="SNG", abbr = "Song", shortname ="Song of Songs", longname ="The Song of Songs"}},
            {23, new ParatextBookName{code="ISA", abbr = "Isa", shortname ="Isaiah", longname ="Isaiah"}},
            {24, new ParatextBookName{code="JER", abbr = "Jer", shortname ="Jeremiah", longname ="Jeremiah"}},
            {25, new ParatextBookName{code="LAM", abbr = "Lam", shortname ="Lamentations", longname ="Lamentations"}},
            {26, new ParatextBookName{code="EZK", abbr = "Ezek", shortname ="Ezekiel", longname ="Ezekiel"}},
            {27, new ParatextBookName{code="DAN", abbr = "Dan", shortname ="Daniel", longname ="Daniel"}},
            {28, new ParatextBookName{code="HOS", abbr = "Hos", shortname ="Hosea", longname ="Hosea"}},
            {29, new ParatextBookName{code="JOL", abbr = "Joel", shortname ="Joel", longname ="Joel"}},
            {30, new ParatextBookName{code="AMO", abbr = "Amos", shortname ="Amos", longname ="Amos"}},
            {31, new ParatextBookName{code="OBA", abbr = "Obad", shortname ="Obadiah", longname ="Obadiah"}},
            {32, new ParatextBookName{code="JON", abbr = "Jonah", shortname ="Jonah", longname ="Jonah"}},
            {33, new ParatextBookName{code="MIC", abbr = "Micah", shortname ="Mic", longname ="Micah"}},
            {34, new ParatextBookName{code="NAM", abbr = "Nah", shortname ="Nahum", longname ="Nahum"}},
            {35, new ParatextBookName{code="HAB", abbr = "Hab", shortname ="Habakkuk", longname ="Habakkuk"}},
            {36, new ParatextBookName{code="ZEP", abbr = "Zeph", shortname ="Zephaniah", longname ="Zephaniah"}},
            {37, new ParatextBookName{code="HAG", abbr = "Hag", shortname ="Haggai", longname ="Haggai"}},
            {38, new ParatextBookName{code="ZEC", abbr = "Zech", shortname ="Zechariah", longname ="Zechariah"}},
            {39, new ParatextBookName{code="MAL", abbr = "Mal", shortname ="Malachi", longname ="Malachi"}},
            {41, new ParatextBookName{code="MAT", abbr = "Matt", shortname ="Matthew", longname ="Matthew"}},
            {42, new ParatextBookName{code="MRK", abbr = "Mark", shortname ="Mark", longname ="Mark"}},
            {43, new ParatextBookName{code="LUK", abbr = "Luke", shortname ="Luke", longname ="Luke"}},
            {44, new ParatextBookName{code="JHN", abbr = "John", shortname ="John", longname ="John"}},
            {45, new ParatextBookName{code="ACT", abbr = "Acts", shortname ="Acts", longname ="Acts"}},
            {46, new ParatextBookName{code="ROM", abbr = "Rom", shortname ="Romans", longname ="Romans"}},
            {47, new ParatextBookName{code="1CO", abbr = "1 Cor", shortname ="1 Corinthians", longname ="1 Corinthians"}},
            {48, new ParatextBookName{code="2CO", abbr = "2 Cor", shortname ="2 Corinthians", longname ="2 Corinthians"}},
            {49, new ParatextBookName{code="GAL", abbr = "Galatians", shortname ="Galatians", longname ="Galatians"}},
            {50, new ParatextBookName{code="EPH", abbr = "Eph", shortname ="Ephesians", longname ="Ephesians"}},
            {51, new ParatextBookName{code="PHP", abbr = "Phil", shortname ="Philippians", longname ="Philippians"}},
            {52, new ParatextBookName{code="COL", abbr = "Col", shortname ="Colossians", longname ="Colossians"}},
            {53, new ParatextBookName{code="1TH", abbr = "1 Thess", shortname ="1 Thessalonians", longname ="1 Thessalonians"}},
            {54, new ParatextBookName{code="2TH", abbr = "2 Thess", shortname ="2 Thessalonians", longname ="2 Thessalonians"}},
            {55, new ParatextBookName{code="1TI", abbr = "1 Tim", shortname ="1 Timothy", longname ="1 Timothy"}},
            {56, new ParatextBookName{code="2TI", abbr = "2 Tim", shortname ="2 Timothy", longname ="2 Timothy"}},
            {57, new ParatextBookName{code="TIT", abbr = "Titus", shortname ="Titus", longname ="Titus"}},
            {58, new ParatextBookName{code="PHM", abbr = "Phlm", shortname ="Philemon", longname ="Philemon"}},
            {59, new ParatextBookName{code="HEB", abbr = "Heb", shortname ="Hebrews", longname ="Hebrews"}},
            {60, new ParatextBookName{code="JAS", abbr = "Jas", shortname ="James", longname ="James"}},
            {61, new ParatextBookName{code="1PE", abbr = "1 Pet", shortname ="1 Peter", longname ="1 Peter"}},
            {62, new ParatextBookName{code="2PE", abbr = "2 Pet", shortname ="2 Peter", longname ="2 Peter"}},
            {63, new ParatextBookName{code="1JN", abbr = "1 John", shortname ="1 John", longname ="1 John"}},
            {64, new ParatextBookName{code="2JN", abbr = "2 John", shortname ="2 John", longname ="2 John"}},
            {65, new ParatextBookName{code="3JN", abbr = "3 John", shortname ="3 John", longname ="3 John"}},
            {66, new ParatextBookName{code="JUD", abbr = "Jude", shortname ="Jude", longname ="Jude"}},
            {67, new ParatextBookName{code="REV", abbr = "Rev", shortname ="Revelation", longname ="Revelation"}},
        };


        private eDirType _dirType = eDirType.Project;
        public eDirType DirType
        {
            get => _dirType;
            set { _dirType = value; }
        }


        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        private eProjectType _projectType;

        public eProjectType ProjectType
        {
            get => _projectType;
            set
            {
                _projectType = value;
                OnPropertyChanged(nameof(ProjectType));
            }
        }

        private string _guid;
        public string Guid
        {
            get => _guid;
            set
            {
                _guid = value;
                OnPropertyChanged(nameof(Guid));
            }
        }

        private string _Language;
        public string Language
        {
            get => _Language;
            set
            {
                _Language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        private string _Encoding;
        public string Encoding
        {
            get => _Encoding;
            set
            {
                _Encoding = value;
                OnPropertyChanged(nameof(Encoding));
            }
        }

        private string _LanguageIsoCode;
        public string LanguageIsoCode
        {
            get => _LanguageIsoCode;
            set
            {
                _LanguageIsoCode = value;
                OnPropertyChanged(nameof(LanguageIsoCode));
            }
        }

        private Translation_Info _TranslationInfo;
        public Translation_Info TranslationInfo
        {
            get => _TranslationInfo;
            set
            {
                _TranslationInfo = value;
                OnPropertyChanged(nameof(TranslationInfo));
            }
        }

        private Translation_Info _BaseTranslation;
        public Translation_Info BaseTranslation
        {
            get => _BaseTranslation;
            set
            {
                _BaseTranslation = value;
                OnPropertyChanged(nameof(BaseTranslation));
            }
        }

        private string _DefaultFont;
        public string DefaultFont
        {
            get => _DefaultFont; 
            set
            {
                _DefaultFont = value;
                OnPropertyChanged(nameof(DefaultFont));
            }
        }

        private string _NormalizationForm;
        public string NormalizationForm
        {
            get => _NormalizationForm;
            set
            {
                _NormalizationForm = value;
                OnPropertyChanged(nameof(NormalizationForm));
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _Copyright;
        public string Copyright
        {
            get => _Copyright;
            set
            {
                _Copyright = value;
                OnPropertyChanged(nameof(Copyright));
            }
        }

        private string _projectPath;
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                _projectPath = value;
                OnPropertyChanged(nameof(ProjectPath));
            }
        }

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
        public List<ParatextBook> BooksList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Translation_Info
    {
        public ParatextProject.eProjectType projectType { get; set; } = ParatextProject.eProjectType.Unknown;
        public string projectName { get; set; } = "";
        public string projectGuid { get; set; } = "";
    }
}
