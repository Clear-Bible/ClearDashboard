using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ClearDashboard.Common.Models;
using Microsoft.Win32;

namespace ClearDashboard.DAL.Paratext
{
    public class ParatextUtils
    {
        private string paratextProjectPath = "";
        private string paratextInstallPath = "";
        private string paratextResourcesPath = "";

        public enum eFolderType
        {
            Projects,
            Resources,
        }

        /// <summary>
        /// Returns if paratext is installed on the computer or not
        /// </summary>
        /// <returns></returns>
        public bool IsParatextInstalled()
        {

            GetParatextProjectsPath();
            GetParatextInstallPath();

            if (string.IsNullOrEmpty(paratextProjectPath))
            {
                return false;
            }

            return true;
        }

        private void GetParatextInstallPath()
        {
            paratextInstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Paratext9_Full_Release_AppPath", null);

            // check if path exists
            if (!Directory.Exists(paratextInstallPath))
            {
                // file doesn't exist so null this out
                paratextInstallPath = "";
            }
        }

        private void GetParatextProjectsPath()
        {
            paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(paratextProjectPath))
            {
                // directory doesn't exist so null this out
                paratextProjectPath = "";
            }
            else
            {
                paratextResourcesPath = Path.Combine(paratextProjectPath, "_Resources");
            }
        }

        /// <summary>
        /// Obtain the installed Paratext Projects and Resources
        /// </summary>
        /// <param name="folderType"></param>
        /// <returns></returns>
        public async Task<List<ParatextProject>> GetParatextProjectsOrResources(eFolderType folderType = eFolderType.Projects)
        {
            if (paratextProjectPath == "")
            {
                GetParatextProjectsPath();
            }

            string searchPath = "";
            if (folderType == eFolderType.Projects)
            {
                searchPath = paratextProjectPath;
            } 
            else if (folderType == eFolderType.Resources)
            {
                searchPath = paratextResourcesPath;
            }

            // look through these folders for files which are called "unique.id"
            string[] directories = Directory.GetDirectories(searchPath);

            List<ParatextProject> projects = new List<ParatextProject>();

            await Task.Run(() =>
            {
                foreach (var directory in directories)
                {
                    // look for settings.xml file
                    string sSettingFilePath = Path.Combine(directory, "settings.xml");

                    // read in settings.xml file
                    if (File.Exists(sSettingFilePath))
                    {
                        ParatextProject.eDirType dirType;
                        if (folderType == eFolderType.Projects)
                        {
                            dirType = ParatextProject.eDirType.Project;
                        }
                        else
                        {
                            dirType = ParatextProject.eDirType.Resources;
                        }

                        var project = GetSettingFileInfo(sSettingFilePath, ParatextProject.eDirType.Project);
                        if (project.FullName != "")
                        {
                            // get the books
                            project.BooksList = GetBookList(project, directory);


                            // check for custom VRS file
                            string sCusteomVRSfilePath = Path.Combine(directory, "custom.vrs");
                            if (File.Exists(sCusteomVRSfilePath))
                            {
                                project.HasCustomVRSfile = true;
                                project.CustomVRSfilePath = sCusteomVRSfilePath;
                            }

                            // read in booknames.xml file
                            string sBookNamesPath = Path.Combine(directory, "booknames.xml");
                            if (File.Exists(sBookNamesPath))
                            {
                                var xmlString = File.ReadAllText(sBookNamesPath);

                                int iRow = 1;
                                // Create an XmlReader
                                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                                {
                                    while (reader.Read())
                                    {
                                        reader.ReadToFollowing("book");
                                        reader.MoveToFirstAttribute();
                                        string code = reader.Value;
                                        reader.MoveToNextAttribute();
                                        string abbr = reader.Value;
                                        reader.MoveToNextAttribute();
                                        string shortname = reader.Value;
                                        reader.MoveToNextAttribute();
                                        string longname = reader.Value;

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

        public List<ParatextProject> GetParatextResources()
        {
            if (paratextResourcesPath == "")
            {
                GetParatextProjectsPath();
            }

            string searchPath = paratextResourcesPath;
            

            // look through these folders for files which are called "unique.id"
            string[] files = Directory.GetFiles(searchPath, "*.p8z");

            List<ParatextProject> projects = new List<ParatextProject>();
            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                projects.Add(new ParatextProject
                {
                    Name = fileInfo.Name.Replace(fileInfo.Extension, ""),
                    ProjectType = ParatextProject.eProjectType.Resource,
                    DirType = ParatextProject.eDirType.Resources
                });
            }

            // alphabetize the list by the short name
            projects.Sort((a, b) => a.Name.CompareTo(b.Name));

            return projects;
        }

        public static List<SelectedBook> UpdateBooks(ParatextProject paratextProject, List<SelectedBook> books)
        {
            for (int i = 0; i < paratextProject.BooksList.Count; i++)
            {
                if (paratextProject.BooksList[i].Available)
                {
                    //Debug.WriteLine(books[i].BookName + " " + paratextProject.BooksList[i].BookNameShort + " AVAILABLE");
                    books[i].IsEnabled = true;
                }
                else
                {
                    //Debug.WriteLine(books[i].BookName + " " + paratextProject.BooksList[i].BookNameShort + " MISSING");
                    books[i].IsEnabled = false;
                    books[i].IsSelected = false;
                }
            }

            return books;
        }

        /// <summary>
        /// Parse through the Settings file and return back a ParatextProject obj
        /// </summary>
        /// <param name="sSettingFilePath"></param>
        /// <param name="dirType"></param>
        /// <returns></returns>
        public ParatextProject GetSettingFileInfo(string sSettingFilePath, ParatextProject.eDirType dirType)
        {
            ParatextProject p = new ParatextProject();

            try
            {
                string xml = File.ReadAllText(sSettingFilePath);
                XmlSerializer serializer = new XmlSerializer(typeof(ScriptureText));
                using (StringReader reader = new StringReader(xml))
                {
                    var obj = (ScriptureText)serializer.Deserialize(reader);
                    if (obj != null)
                    {
                        p.Name = obj.Name;
                        p.Guid = obj.Guid;
                        p.FullName = obj.FullName;
                        p.Copyright = obj.Copyright;
                        p.BooksPresent = obj.BooksPresent;
                        p.FileNameBookNameForm = obj.FileNameBookNameForm;
                        if (obj.FileNamePostPart != null)
                        {
                            p.FileNamePostPart = obj.FileNamePostPart;
                        }
                        if (obj.FileNamePrePart != null)
                        {
                            if (obj.FileNamePrePart is object)
                            {
                                p.FileNamePrePart = "";
                            }
                            else
                            {
                                p.FileNamePrePart = obj.FileNamePrePart.ToString();
                            }
                        }

                        if (obj.NormalizationForm != null)
                        {
                            p.NormalizationForm = obj.NormalizationForm;
                        }

                        if (obj.Language != null)
                        {
                            p.Language = obj.Language;
                        }

                        if (obj.DefaultFont != null)
                        {
                            p.DefaultFont = obj.DefaultFont;
                        }

                        if (obj.Encoding != null)
                        {
                            p.Encoding = obj.Encoding;
                        }

                        if (obj.LanguageIsoCode != null)
                        {
                            p.LanguageIsoCode = obj.LanguageIsoCode;
                        }

                        if (obj.LeftToRight != null)
                        {
                            if (obj.LeftToRight.ToUpper() == "T")
                            {
                                p.IsRTL = true;
                            }
                        }

                        if (obj.TranslationInfo != null)
                        {
                            var split = obj.TranslationInfo.Split(':');
                            if (split.Length >= 3)
                            {
                                ParatextProject.eProjectType projType = GetProjectType(split[0], dirType);

                                p.TranslationInfo = new Translation_Info
                                {
                                    projectType = projType,
                                    projectName = split[1],
                                    projectGuid = split[2],
                                };

                                p.ProjectType = projType;
                            }
                        }

                        if (obj.BaseTranslation != null)
                        {
                            var split = obj.BaseTranslation.Split(':');
                            if (split.Length >= 3)
                            {
                                ParatextProject.eProjectType projType = GetProjectType(split[0], dirType);

                                p.BaseTranslation = new Translation_Info
                                {
                                    projectType = projType,
                                    projectName = split[1],
                                    projectGuid = split[2],
                                };
                            }
                        }

                        p.Versification = obj.Versification;
                        p.ProjectPath = sSettingFilePath;

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return p;
        }

        /// <summary>
        /// convert from string to a project type enum
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dirType"></param>
        /// <returns></returns>
        private ParatextProject.eProjectType GetProjectType(string s, ParatextProject.eDirType dirType)
        {
            s = s.ToUpper().Trim();

            switch (s)
            {
                case "STANDARD":
                    if (dirType == ParatextProject.eDirType.Project)
                    {
                        return ParatextProject.eProjectType.Standard;
                    }
                    else
                    {
                        return ParatextProject.eProjectType.Resource;
                    }
                    break;
                case "BACKTRANSLATION":
                    return ParatextProject.eProjectType.BackTranslation;
                    break;
                case "AUXILIARY":
                    return ParatextProject.eProjectType.Auxiliary;
                    break;
                case "DAUGHTER":
                    return ParatextProject.eProjectType.Daughter;
                    break;
                case "MARBLERESOURCE":
                    return ParatextProject.eProjectType.MarbleResource;
                    break;
            }

            return ParatextProject.eProjectType.Unknown;
        }

        public static List<ParatextBook> GetBookList(ParatextProject project, string directory)
        {
            // initialize the list
            List<ParatextBook> books = new List<ParatextBook>
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

            for (int i = 0; i < project.BooksPresent.Length; i++)
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

                        string sFileName = "";

                        if (project.FileNamePrePart != "System.Object")
                        {
                            sFileName += project.FileNamePrePart;
                        }

                        if (project.FileNameBookNameForm == "41MAT")
                        {
                            sFileName += books[i].USFM_Num.ToString().PadLeft(2, '0') + books[i].BookNameShort;
                        }
                        else if (project.FileNameBookNameForm == "41")
                        {
                            sFileName += books[i].USFM_Num.ToString().PadLeft(2, '0');
                        }
                        else if (project.FileNameBookNameForm == "MAT")
                        {
                            sFileName += books[i].BookNameShort;
                        }

                        if (project.FileNamePostPart != "System.Object")
                        {
                            sFileName += project.FileNamePostPart;
                        }

                        books[i].FilePath = Path.Combine(directory, sFileName);

                        // check if the file exists
                        if (!File.Exists(books[i].FilePath))
                        {
                            // check to see if the file exists with a .usx ending instead
                            sFileName = sFileName.Substring(0, sFileName.LastIndexOf("."));
                            sFileName += ".usx";
                            sFileName = Path.Combine(directory, sFileName);
                            if (File.Exists(sFileName))
                            {
                                books[i].FilePath = sFileName;
                            }
                            else
                            {
                                books[i].FilePath = "";
                                books[i].Available = false;
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }

            return books;
        }


    }
}
