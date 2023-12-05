using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Paratext.PluginInterfaces;
using System.Diagnostics;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class ParatextHelpers
    {
        private static string _fileNameBookNameForm = string.Empty;
        private static string _fileNamePrePart = string.Empty;
        private static string _fileNamePostPart = string.Empty;

        /// <summary>
        /// Returns the directory path to where the Paratext project resides
        /// </summary>
        /// <returns></returns>
        public static string GetParatextProjectsPath()
        {
            string paratextProjectPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Paratext\8", "Settings_Directory", null);
            // check if directory exists
            if (!Directory.Exists(paratextProjectPath))
            {
                // directory doesn't exist so null this out
                paratextProjectPath = "";
            }

            return paratextProjectPath;
        }



        public static List<ParatextBook> GetUsfmFilePaths(string projectDirectory, IProject project)
        {
            var list = new List<ParatextBook>();

            GetProjectSettingsXml(projectDirectory);

            // get the list of files in the directory to return the file paths
            foreach (var book in project.AvailableBooks)
            {
                // build the file name
                string fileName;

                if (_fileNameBookNameForm == "41MAT")
                {
                    if (book.Number >= 40)
                    {
                        fileName = $"{_fileNamePrePart}{(book.Number + 1).ToString().PadLeft(2, '0')}{book.Code}{_fileNamePostPart}";
                    }
                    else
                    {
                        fileName = $"{_fileNamePrePart}{book.Number.ToString().PadLeft(2, '0')}{book.Code}{_fileNamePostPart}";
                    }
                }
                else if (_fileNameBookNameForm == "MAT")
                {
                    fileName = $"{_fileNamePrePart}{book.Code}{_fileNamePostPart}";
                }
                else
                {
                    if (book.Number >= 40)
                    {
                        fileName = $"{_fileNamePrePart}{(book.Number + 1).ToString().PadLeft(2, '0')}{_fileNamePostPart}";
                    }
                    else
                    {
                        fileName = $"{_fileNamePrePart}{book.Number.ToString().PadLeft(2, '0')}{_fileNamePostPart}";
                    }
                }


                Debug.WriteLine($"book.Number: {book.Number}");
                Debug.WriteLine($"book.Code: {book.Code}");
                Debug.WriteLine($"fileName: {fileName}");
                Debug.WriteLine("");



                // get the file path
                var filePath = Path.Combine(projectDirectory, fileName);
                // check if the file exists
                if (File.Exists(filePath))
                {
                    // add the file path to the list
                    var entry = new ParatextBook
                    {
                        Available = true,
                        BookId = book.Number.ToString().PadLeft(2, '0'),
                        BookNameShort = book.Code,
                        FilePath = filePath,
                        USFM_Num = book.Number
                    };

                    list.Add(entry);
                }
                else
                {
                    // add an empty string to the list as the file does not exist
                    var entry = new ParatextBook
                    {
                        Available = false,
                        BookId = book.Number.ToString().PadLeft(2, '0'),
                        BookNameShort = book.Code,
                        FilePath = string.Empty,
                        USFM_Num = book.Number
                    };
                    list.Add(entry);
                }



            }
            return list;
        }

        private static void GetProjectSettingsXml(string projectDirectory)
        {
            var files = new List<string>();

            // get the settings file
            var settingsFile = Path.Combine(projectDirectory, "settings.xml");
            if (File.Exists(settingsFile))
            {
                var xmlStr = File.ReadAllText(settingsFile);
                var str = XElement.Parse(xmlStr);

                // get the data using the <Naming> element
                var prePart = str.Elements("Naming").Attributes("PrePart").FirstOrDefault();
                var postPart = str.Elements("Naming").Attributes("PostPart").FirstOrDefault();
                var bookNameForm = str.Elements("Naming").Attributes("BookNameForm").FirstOrDefault();

                if (bookNameForm is not null)
                {
                    _fileNameBookNameForm = bookNameForm.Value;
                    _fileNamePrePart = prePart.Value;
                    _fileNamePostPart = postPart.Value;
                }
                else
                {
                    // get the data using the other element names
                    _fileNamePostPart = str.Elements("FileNamePostPart").FirstOrDefault().Value;
                    _fileNamePrePart = str.Elements("FileNamePrePart").FirstOrDefault().Value;
                    _fileNameBookNameForm = str.Elements("FileNameBookNameForm").FirstOrDefault().Value;
                }
            }
        }

    }
}
