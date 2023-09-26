using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Helpers
{
    public static class ConvertIProjectToParatextProject
    {
        /// <summary>
        /// Build up a project object to send over.  This is only a small portion
        /// of what is available in the m_project object
        /// </summary>
        /// <returns></returns>
        public static DataAccessLayer.Models.ParatextProject BuildParatextProjectFromIProject(IProject givenProject)
        {
            var logger = WebHostStartup.ServiceProvider.GetService<IPluginLogger>();

            var project = new DataAccessLayer.Models.ParatextProject
            {
                Guid = givenProject.ID,
                LanguageName = givenProject.LanguageName,
                ShortName = givenProject.ShortName,
                LongName = givenProject.LongName,
                IsResource = givenProject.IsResource,
            };
            foreach (var users in givenProject.NonObserverUsers)
            {
                project.NonObservers.Add(users.Name);
            }

            foreach (var book in givenProject.AvailableBooks)
            {
                project.AvailableBooks.Add(new BookInfo
                {
                    Code = book.Code,
                    InProjectScope = book.InProjectScope,
                    Number = book.Number,
                });
            }

            project.Language = new ScrLanguageWrapper
            {
                FontFamily = givenProject.Language.Font.FontFamily,
                Size = givenProject.Language.Font.Size,
                IsRtol = givenProject.Language.IsRtoL,
            };

            switch (givenProject.Type)
            {
                case Paratext.PluginInterfaces.ProjectType.Standard:
                    project.Type = DataAccessLayer.Models.ParatextProject.ProjectType.Standard;
                    break;
                default:
                    project.Type = DataAccessLayer.Models.ParatextProject.ProjectType.NotSelected;
                    break;
            }

            // versification
            switch (givenProject.Versification.Type)
            {
                case StandardScrVersType.Unknown:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.Unknown;
                    break;
                case StandardScrVersType.Original:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.Original;
                    break;
                case StandardScrVersType.English:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.English;
                    break;
                case StandardScrVersType.RussianOrthodox:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.RussianOrthodox;
                    break;
                case StandardScrVersType.RussianProtestant:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.RussianProtestant;
                    break;
                case StandardScrVersType.Septuagint:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.Septuagint;
                    break;
                case StandardScrVersType.Vulgate:
                    project.ScrVerseType = SIL.Scripture.ScrVersType.Vulgate;
                    break;

            }

            project.IsCustomVersification = givenProject.Versification.IsCustomized;


            project.BcvDictionary = GetBCV_Dictionary(givenProject, logger);

            return project;
        }

        private static Dictionary<string, string> GetBCV_Dictionary(IProject givenProject, IPluginLogger logger)
        {
            var bcvDict = new Dictionary<string, string>();

            // loop through all the bible books capturing the BBCCCVVV for every verse
            for (var bookNum = 0; bookNum < givenProject.AvailableBooks.Count; bookNum++)
            {
                if (BibleBookScope.IsBibleBook(givenProject.AvailableBooks[bookNum].Code))
                {
                    IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                    try
                    {
                        // get tokens by book number (from object) and chapter
                        tokens = givenProject.GetUSFMTokens(givenProject.AvailableBooks[bookNum].Number);
                    }
                    catch (Exception)
                    {
                        logger.AppendText(Color.Red, $"No Scripture for {bookNum}");
                        //AppendText(MainWindow.MsgColor.Orange, $"No Scripture for {bookNum}");
                    }

                    foreach (var token in tokens)
                    {
                        if (token is IUSFMMarkerToken marker)
                        {
                            // a verse token
                            if (marker.Type == MarkerType.Verse)
                            {
                                string verseID = marker.VerseRef.BBBCCCVVV.ToString().PadLeft(9, '0');
                                if (!bcvDict.ContainsKey(verseID))
                                {
                                    bcvDict.Add(verseID, verseID);
                                }
                            }
                        }
                    }
                }
            }
            return bcvDict;
        }
    }
}
