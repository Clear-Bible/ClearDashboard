using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.WebApiParatextPlugin.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;


namespace ClearDashboard.WebApiParatextPlugin.Features.Project
{
    public class GetCurrentProjectQueryHandler : IRequestHandler<GetCurrentProjectQuery, RequestResult<DataAccessLayer.Models.ParatextProject>>
    {
        private readonly IProject _project;
        private readonly ILogger<GetCurrentProjectQueryHandler> _logger;

        public GetCurrentProjectQueryHandler(IProject project, ILogger<GetCurrentProjectQueryHandler> logger)
        {
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<DataAccessLayer.Models.ParatextProject>> Handle(GetCurrentProjectQuery request, CancellationToken cancellationToken)
        {
            var project = BuildProject();
            var result = new RequestResult<DataAccessLayer.Models.ParatextProject>(project);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Build up a project object to send over.  This is only a small portion
        /// of what is available in the m_project object
        /// </summary>
        /// <returns></returns>
        private DataAccessLayer.Models.ParatextProject BuildProject()
        {
            var project = new DataAccessLayer.Models.ParatextProject
            {
                Guid = _project.ID,
                LanguageName = _project.LanguageName,
                ShortName = _project.ShortName,
                LongName = _project.LongName,
                IsResource = _project.IsResource,
            };
            foreach (var users in _project.NonObserverUsers)
            {
                project.NonObservers.Add(users.Name);
            }

            foreach (var book in _project.AvailableBooks)
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
                FontFamily = _project.Language.Font.FontFamily,
                Size = _project.Language.Font.Size,
                IsRtol = _project.Language.IsRtoL,
            };

            switch (_project.Type)
            {
                case Paratext.PluginInterfaces.ProjectType.Standard:
                    project.Type = DataAccessLayer.Models.ParatextProject.ProjectType.Standard;
                    break;
                default:
                    project.Type = DataAccessLayer.Models.ParatextProject.ProjectType.NotSelected;
                    break;
            }

            // versification
            switch (_project.Versification.Type)
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

            project.IsCustomVersification = _project.Versification.IsCustomized;


            project.BcvDictionary = GetBCV_Dictionary();

            return project;
        }

        private Dictionary<string, string> GetBCV_Dictionary()
        {
            var bcvDict = new Dictionary<string, string>();

            // loop through all the bible books capturing the BBCCCVVV for every verse
            for (var bookNum = 0; bookNum < _project.AvailableBooks.Count; bookNum++)
            {
                if (BibleBookScope.IsBibleBook(_project.AvailableBooks[bookNum].Code))
                {
                    IEnumerable<IUSFMToken> tokens = new List<IUSFMToken>();
                    try
                    {
                        // get tokens by book number (from object) and chapter
                        tokens = _project.GetUSFMTokens(_project.AvailableBooks[bookNum].Number);
                    }
                    catch (Exception)
                    {
                        _logger.LogError($"No Scripture for {bookNum}");
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
