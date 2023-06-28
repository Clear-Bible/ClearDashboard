using ClearDashboard.DataAccessLayer.Models.Common;
using Microsoft.Win32;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public class ParatextExtractUSFM
    {
        /// <summary>
        /// This method iterates over the tokenized USFM objects and pulls out only the chapter, verse, and
        /// verse text data and saves each USFM book into the user's:
        ///     \MyDocuments\ClearDashboard_Projects\DataFiles\{projectID} directory along with
        /// the settings.xml and the custom versification files
        /// </summary>
        /// <param name="project"></param>
        /// <param name="mainWindow"></param>
        // ReSharper disable once InconsistentNaming
        public UsfmHelper ExportUSFMScripture(IProject project, MainWindow mainWindow)
        {
            List<UsfmError> usfmError = new();

            string exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            exportPath = Path.Combine(exportPath, "ClearDashboard_Projects", "DataFiles", project.ID);

            if (!Directory.Exists(exportPath))
            {
                try
                {
                    Directory.CreateDirectory(exportPath);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(Color.Red, e.Message);
                    return new UsfmHelper
                    {
                        Path = exportPath,
                    };
                }
            }

            // copy over the project's settings file
            string settingsFile = Path.Combine(GetParatextProjectsPath(), project.ShortName, "settings.xml");
            if (File.Exists(settingsFile))
            {
                try
                {
                    File.Copy(settingsFile, Path.Combine(exportPath, "settings.xml"), true);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(Color.Red, e.Message);
                }

                FixParatextSettingsFile(Path.Combine(exportPath, "settings.xml"));

            }
            else
            {
                // TODO create settings file

            }

            // copy over the project's custom versification file
            string versificationFile = Path.Combine(GetParatextProjectsPath(), project.ShortName, "custom.vrs");
            if (File.Exists(versificationFile))
            {
                try
                {
                    File.Copy(versificationFile, Path.Combine(exportPath, "custom.vrs"), true);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(Color.Red, e.Message);
                }
            }

            // copy over project's usfm.sty
            bool bFound = false;
            if (File.Exists(Path.Combine(GetParatextProjectsPath(), project.ShortName, "settings.xml")))
            {
                string stylePath = GetAttributeFromSettingsXML.GetValue(Path.Combine(GetParatextProjectsPath(), project.ShortName, "settings.xml"), "StyleSheet");

                if (stylePath != "")
                {
                    if (stylePath != "usfm.sty") // standard stylesheet
                    {
                        if (File.Exists(Path.Combine(GetParatextProjectsPath(), project.ShortName, stylePath)))
                        {
                            try
                            {
                                File.Copy(Path.Combine(GetParatextProjectsPath(), project.ShortName, stylePath),
                                    Path.Combine(exportPath, "usfm.sty"), true);
                                bFound = true;
                            }
                            catch (Exception e)
                            {
                                mainWindow.AppendText(Color.Red, e.Message);
                            }
                        }
                    }
                }
            }

            if (!bFound)
            {
                // standard stylesheet
                if (File.Exists(Path.Combine(GetParatextProjectsPath(), "usfm.sty")))
                {
                    try
                    {
                        File.Copy(Path.Combine(GetParatextProjectsPath(), "usfm.sty"),
                            Path.Combine(exportPath, "usfm.sty"), true);
                    }
                    catch (Exception e)
                    {
                        mainWindow.AppendText(Color.Red, e.Message);
                    }
                }
            }


            Dictionary<string, string> verseKey = new();

            try
            {
                for (int bookNum = 0; bookNum < project.AvailableBooks.Count; bookNum++)
                {
                    if (BibleBookScope.IsBibleBook(project.AvailableBooks[bookNum].Code))
                    {
                        mainWindow.AppendText(Color.Blue, $"Processing {project.AvailableBooks[bookNum].Code}");

                        StringBuilder sb = new StringBuilder();
                        // do the header
                        sb.AppendLine($@"\id {project.AvailableBooks[bookNum].Code}");

                        int bookFileNum;
                        if (project.AvailableBooks[bookNum].Number >= 40)
                        {
                            // do that crazy USFM file naming where Matthew starts at 41
                            bookFileNum = project.AvailableBooks[bookNum].Number + 1;
                        }
                        else
                        {
                            // normal OT book
                            bookFileNum = project.AvailableBooks[bookNum].Number;
                        }
                        var fileName = bookFileNum.ToString().PadLeft(2, '0')
                                       + project.AvailableBooks[bookNum].Code + ".sfm";

                        IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                        try
                        {
                            // get tokens by book number (from object) and chapter
                            tokens = project.GetUSFMTokens(project.AvailableBooks[bookNum].Number);
                        }
                        catch (Exception)
                        {
                            mainWindow.AppendText(Color.Orange, $"No Scripture for {bookNum}");
                        }

                        string lastChapter = "";
                        bool lastTokenChapter = false;
                        bool lastTokenText = false;
                        bool lastVerseZero = false;
                        string lastVerseRef = "";
                       

                        if (bookNum == 19)
                        {
                            Console.WriteLine();
                        }

                        foreach (var token in tokens)
                        {
                            if (token is IUSFMMarkerToken marker)
                            {
                                // a verse token
                                if (marker.Type == MarkerType.Verse)
                                {
                                    lastTokenText = false;
                                    if (!lastTokenChapter || lastVerseZero)
                                    {
                                        sb.AppendLine();
                                    }
                                    // this includes single verses (\v 1) and multiline (\v 1-3)
                                    if (marker.Data != null)
                                    {
                                        string verseMarker = marker.Data.Trim();
                                        
                                        try
                                        {
                                            bool foundMatch = false;
                                            string key = $"{project.AvailableBooks[bookNum].Number}{lastChapter.PadLeft(3, '0')}{marker.Data.Trim().PadLeft(3, '0')}";

                                            // look for numbers, space, and a dash as being valid
                                            // also match thins like \v 43a
                                            foundMatch = Regex.IsMatch(verseMarker, "^[0-9* -abc]+$");
                                            if (foundMatch)
                                            {
                                                // check to see if the verse ends in '-'
                                                if (marker.Data.Trim().EndsWith("-"))
                                                {
                                                    mainWindow.AppendText(Color.Red, $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} ends in '-'");
                                                    usfmError.Add( new UsfmError
                                                    {
                                                        Reference = UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key),
                                                        Error = $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} ends in '-'",
                                                    });
                                                }
                                                else
                                                {
                                                    sb.Append($@"\v {marker.Data.Trim()} ");

                                                    // check if this has already been entered and is a duplicate
                                                    if (verseKey.ContainsKey(key))
                                                    {
                                                        if (lastVerseRef != marker.VerseRef.ToString())
                                                        {
                                                            mainWindow.AppendText(Color.Red,
                                                                $"Duplicate verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)}");
                                                            usfmError.Add(new UsfmError
                                                            {
                                                                Reference = UsfmReferenceHelper
                                                                    .ConvertBbbcccvvvToReadable(key),
                                                                Error =
                                                                    $"Duplicate verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)}",
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        verseKey.Add(key, key);
                                                        lastVerseRef = marker.VerseRef.ToString();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                mainWindow.AppendText(Color.Red, $"Error with verse numbering in {project.AvailableBooks[bookNum].Code} {lastChapter} [{verseMarker}]");
                                                usfmError.Add(new UsfmError
                                                {
                                                    Reference = marker.VerseRef.ToString(),
                                                    Error = $"Error with verse numbering in {project.AvailableBooks[bookNum].Code} {lastChapter} [{verseMarker}]",
                                                });
                                            }
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            // Syntax error in the regular expression
                                            mainWindow.AppendText(Color.Red, ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        sb.Append($@"\v {marker.Data?.Trim()} ");
                                        mainWindow.AppendText(Color.Red, $"Error with empty verse tag in {project.AvailableBooks[bookNum].Code} {lastChapter}");
                                        usfmError.Add(new UsfmError
                                        {
                                            Reference = $"{project.AvailableBooks[bookNum].Code} {lastChapter }",
                                            Error = $"Error with empty verse tag in {project.AvailableBooks[bookNum].Code} {lastChapter}",
                                        });
                                    }

                                    lastTokenChapter = false;
                                    lastVerseZero = false;
                                }
                                else if (marker.Type == MarkerType.Chapter)
                                {
                                    lastVerseZero = false;
                                    lastTokenText = false;
                                    // new chapter
                                    sb.AppendLine();
                                    sb.AppendLine();
                                    sb.AppendLine(@"\c " + marker.Data);
                                    lastChapter = marker.Data;

                                    //if (lastChapter == "90")
                                    //{
                                    //    Console.WriteLine();
                                    //}

                                    lastTokenChapter = true;
                                }
                            }
                            else if (token is IUSFMTextToken textToken)
                            {
                                if (token.IsScripture)
                                {
                                    // verse text

                                    // check to see if this is a verse zero
                                    if (textToken.VerseRef.VerseNum == 0)
                                    {
                                        if (lastVerseZero == false)
                                        {
                                            sb.Append(@"\v 0 " + textToken.Text);
                                        }
                                        else
                                        {
                                            sb.Append(textToken.Text);
                                        }

                                        lastVerseZero = true;
                                        lastTokenText = true;
                                    }
                                    else
                                    {
                                        // check to see if the last character is a space
                                        if (sb[sb.Length - 1] == ' ' && lastTokenText)
                                        {
                                            sb.Append(textToken.Text.TrimStart());
                                        }
                                        else
                                        {
                                            if (sb[sb.Length - 1] == ' ' && textToken.Text.StartsWith(" "))
                                            {
                                                sb.Append(textToken.Text.TrimStart());
                                            }
                                            else
                                            {
                                                sb.Append(textToken.Text);
                                            }

                                        }

                                        lastTokenText = true;
                                    }
                                }
                            }
                        }

                        // write out to \Documents\ClearDashboard\DataFiles\(project guid)\usfm files
                        try
                        {
                            File.WriteAllText(Path.Combine(exportPath, fileName), sb.ToString());
                        }
                        catch (Exception e)
                        {
                            mainWindow.AppendText(Color.Red, e.Message);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                mainWindow.AppendText(Color.Red, ex.Message);
            }

            return new UsfmHelper
            {
                Path = exportPath, 
                UsfmErrors = usfmError, 
                NumberOfErrors = usfmError.Count
            };
        }

        // update the settings file to use "normal" file extensions
        private void FixParatextSettingsFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            XmlElement root = doc.DocumentElement;
            if (root != null)
            {
                var node = root.SelectSingleNode("//Naming");

                if (node is { Attributes: { } })
                {
                    node.Attributes["PrePart"].Value = "";
                    node.Attributes["PostPart"].Value = ".sfm";
                    node.Attributes["BookNameForm"].Value = "41MAT";
                }


                node = root.SelectSingleNode("//FileNameForm");
                if (node != null)
                {
                    node.InnerText = "41MAT";
                }

                node = root.SelectSingleNode("//FileNameBookNameForm");
                if (node != null)
                {
                    node.InnerText = "41MAT";
                }

                node = root.SelectSingleNode("//FileNamePostPart");
                if (node != null)
                {
                    node.InnerText = ".sfm";
                }

                node = root.SelectSingleNode("//FileNamePrePart");
                if (node != null)
                {
                    node.InnerText = "";
                }
            }

            doc.Save(path);
        }


        /// <summary>
        /// Returns the Paratext Project's path.  Usually:
        /// {drive}:\My Paratext 9 Projects\
        /// </summary>
        /// <returns></returns>
        private string GetParatextProjectsPath()
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

    }
}
