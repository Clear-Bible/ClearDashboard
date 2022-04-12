using ClearDashboard.Common.Models;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClearDashboard.DataAccessLayer.Paratext
{
    public class ExtractVersesFromChapter
    {
        /// <summary>
        /// Extracts a list of verses from a chapter given a stylesheet and
        /// path to a usfm document
        /// </summary>
        /// <param name="styleSheetPath">file path to the usfm stylesheet</param>
        /// <param name="usfmBookPath">file path to the usfm file</param>
        /// <param name="verse">the verse from which we want to extract the chapter</param>
        /// <param name="paratextProject"></param>
        /// <returns></returns>
        public static List<string> ParseUSFM(ILogger logger, ParatextProject project, Verse verse)
        {
            ParatextUtils paratextUtils = new ParatextUtils(logger as ILogger<ParatextUtils>);
            string projectPath = "";
            if (paratextUtils.IsParatextInstalled())
            {
                projectPath = paratextUtils.ParatextProjectPath;
            }

            var stylesheetPath = GetStyleSheetPath(logger, project);
            string usfmBookPath = GetUsfmBookPath(project, verse, projectPath);

            if (usfmBookPath == String.Empty || stylesheetPath == String.Empty)
            {
                return new List<string>();
            }

            List<string> lines = new List<string>();
            UsfmStylesheet usfmStylesheet = new UsfmStylesheet(stylesheetPath, null);
            UsfmParser parser = new UsfmParser(usfmStylesheet);

            using (TextReader reader = File.OpenText(usfmBookPath))
            {
                string usfm = reader.ReadToEnd();
                var tokens = parser.Parse(usfm, false);

                UsfmTokenType lastTokeType = UsfmTokenType.Unknown;
                foreach (var token in tokens)
                {
                    
                    if (token.Type is UsfmTokenType marker)
                    {
                        switch (marker)
                        {
                            case UsfmTokenType.Verse:
                                lines.Add($@"\v {token.Text} ");
                                break;
                            case UsfmTokenType.Book:
                                //lines.Add($"{token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.Chapter:
                                //lines.Add($@"\c {token.Text}");
                                break;
                            case UsfmTokenType.Character:
                                //lines.Add($"Marker Character: {token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.End:
                                //lines.Add($"Marker End: {token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.Note:
                                //lines.Add($"Marker Note: {token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.Unknown:
                                //lines.Add($"Marker Unknown: {token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.Paragraph:
                                //lines.Add($" {token.Marker} {token.Text}");
                                break;
                            case UsfmTokenType.Text:
                                if (lines.Count > 0)
                                {
                                    if (lines[lines.Count - 1].StartsWith(@"\v "))
                                    {
                                        lines[lines.Count - 1] += token.Text;
                                    }
                                }
                                break;
                        }
                    }
                }

                // get all the chapter line numbers
                Dictionary<int, int> chapters = new Dictionary<int, int>();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith(@"\c"))
                    {
                        // get the chapter reference & line number
                        var parts = lines[i].Split(' ');
                        if (parts.Length == 2)
                        {
                            string numPart = parts[1].Trim();
                            if (IsNumeric(numPart))
                            {
                                int chapNum = Convert.ToInt32(numPart);

                                if (!chapters.ContainsKey(chapNum))
                                {
                                    chapters.Add(chapNum, i);
                                }
                            }
                        }
                    }
                }

                string targetChapterNum = verse.ChapterStr;
                int targetChapNum = Convert.ToInt32(targetChapterNum);

                if (chapters.Count > 0)
                {
                    // remove lines after chapter
                    if (chapters.ContainsKey(targetChapNum + 1))
                    {
                        lines.RemoveRange(chapters[targetChapNum + 1], lines.Count - chapters[targetChapNum + 1]);
                    }

                    //remove lines from the start
                    lines.RemoveRange(0, chapters[targetChapNum]);
                }
            }
            return lines;
        }

        private static string GetStyleSheetPath(ILogger logger, ParatextProject project)
        {
            // get the standard Paratext one
            string stylesheetPath = "";
            ParatextUtils paratextUtils = new ParatextUtils(logger as ILogger<ParatextUtils>);
            if (paratextUtils.IsParatextInstalled())
            {
                var projectPath = paratextUtils.ParatextProjectPath;
                var standardStyleSheet = Path.Combine(projectPath, "usfm.sty");
                if (File.Exists(standardStyleSheet))
                {
                    stylesheetPath = standardStyleSheet;
                }
            }
            return stylesheetPath;
        }

        private static string GetUsfmBookPath(ParatextProject project, Verse verse, string projectPath)
        {
            var book = verse.BookStr;
            // get the file name for that book
            var bookFile = project.BooksList.Where(b => b.BookId == verse.BookStr).FirstOrDefault();

            if (bookFile is null)
            {
                return string.Empty;
            }

            var path = Path.Combine(projectPath, project.Name, bookFile.FilePath);
            return path;
        }

        public static bool IsNumeric(string input)
        {
            return int.TryParse(input, out _);
        }
    }
}
