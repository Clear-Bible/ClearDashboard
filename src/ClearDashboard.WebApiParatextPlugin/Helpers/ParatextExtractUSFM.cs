﻿using ClearDashboard.DataAccessLayer.Models.Common;
using Icu;
using Microsoft.Win32;
using Paratext.PluginInterfaces;
using SIL.Machine.FiniteState;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
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
            string settingsFile = Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, "settings.xml");
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
            string versificationFile = Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, "custom.vrs");
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
            if (File.Exists(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, "settings.xml")))
            {
                string stylePath = GetAttributeFromSettingsXML.GetValue(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, "settings.xml"), "StyleSheet");

                if (stylePath != "")
                {
                    if (stylePath != "usfm.sty") // standard stylesheet
                    {
                        if (File.Exists(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, stylePath)))
                        {
                            try
                            {
                                File.Copy(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), project.ShortName, stylePath),
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
                if (File.Exists(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), "usfm.sty")))
                {
                    try
                    {
                        File.Copy(Path.Combine(ParatextHelpers.GetParatextProjectsPath(), "usfm.sty"),
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

                        // do the actual parsing of the USFM tokens
                        var verses = ParseUsfmBook(project, mainWindow, project.AvailableBooks[bookNum].Number, usfmError, verseKey);

                        string chapter = "";
                        foreach (var verse in verses)
                        {
                            if (verse.Chapter != chapter)
                            {
                                sb.AppendLine();
                                sb.AppendLine(@"\c " + verse.Chapter);
                                chapter = verse.Chapter;
                            }
                            
                            sb.AppendLine(@"\v " + verse.Verse + " " + verse.Text);
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

        /// <summary>
        /// This method iterates over the tokenized USFM objects and pulls out only the chapter, verse, and verse text data
        /// </summary>
        /// <param name="project"></param>
        /// <param name="mainWindow"></param>
        /// <param name="bookNum"></param>
        /// <param name="sb"></param>
        /// <param name="usfmError"></param>
        /// <param name="verseKey"></param>
        /// <returns></returns>
        public static List<UsfmVerse> ParseUsfmBook(IProject project, MainWindow mainWindow, int bookNum, List<UsfmError> usfmError, Dictionary<string, string> verseKey)
        {
            var verses = new List<UsfmVerse>();

            IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
            try
            {
                // get tokens by book number (from object) and chapter
                var selectedProject = project.AvailableBooks.FirstOrDefault(b => b.Number == bookNum);
                tokens = project.GetUSFMTokens(bookNum);
            }
            catch (Exception)
            {
                mainWindow.AppendText(Color.Orange, $"No Scripture for {bookNum}");
            }


            var chapter = "";
            var verse = "";
            var verseText = "";

            string lastChapter = "";
            bool lastTokenChapter = false;
            bool lastTokenText = false;
            bool lastVerseZero = false;
            var previousTokenWasCp = false;

            foreach (var token in tokens)
            {
                if (previousTokenWasCp)
                {
                    previousTokenWasCp = false;
                    continue;
                }


                if (token is IUSFMMarkerToken marker)
                {
                    // a verse token
                    if (marker.Type == MarkerType.Verse)
                    {
                        lastTokenText = false;

                        if (lastVerseZero)
                        {
                            var usfm = new UsfmVerse
                            {
                                Chapter = chapter,
                                Verse = "0",
                                Text = verseText,
                            };
                            verses.Add(usfm);
                            verseText = "";
                        }


                        // this includes single verses (\v 1) and multiline (\v 1-3)
                        // \v 2,3 and \v 2b-4d are OK so is \v 2ബി-4ഡി (Malayalam)
                        if (marker.Data != null)
                        {
                            string verseMarker = marker.Data.Trim();

                            try
                            {
                                bool foundMatch = false;
                                string key = $"{bookNum.ToString().PadLeft(3, '0')}{lastChapter.PadLeft(3, '0')}{marker.Data.Trim().PadLeft(3, '0')}";
                                // look for numbers, space, and a dash as being valid
                                // also match thins like \v 43a
                                //foundMatch = Regex.IsMatch(verseMarker, "^[0-9* -abc]+$");  // original regex
                                foundMatch = Regex.IsMatch(verseMarker, @"[0-9]+[\p{L}\p{Mn}]*(\u200F?[\-,][0-9]+[\p{L}\p{Mn}]*)*");  // new regex from Kent Spielman that handles RTL characters

                                if (foundMatch)
                                {
                                    // check to see if the verse ends in '-'
                                    if (marker.Data.Trim().EndsWith("-"))
                                    {
                                        mainWindow.AppendText(Color.Red, $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} ends in '-'");
                                        usfmError.Add(new UsfmError
                                        {
                                            Reference = UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key),
                                            Error = $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} ends in '-'",
                                        });
                                    }
                                    else // check to see if the verse starts with '-'
                                    if (marker.Data.Trim().StartsWith("-"))
                                    {
                                        mainWindow.AppendText(Color.Red, $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} starts with '-'");
                                        usfmError.Add(new UsfmError
                                        {
                                            Reference = UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key),
                                            Error = $"Verse {UsfmReferenceHelper.ConvertBbbcccvvvToReadable(key)} starts with '-'",
                                        });
                                    }
                                    else
                                    {
                                        if (chapter != "" && verse != "" && verseText != "")
                                        {
                                            var usfm = new UsfmVerse
                                            {
                                                Chapter = chapter,
                                                Verse = verse,
                                                Text = verseText,
                                            };

                                            verses.Add(usfm);

                                            verseText = "";
                                        }


                                        // check if this has already been entered and is a duplicate
                                        if (verseKey.ContainsKey(key))
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
                                        else
                                        {
                                            verseKey.Add(key, key);
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

                                verse = marker.Data.Trim();
                                lastTokenChapter = false;
                                lastVerseZero = false;
                            }
                            catch (ArgumentException ex)
                            {
                                // Syntax error in the regular expression
                                mainWindow.AppendText(Color.Red, ex.Message);
                            }
                        }
                        else
                        {
                            mainWindow.AppendText(Color.Red, $"Error with empty verse tag in {project.AvailableBooks[bookNum].Code} {lastChapter}");
                            usfmError.Add(new UsfmError
                            {
                                Reference = $"{project.AvailableBooks[bookNum].Code} {lastChapter}",
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
                        if (chapter != "" && verse != "" && verseText != "")
                        {
                            var usfm = new UsfmVerse
                            {
                                Chapter = chapter,
                                Verse = verse,
                                Text = verseText,
                            };

                            verses.Add(usfm);
                        }

                        chapter = marker.Data.Trim();  
                        verse = "";
                        lastChapter = marker.Data.Trim();
                        verseText = "";

                        lastTokenChapter = true;

                        if (marker.Marker == "cp")
                        {
                            previousTokenWasCp = true;
                        }
                    }
                }
                else if (token is IUSFMTextToken textToken)
                {
                    if (token.IsScripture && token.IsSpecial == false)
                    {
                        // verse text

                        // check to see if this is a verse zero
                        if (textToken.VerseRef.VerseNum == 0)
                        {
                            if (lastVerseZero == false)
                            {
                                verseText = textToken.Text;
                            }
                            else
                            {
                                verseText = textToken.Text;
                            }

                            lastVerseZero = true;
                            lastTokenText = true;
                        }
                        else
                        {
                            // check to see if the last character is a space
                            if (verseText.EndsWith(" ") && lastTokenText)
                            {
                                verseText += textToken.Text.TrimStart();
                            }
                            else
                            {
                                if (verseText.EndsWith(" ") && textToken.Text.StartsWith(" "))
                                {
                                    verseText = textToken.Text.TrimStart();
                                }
                                else
                                {
                                    verseText += textToken.Text;
                                }

                            }

                            lastTokenText = true;
                        }
                    }
                }
            }

            // do the last verse
            if (chapter != "" && verse != "" && verseText != "")
            {
                var usfm = new UsfmVerse
                {
                    Chapter = chapter,
                    Verse = verse,
                    Text = verseText,
                };

                verses.Add(usfm);
            }

            // fix split verses (e.g. \v 2a, \v 2b, \v 2c, etc.)
            verses = ParatextExtractUSFM.FixSplitVerses(verses);


            //foreach (var v in verses)
            //{
            //    Console.WriteLine($"{v.Chapter}:{v.Verse} {v.Text}");
            //}

            return verses;
        }

        /// <summary>
        /// This method iterates over the verses looking for verses that are split over multiple lines
        /// e.g. \v 2a, \v 2b, \v 2c, etc. and combines them into a single line
        /// </summary>
        /// <param name="usfmFile"></param>
        //private static void FixSplitVerses(string usfmFile)
        //{
        //    List<string> output = new();
        //    // read in the file
        //    string[] lines = File.ReadAllLines(usfmFile);

        //    Dictionary<string, string> verseKey = new();
        //    string chapter = "";
        //    bool firstHalfVerse = false;

        //    foreach (string line in lines)
        //    {
        //        if (line.StartsWith(@"\c "))
        //        {
        //            // chapter
        //            string[] parts = line.Split(' ');
        //            chapter = parts[1].Trim().PadLeft(3, ' ');
        //            firstHalfVerse = false;
        //            output.Add(line);
        //        }
        //        else if (line.StartsWith(@"\v "))
        //        {
        //            // verse
        //            string[] parts = line.Split(' ');
        //            string verse = parts[1].Trim();

        //            // check if verse is a number
        //            bool isNumber = double.TryParse(verse, out _);
        //            string key = "";
        //            if (isNumber)
        //            {
        //                key = $"{chapter}.{verse}";
        //                output.Add(line);
        //            }
        //            else
        //            {
        //                if (verse.Contains("-"))
        //                {
        //                    // let this pass normally
        //                    key = $"{chapter}.{verse}";
        //                    output.Add(line);
        //                }
        //                else
        //                {
        //                    if (firstHalfVerse)
        //                    {
        //                        // append onto the previous verse since we are now the second verse
        //                        if (output[output.Count - 1].EndsWith(" "))
        //                        {
        //                            output[output.Count - 1] += line.Substring((parts[0] + " " + parts[1]).Length);
        //                        }
        //                        else
        //                        {
        //                            output[output.Count - 1] += " " + line.Substring((parts[0] + " " + parts[1]).Length);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // first half of the verse
        //                        firstHalfVerse = true;
        //                        var newVerse = @"\v " + StringHelpers.RemoveNonNumeric(verse) + line.Substring((parts[0] + " " + parts[1]).Length);
        //                        output.Add(newVerse);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // not a chapter or verse line
        //            output.Add(line);
        //        }
        //    }

        //    File.WriteAllLines(usfmFile, output);
        //}


        /// <summary>
        /// This method iterates over the verses looking for verses that are split over multiple lines
        /// e.g. \v 2a, \v 2b, \v 2c, etc. and combines them into a single line
        /// </summary>
        /// <param name="usfmFile"></param>
        public static List<UsfmVerse> FixSplitVerses(List<UsfmVerse> verses)
        {
            List<UsfmVerse> usfmVerses = new();

            Dictionary<string, string> verseKey = new();
            string chapter = "";
            bool firstHalfVerse = false;
            foreach (var v in verses)
            {
                //if (v.Verse == "1-2" && v.Chapter == "10")
                //{
                //    Console.WriteLine();
                //}

                if (v.Chapter != chapter)
                {
                    // new chapter
                    chapter = v.Chapter;
                    firstHalfVerse = false;
                    bool isNumber = double.TryParse(v.Verse, out _);
                    if (isNumber == false)
                    {
                        // first half of the verse
                        firstHalfVerse = true;

                        // let hyphenated verses pass by
                        if (v.Verse.Contains("-") == false)
                        {
                            v.Verse = StringHelpers.RemoveNonNumeric(v.Verse);
                        }
                    }
                    usfmVerses.Add(v);
                }
                else
                {
                    // verse

                    // check if verse is a number
                    bool isNumber = double.TryParse(v.Verse, out _);
                    string key = "";
                    if (isNumber)
                    {
                        key = $"{chapter}.{v.Verse}";
                        usfmVerses.Add(v);
                    }
                    else
                    {
                        if (v.Verse.Contains("-"))
                        {
                            // let this pass normally
                            key = $"{chapter}.{v.Verse}";
                            usfmVerses.Add(v);
                        }
                        else
                        {
                            if (firstHalfVerse)
                            {
                                // append onto the previous verse since we are now the second verse
                                if (usfmVerses[usfmVerses.Count - 1].Text.EndsWith(" "))
                                {
                                    usfmVerses[usfmVerses.Count - 1].Text += v.Text;
                                }
                                else
                                {
                                    usfmVerses[usfmVerses.Count - 1].Text += " " + v.Text;
                                }
                            }
                            else
                            {
                                // first half of the verse
                                firstHalfVerse = true;

                                var usfm = new UsfmVerse
                                {
                                    Chapter = chapter,
                                    Verse = StringHelpers.RemoveNonNumeric(v.Verse),
                                    Text = v.Text,
                                };

                                usfmVerses.Add(usfm);
                            }
                        }
                    }
                }
            }

            return usfmVerses;
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
    }
}
