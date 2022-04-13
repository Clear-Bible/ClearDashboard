﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace ClearDashboard.DataAccessLayer.Paratext
{
    public class ParatextProxy
    {
        private string _paratextProjectPath = string.Empty;
        public string ParatextProjectPath
        {
            get => _paratextProjectPath;
            set => _paratextProjectPath = value;
        }

        private string _paratextInstallPath = string.Empty;
        public string ParatextInstallPath
        {
            get => _paratextInstallPath;
            set => _paratextInstallPath = value;
        }

        private string _paratextResourcesPath = string.Empty;
        public string ParatextResourcePath
        {
            get => _paratextResourcesPath;
            set => _paratextResourcesPath = value;
        }


        private readonly ILogger _logger;
        public enum FolderType
        {
            Projects,
            Resources,
        }

        public ParatextProxy(ILogger<ParatextProxy> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns if paratext is installed on the computer or not
        /// </summary>
        /// <returns></returns>
        public bool IsParatextInstalled()
        {

            GetParatextProjectsPath();
            GetParatextInstallPath();

            if (string.IsNullOrEmpty(_paratextProjectPath))
            {
                return false;
            }

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Application is intended for Windows OS only.")]
        private void GetParatextInstallPath()
        {
            _paratextInstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Paratext9_Full_Release_AppPath", null);

            // check if path exists
            if (!Directory.Exists(_paratextInstallPath))
            {
                // file doesn't exist so null this out
                _paratextInstallPath = "";
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Application is intended for Windows OS only.")]
        private void GetParatextProjectsPath()
        {
            _paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(_paratextProjectPath))
            {
                // directory doesn't exist so null this out
                _paratextProjectPath = "";
            }
            else
            {
                _paratextResourcesPath = Path.Combine(_paratextProjectPath, "_Resources");
            }
        }

        /// <summary>
        /// Obtain the installed Paratext Projects and Resources
        /// </summary>
        /// <param name="folderType"></param>
        /// <returns></returns>
        public async Task<List<ParatextProject>> GetParatextProjectsOrResources(FolderType folderType = FolderType.Projects)
        {
            if (_paratextProjectPath == "")
            {
                GetParatextProjectsPath();
            }

            var searchPath = "";
            if (folderType == FolderType.Projects)
            {
                searchPath = _paratextProjectPath;
            } 
            else if (folderType == FolderType.Resources)
            {
                searchPath = _paratextResourcesPath;
            }

            // look through these folders for files which are called "unique.id"
            var directories = Directory.GetDirectories(searchPath);

            var projects = new List<ParatextProject>();

            await Task.Run(() =>
            {
                foreach (var directory in directories)
                {
                    // look for settings.xml file
                    var sSettingFilePath = Path.Combine(directory, "settings.xml");

                    // read in settings.xml file
                    if (File.Exists(sSettingFilePath))
                    {
                      
                        var project = GetParatextProject(sSettingFilePath, DirType.Project);
                        var sBookNamesPath = Path.Combine(directory, "booknames.xml");
                        if (project.FullName != "")
                        {
                            // get the books
                            project.BooksList = GetBookList(project, directory);


                            // check for custom VRS file
                            var customVRSfilePath = Path.Combine(directory, "custom.vrs");
                            if (File.Exists(customVRSfilePath))
                            {
                                project.HasCustomVRSfile = true;
                                project.CustomVRSfilePath = customVRSfilePath;
                            }

                            // read in booknames.xml file
                            if (File.Exists(sBookNamesPath))
                            {
                                var xmlString = File.ReadAllText(sBookNamesPath);

                                var iRow = 1;
                                // Create an XmlReader
                                using (var reader = XmlReader.Create(new StringReader(xmlString)))
                                {
                                    while (reader.Read())
                                    {
                                        reader.ReadToFollowing("book");
                                        reader.MoveToFirstAttribute();
                                        var code = reader.Value;
                                        reader.MoveToNextAttribute();
                                        var abbr = reader.Value;
                                        reader.MoveToNextAttribute();
                                        var shortname = reader.Value;
                                        reader.MoveToNextAttribute();
                                        var longname = reader.Value;

                                        iRow++;
                                        if (iRow == 40)
                                        {
                                            iRow = 41;
                                        }

                                        if (project.BookNames.ContainsKey(iRow))
                                        {
                                            var temp = project.BookNames[iRow];

                                            if (temp.code == code)
                                            {
                                                temp.abbr = abbr;
                                                temp.shortname = shortname;
                                                temp.longname = longname;

                                                // replace key
                                                project.BookNames.Remove(iRow);
                                                project.BookNames.Add(iRow, temp);
                                            }

                                        }

                                        // exit after revelation
                                        if (iRow == 67)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            projects.Add(project);
                        }
                    }

                }
            }).ConfigureAwait(false);

            // alphabetize the list by the short name
            projects.Sort((a, b) => a.Name.CompareTo(b.Name));

            return projects;
        }

        public string GetCurrentParatextUser()
        {
            if (_paratextResourcesPath == "")
            {
                GetParatextProjectsPath();
            }

            var user = "USER NOT DETERMINED";
            var userfile = Path.Combine(this._paratextProjectPath, "localUsers.txt");
            if (File.Exists(userfile))
            {
                var tmp = File.ReadAllText(userfile);
                var users = tmp.Split("\r\n");
                if (users.Length > 0)
                {
                    return users[0];
                }
            }
            else
            {
                return "USER UNKNOWN";
            }

            return user;
        }

        public List<ParatextProject> GetParatextResources()
        {
            if (_paratextResourcesPath == "")
            {
                GetParatextProjectsPath();
            }

            var searchPath = _paratextResourcesPath;
            

            // look through these folders for files which are called "unique.id"
            var files = Directory.GetFiles(searchPath, "*.p8z");

            var projects = new List<ParatextProject>();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                projects.Add(new ParatextProject
                {
                    Name = fileInfo.Name.Replace(fileInfo.Extension, ""),
                    ProjectType = ProjectType.Resource,
                    DirType = DirType.Resources
                });
            }

            // alphabetize the list by the short name
            projects.Sort((a, b) => a.Name.CompareTo(b.Name));

            return projects;
        }

        public static List<SelectedBook> UpdateBooks(ParatextProject paratextProject, List<SelectedBook> books)
        {
            for (var i = 0; i < paratextProject.BooksList.Count; i++)
            {
                if (paratextProject.BooksList[i].Available)
                {
                    //_logger.LogInformation(books[i].BookName + " " + paratextProject.BooksList[i].BookNameShort + " AVAILABLE");
                    books[i].IsEnabled = true;
                }
                else
                {
                    //_logger.LogInformation(books[i].BookName + " " + paratextProject.BooksList[i].BookNameShort + " MISSING");
                    books[i].IsEnabled = false;
                    books[i].IsSelected = false;
                }
            }

            return books;
        }

        /// <summary>
        /// Parse through the Settings file and return back a ParatextProject obj
        /// </summary>
        /// <param name="settingFilePath"></param>
        /// <param name="dirType"></param>
        /// <returns></returns>
        public ParatextProject GetParatextProject(string settingFilePath, DirType dirType)
        {
            var paratextProject = new ParatextProject();

            try
            {
                var xml = File.ReadAllText(settingFilePath);
                var serializer = new XmlSerializer(typeof(ScriptureText));
                using (var reader = new StringReader(xml))
                {
                    var scriptureText = (ScriptureText)serializer.Deserialize(reader);
                    if (scriptureText != null)
                    {
                        paratextProject.Name = scriptureText.Name;
                        paratextProject.Guid = scriptureText.Guid;
                        paratextProject.FullName = scriptureText.FullName;
                        paratextProject.Copyright = scriptureText.Copyright;
                        paratextProject.BooksPresent = scriptureText.BooksPresent;
                        paratextProject.FileNameBookNameForm = scriptureText.FileNameBookNameForm;
                        if (scriptureText.FileNamePostPart != null)
                        {
                            paratextProject.FileNamePostPart = scriptureText.FileNamePostPart;
                        }
                        if (scriptureText.FileNamePrePart != null)
                        {
                            if (scriptureText.FileNamePrePart is object)
                            {
                                paratextProject.FileNamePrePart = "";
                            }
                            else
                            {
                                paratextProject.FileNamePrePart = scriptureText.FileNamePrePart.ToString();
                            }
                        }

                        if (scriptureText.NormalizationForm != null)
                        {
                            paratextProject.NormalizationForm = scriptureText.NormalizationForm;
                        }

                        if (scriptureText.Language != null)
                        {
                            paratextProject.Language = scriptureText.Language;
                        }

                        if (scriptureText.DefaultFont != null)
                        {
                            paratextProject.DefaultFont = scriptureText.DefaultFont;
                        }

                        if (scriptureText.Encoding != null)
                        {
                            paratextProject.Encoding = scriptureText.Encoding;
                        }

                        if (scriptureText.LanguageIsoCode != null)
                        {
                            paratextProject.LanguageIsoCode = scriptureText.LanguageIsoCode;
                        }

                        if (scriptureText.LeftToRight != null)
                        {
                            if (scriptureText.LeftToRight.ToUpper() == "T")
                            {
                                paratextProject.IsRTL = true;
                            }
                        }

                        if (scriptureText.TranslationInfo != null)
                        {
                            var split = scriptureText.TranslationInfo.Split(':');
                            if (split.Length >= 3)
                            {
                                var projType = GetProjectType(split[0], dirType);

                                paratextProject.TranslationInfo = new Translation_Info
                                {
                                    projectType = projType,
                                    projectName = split[1],
                                    projectGuid = split[2],
                                };

                                paratextProject.ProjectType = projType;
                            }
                        }

                        if (scriptureText.BaseTranslation != null)
                        {
                            var split = scriptureText.BaseTranslation.Split(':');
                            if (split.Length >= 3)
                            {
                               var projType = GetProjectType(split[0], dirType);

                                paratextProject.BaseTranslation = new Translation_Info
                                {
                                    projectType = projType,
                                    projectName = split[1],
                                    projectGuid = split[2],
                                };
                            }
                        }

                        paratextProject.Versification = scriptureText.Versification;
                        paratextProject.ProjectPath = settingFilePath;

                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An unexpected error occurred while deserializing the setting file {settingFilePath}");
            }

            FileInfo fileInfo = new FileInfo(settingFilePath);
            paratextProject.DirectoryPath = fileInfo.DirectoryName;

            return paratextProject;
        }

        /// <summary>
        /// convert from string to a project type enum
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dirType"></param>
        /// <returns></returns>
        private ProjectType GetProjectType(string s, DirType dirType)
        {
            s = s.ToUpper().Trim();

            switch (s)
            {
                case "STANDARD":
                    if (dirType == DirType.Project)
                    {
                        return ProjectType.Standard;
                    }
                    else
                    {
                        return ProjectType.Resource;
                    }
                case "BACKTRANSLATION":
                    return ProjectType.BackTranslation;
                case "AUXILIARY":
                    return ProjectType.Auxiliary;
                case "DAUGHTER":
                    return ProjectType.Daughter;
                case "MARBLERESOURCE":
                    return ProjectType.MarbleResource;
            }

            return ProjectType.Unknown;
        }

        public List<ParatextBook> GetBookList(ParatextProject project, string directory)
        {
            // initialize the list
            var books = new List<ParatextBook>
            {
                new ParatextBook{BookId = "01", BookNameShort = "GEN", Available = false, FilePath = "", USFM_Num = 1},
                new ParatextBook{BookId = "02", BookNameShort = "EXO", Available = false, FilePath = "", USFM_Num = 2},
                new ParatextBook{BookId = "03", BookNameShort = "LEV", Available = false, FilePath = "", USFM_Num = 3},
                new ParatextBook{BookId = "04", BookNameShort = "NUM", Available = false, FilePath = "", USFM_Num = 4},
                new ParatextBook{BookId = "05", BookNameShort = "DEU", Available = false, FilePath = "", USFM_Num = 5},
                new ParatextBook{BookId = "06", BookNameShort = "JOS", Available = false, FilePath = "", USFM_Num = 6},
                new ParatextBook{BookId = "07", BookNameShort = "JDG", Available = false, FilePath = "", USFM_Num = 7},
                new ParatextBook{BookId = "08", BookNameShort = "RUT", Available = false, FilePath = "", USFM_Num = 8},
                new ParatextBook{BookId = "09", BookNameShort = "1SA", Available = false, FilePath = "", USFM_Num = 9},
                new ParatextBook{BookId = "10", BookNameShort = "2SA", Available = false, FilePath = "", USFM_Num = 10},
                new ParatextBook{BookId = "11", BookNameShort = "1KI", Available = false, FilePath = "", USFM_Num = 11},
                new ParatextBook{BookId = "12", BookNameShort = "2KI", Available = false, FilePath = "", USFM_Num = 12},
                new ParatextBook{BookId = "13", BookNameShort = "1CH", Available = false, FilePath = "", USFM_Num = 13},
                new ParatextBook{BookId = "14", BookNameShort = "2CH", Available = false, FilePath = "", USFM_Num = 14},
                new ParatextBook{BookId = "15", BookNameShort = "EZR", Available = false, FilePath = "", USFM_Num = 15},
                new ParatextBook{BookId = "16", BookNameShort = "NEH", Available = false, FilePath = "", USFM_Num = 16},
                new ParatextBook{BookId = "17", BookNameShort = "EST", Available = false, FilePath = "", USFM_Num = 17},
                new ParatextBook{BookId = "18", BookNameShort = "JOB", Available = false, FilePath = "", USFM_Num = 18},
                new ParatextBook{BookId = "19", BookNameShort = "PSA", Available = false, FilePath = "", USFM_Num = 19},
                new ParatextBook{BookId = "20", BookNameShort = "PRO", Available = false, FilePath = "", USFM_Num = 20},
                new ParatextBook{BookId = "21", BookNameShort = "ECC", Available = false, FilePath = "", USFM_Num = 21},
                new ParatextBook{BookId = "22", BookNameShort = "SNG", Available = false, FilePath = "", USFM_Num = 22},
                new ParatextBook{BookId = "23", BookNameShort = "ISA", Available = false, FilePath = "", USFM_Num = 23},
                new ParatextBook{BookId = "24", BookNameShort = "JER", Available = false, FilePath = "", USFM_Num = 24},
                new ParatextBook{BookId = "25", BookNameShort = "LAM", Available = false, FilePath = "", USFM_Num = 25},
                new ParatextBook{BookId = "26", BookNameShort = "EZK", Available = false, FilePath = "", USFM_Num = 26},
                new ParatextBook{BookId = "27", BookNameShort = "DAN", Available = false, FilePath = "", USFM_Num = 27},
                new ParatextBook{BookId = "28", BookNameShort = "HOS", Available = false, FilePath = "", USFM_Num = 28},
                new ParatextBook{BookId = "29", BookNameShort = "JOL", Available = false, FilePath = "", USFM_Num = 29},
                new ParatextBook{BookId = "30", BookNameShort = "AMO", Available = false, FilePath = "", USFM_Num = 30},
                new ParatextBook{BookId = "31", BookNameShort = "OBA", Available = false, FilePath = "", USFM_Num = 31},
                new ParatextBook{BookId = "32", BookNameShort = "JON", Available = false, FilePath = "", USFM_Num = 32},
                new ParatextBook{BookId = "33", BookNameShort = "MIC", Available = false, FilePath = "", USFM_Num = 33},
                new ParatextBook{BookId = "34", BookNameShort = "NAM", Available = false, FilePath = "", USFM_Num = 34},
                new ParatextBook{BookId = "35", BookNameShort = "HAB", Available = false, FilePath = "", USFM_Num = 35},
                new ParatextBook{BookId = "36", BookNameShort = "ZEP", Available = false, FilePath = "", USFM_Num = 36},
                new ParatextBook{BookId = "37", BookNameShort = "HAG", Available = false, FilePath = "", USFM_Num = 37},
                new ParatextBook{BookId = "38", BookNameShort = "ZEC", Available = false, FilePath = "", USFM_Num = 38},
                new ParatextBook{BookId = "39", BookNameShort = "MAL", Available = false, FilePath = "", USFM_Num = 39},
                new ParatextBook{BookId = "40", BookNameShort = "MAT", Available = false, FilePath = "", USFM_Num = 41},
                new ParatextBook{BookId = "41", BookNameShort = "MRK", Available = false, FilePath = "", USFM_Num = 42},
                new ParatextBook{BookId = "42", BookNameShort = "LUK", Available = false, FilePath = "", USFM_Num = 43},
                new ParatextBook{BookId = "43", BookNameShort = "JHN", Available = false, FilePath = "", USFM_Num = 44},
                new ParatextBook{BookId = "44", BookNameShort = "ACT", Available = false, FilePath = "", USFM_Num = 45},
                new ParatextBook{BookId = "45", BookNameShort = "ROM", Available = false, FilePath = "", USFM_Num = 46},
                new ParatextBook{BookId = "46", BookNameShort = "1CO", Available = false, FilePath = "", USFM_Num = 47},
                new ParatextBook{BookId = "47", BookNameShort = "2CO", Available = false, FilePath = "", USFM_Num = 48},
                new ParatextBook{BookId = "48", BookNameShort = "GAL", Available = false, FilePath = "", USFM_Num = 49},
                new ParatextBook{BookId = "49", BookNameShort = "EPH", Available = false, FilePath = "", USFM_Num = 50},
                new ParatextBook{BookId = "50", BookNameShort = "PHP", Available = false, FilePath = "", USFM_Num = 51},
                new ParatextBook{BookId = "51", BookNameShort = "COL", Available = false, FilePath = "", USFM_Num = 52},
                new ParatextBook{BookId = "52", BookNameShort = "1TH", Available = false, FilePath = "", USFM_Num = 53},
                new ParatextBook{BookId = "53", BookNameShort = "2TH", Available = false, FilePath = "", USFM_Num = 54},
                new ParatextBook{BookId = "54", BookNameShort = "1TI", Available = false, FilePath = "", USFM_Num = 55},
                new ParatextBook{BookId = "55", BookNameShort = "2TI", Available = false, FilePath = "", USFM_Num = 56},
                new ParatextBook{BookId = "56", BookNameShort = "TIT", Available = false, FilePath = "", USFM_Num = 57},
                new ParatextBook{BookId = "57", BookNameShort = "PHM", Available = false, FilePath = "", USFM_Num = 58},
                new ParatextBook{BookId = "58", BookNameShort = "HEB", Available = false, FilePath = "", USFM_Num = 59},
                new ParatextBook{BookId = "59", BookNameShort = "JAS", Available = false, FilePath = "", USFM_Num = 60},
                new ParatextBook{BookId = "60", BookNameShort = "1PE", Available = false, FilePath = "", USFM_Num = 61},
                new ParatextBook{BookId = "61", BookNameShort = "2PE", Available = false, FilePath = "", USFM_Num = 62},
                new ParatextBook{BookId = "62", BookNameShort = "1JN", Available = false, FilePath = "", USFM_Num = 63},
                new ParatextBook{BookId = "63", BookNameShort = "2JN", Available = false, FilePath = "", USFM_Num = 64},
                new ParatextBook{BookId = "64", BookNameShort = "3JN", Available = false, FilePath = "", USFM_Num = 65},
                new ParatextBook{BookId = "65", BookNameShort = "JUD", Available = false, FilePath = "", USFM_Num = 66},
                new ParatextBook{BookId = "66", BookNameShort = "REV", Available = false, FilePath = "", USFM_Num = 67}
            };

            for (var i = 0; i < project.BooksPresent.Length; i++)
            {
                // stop after OT / NT
                if (i >= 66)
                {
                    break;
                }

                try
                {
                    if (project.BooksPresent[i] == '1')
                    {
                        // book present
                        books[i].Available = true;
                        // build the path

                        var fileName = string.Empty;

                        if (project.FileNamePrePart != "System.Object")
                        {
                            fileName += project.FileNamePrePart;
                        }

                        if (project.FileNameBookNameForm == "41MAT")
                        {
                            fileName += books[i].USFM_Num.ToString().PadLeft(2, '0') + books[i].BookNameShort;
                        }
                        else if (project.FileNameBookNameForm == "41")
                        {
                            fileName += books[i].USFM_Num.ToString().PadLeft(2, '0');
                        }
                        else if (project.FileNameBookNameForm == "MAT")
                        {
                            fileName += books[i].BookNameShort;
                        }

                        if (project.FileNamePostPart != "System.Object")
                        {
                            fileName += project.FileNamePostPart;
                        }

                        books[i].FilePath = fileName;

                        // check if the file exists
                        if (!File.Exists(Path.Combine(directory, fileName)))
                        {
                            // check to see if the file exists with a .usx ending instead
                            fileName = fileName.Substring(0, fileName.LastIndexOf("."));
                            fileName += ".usx";
                            fileName = Path.Combine(directory, fileName);
                            if (File.Exists(fileName))
                            {
                                books[i].FilePath = fileName;
                            }
                            else
                            {
                                books[i].FilePath = "";
                                books[i].Available = false;
                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while getting the Paratext book list.");
                   
                    throw;
                }

            }

            return books;
        }


    }
}
