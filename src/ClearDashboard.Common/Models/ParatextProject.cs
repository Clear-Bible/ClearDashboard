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
                DirectoryPath = _projectPath.Replace("settings.xml", "");

                if (!String.IsNullOrEmpty(Name))
                {
                    string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    ClearEngineDirectoryPath = Path.Combine(appPath, "CLEAR_Projects", Name,
                        "ClearEngine");

                    //check to see if the directory exists already
                    string newDir = Path.Combine(appPath, "CLEAR_Projects");
                    if (!Directory.Exists(newDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(newDir);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }

                    newDir = Path.Combine(newDir, Name);
                    if (!Directory.Exists(newDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(newDir);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }

                    newDir = Path.Combine(newDir, "ClearEngine");
                    if (!Directory.Exists(newDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(newDir);
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
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
}
