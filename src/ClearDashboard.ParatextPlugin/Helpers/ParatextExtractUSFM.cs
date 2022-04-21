using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClearDashboardPlugin;
using Microsoft.Win32;
using Paratext.PluginInterfaces;

namespace ClearDashboard.ParatextPlugin.Helpers
{
    public class ParatextExtractUSFM
    {
        public void ExportUSFMScripture(IProject m_project, MainWindow mainWindow)
        {
            string exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            exportPath = Path.Combine(exportPath, "ClearDashboard_Projects", "DataFiles", m_project.ID);

            if (!Directory.Exists(exportPath))
            {
                try
                {
                    Directory.CreateDirectory(exportPath);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(MainWindow.MsgColor.Red, e.Message);
                    return;
                }
            }

            // copy over the project's settings file
            string settingsFile = Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "settings.xml");
            if (File.Exists(settingsFile))
            {
                try
                {
                    File.Copy(settingsFile, Path.Combine(exportPath, "settings.xml"), true);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(MainWindow.MsgColor.Red, e.Message);
                }

                FixParatextSettingsFile(Path.Combine(exportPath, "settings.xml"));

            }

            // copy over the project's custom versification file
            string versificationFile = Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "custom.vrs");
            if (File.Exists(versificationFile))
            {
                try
                {
                    File.Copy(versificationFile, Path.Combine(exportPath, "custom.vrs"), true);
                }
                catch (Exception e)
                {
                    mainWindow.AppendText(MainWindow.MsgColor.Red, e.Message);
                }
            }

            // copy over project's usfm.sty
            string stylePath = GetAttributeFromSettingsXML.GetValue(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, "settings.xml"), "StyleSheet");
            bool bFound = false;
            if (stylePath != "")
            {
                if (stylePath != "usfm.sty") // standard stylesheet
                {
                    if (File.Exists(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, stylePath)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(GetParatextProjectsPath(), m_project.ShortName, stylePath),
                                Path.Combine(exportPath, "usfm.sty"), true);
                        }
                        catch (Exception e)
                        {
                            mainWindow.AppendText(MainWindow.MsgColor.Red, e.Message);
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
                        mainWindow.AppendText(MainWindow.MsgColor.Red, e.Message);
                    }
                }
            }


            for (int bookNum = 0; bookNum < m_project.AvailableBooks.Count; bookNum++)
            {
                if (BibleBookScope.IsBibleBook(m_project.AvailableBooks[bookNum].Code))
                {
                    if (m_project.AvailableBooks[bookNum].Code == "PSA")
                    {
                        Console.WriteLine();
                    }


                    mainWindow.AppendText(MainWindow.MsgColor.Blue, $"Processing {m_project.AvailableBooks[bookNum].Code}");

                    StringBuilder sb = new StringBuilder();
                    // do the header
                    sb.AppendLine($@"\id {m_project.AvailableBooks[bookNum].Code}");

                    int bookFileNum = 0;
                    if (m_project.AvailableBooks[bookNum].Number >= 40)
                    {
                        // do that crazy USFM file naming where Matthew starts at 41
                        bookFileNum = m_project.AvailableBooks[bookNum].Number + 1;
                    }
                    else
                    {
                        // normal OT book
                        bookFileNum = m_project.AvailableBooks[bookNum].Number;
                    }
                    var fileName = bookFileNum.ToString().PadLeft(2, '0')
                                   + m_project.AvailableBooks[bookNum].Code + ".sfm";

                    IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                    try
                    {
                        // get tokens by book number (from object) and chapter
                        tokens = m_project.GetUSFMTokens(m_project.AvailableBooks[bookNum].Number);
                    }
                    catch (Exception)
                    {
                        mainWindow.AppendText(MainWindow.MsgColor.Orange, $"No Scripture for {bookNum}");
                    }


                    bool lastTokenChapter = false;
                    bool lastTokenText = false;
                    bool lastVerseZero = false;
                    foreach (var token in tokens)
                    {
                        if (token is IUSFMMarkerToken marker)
                        {
                            // a verse token
                            if (marker.Type == MarkerType.Verse)
                            {
                                lastTokenText = false;
                                // ReSharper disable once NotAccessedVariable
                                int p = 0;
                                bool result = int.TryParse(marker.Data, out p);

                                if (result)
                                {
                                    if (!lastTokenChapter || lastVerseZero)
                                    {
                                        sb.AppendLine();
                                    }
                                    sb.Append($@"\v {marker.Data} ");
                                }
                                else
                                {
                                    // verse span so bust up the verse span
                                    string[] nums = marker.Data.Split('-');
                                    if (nums.Length > 1)
                                    {
                                        if (int.TryParse(nums[0], out p))
                                        {
                                            if (int.TryParse(nums[1], out p))
                                            {
                                                int start = Convert.ToInt16(nums[0]);
                                                int end = Convert.ToInt16(nums[1]);
                                                for (int j = start; j < end + 1; j++)
                                                {
                                                    if (!lastTokenChapter)
                                                    {
                                                        sb.AppendLine();
                                                    }
                                                    sb.Append($@"\v {j} ");
                                                }
                                            }
                                        }
                                    }
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
                                //sb.AppendLine();

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
                                        sb.Append($@"\v 0 " + textToken.Text);
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
                                        sb.Append(textToken.Text);
                                    }

                                    lastTokenText = true;
                                }
                            }
                        }
                    }

                    // write out to \Documents\ClearDashboard\DataFiles\(project guid)\usfm files
                    File.WriteAllText(Path.Combine(exportPath, fileName), sb.ToString());
                }
            }
        }

        // update the settings file to use "normal" file extensions
        private void FixParatextSettingsFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            XmlElement root = doc.DocumentElement;
            var node = root.SelectSingleNode("//Naming");

            if (node != null)
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
