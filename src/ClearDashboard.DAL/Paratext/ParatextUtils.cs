using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ClearDashboard.Common.Models;
using Microsoft.Win32;

namespace ClearDashboard.DAL.Paratext
{
    public class ParatextUtils
    {
        private string paratextProjectPath = "";
        private string paratextInstallPath = "";

        /// <summary>
        /// Returns if paratext is installed on the computer or not
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsParatextInstalledAsync()
        {

            await GetParatextProjectsPath();
            await GetParatextInstallPath();

            if (string.IsNullOrEmpty(paratextProjectPath))
            {
                return false;
            }

            return true;
        }

        private async Task GetParatextInstallPath()
        {
            paratextInstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Paratext9_Full_Release_AppPath", null);

            // check if path exists
            if (!Directory.Exists(paratextInstallPath))
            {
                // file doesn't exist so null this out
                paratextInstallPath = "";
            }
        }

        private async Task GetParatextProjectsPath()
        {
            paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(paratextProjectPath))
            {
                // directory doesn't exist so null this out
                paratextProjectPath = "";
            }
        }

        public List<ParatextProject> GetParatextProjects()
        {
            if (paratextProjectPath == "")
            {
                GetParatextProjectsPath();
            }

            // look through these folders for files which are called "unique.id"
            string[] directories = Directory.GetDirectories(paratextProjectPath);

            List<ParatextProject> projects = new List<ParatextProject>();
            foreach (var directory in directories)
            {

                // look for settings.xml file
                string sSettingFilePath = Path.Combine(directory, "settings.xml");

                Debug.WriteLine(sSettingFilePath);

                if (sSettingFilePath == @"d:\My Paratext 9 Projects\CSPI1\settings.xml")
                {
                    Debug.WriteLine("");
                }

                if (File.Exists(sSettingFilePath))
                {
                    var project = GetSettingFileInfo(sSettingFilePath);
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

                        projects.Add(project);
                    }
                }


            }

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

        public ParatextProject GetSettingFileInfo(string sSettingFilePath)
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
                            p.FileNamePrePart = obj.FileNamePrePart.ToString();
                        }

                        if (obj.LeftToRight != null)
                        {
                            if (obj.LeftToRight.ToUpper() == "F")
                            {
                                p.IsRTL = true;
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
