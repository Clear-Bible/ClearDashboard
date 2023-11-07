using ClearDashboard.DAL.CQRS;

using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class AddNoteCommandHandler : IRequestHandler<AddNoteCommand, RequestResult<ExternalNote>>
    {
        private readonly IPluginHost _host;
        private readonly MainWindow _mainWindow;
        private readonly IPluginChildWindow _parent;
        private readonly ILogger<AddNoteCommandHandler> _logger;

        public AddNoteCommandHandler(IPluginHost host, MainWindow mainWindow, IPluginChildWindow parent, ILogger<AddNoteCommandHandler> logger)
        {
            _host = host;
            _mainWindow = mainWindow;
            _parent = parent;
            _logger = logger;
        }
        public Task<RequestResult<ExternalNote>> Handle(AddNoteCommand request, CancellationToken cancellationToken)
        {
            if (request.Data.ExternalProjectId.Equals(string.Empty))
            {
                throw new Exception($"externalprojectid is not set");
            }
            IProject project;
            try
            {
                project = _host.GetAllProjects(true)
                    .Where(p => p.ID.Equals(request.Data.ExternalProjectId))
                    .First();
            }
            catch (Exception)
            {
                throw new Exception($"externalprojectid {request.Data.ExternalProjectId} not found");
            }

            IVerseRef verseRef;
            if (request.Data.Book != -1 && request.Data.Chapter != -1 && request.Data.Verse != -1)
            {
                verseRef = project.Versification.CreateReference(request.Data.Book, request.Data.Chapter, request.Data.Verse);
            }
            else
            {
                verseRef = _parent.CurrentState.VerseRef;
            }

            using var writeLock = project.RequestWriteLock(_mainWindow, WriteLockReleaseRequested, WriteLockScope.ProjectNotes);
            if (writeLock == null)
            {
                throw new Exception("Couldn't obtain write lock required to add note.");
            }
            else
            {
                IScriptureTextSelection anchor = null;
                if (request.Data.SelectedText == string.Empty)
                {
                    anchor = project.GetScriptureSelectionForVerse(verseRef);
                }
                else
                {
                    IReadOnlyList<IScriptureTextSelection> anchors;

                    anchors = project.FindMatchingScriptureSelections(verseRef, request.Data.SelectedText, wholeWord: false);
                    if (anchors.Count != 0)
                    {
                        if (anchors.Count() > request.Data.OccuranceIndexOfSelectedTextInVerseText)
                            anchor = anchors[request.Data.OccuranceIndexOfSelectedTextInVerseText];
                        else
                            throw new Exception($"Couldn't find match for occurance index {request.Data.OccuranceIndexOfSelectedTextInVerseText} of selected text {request.Data.SelectedText}");
                    }
                }
                if (anchor == null)
                {
                    throw new Exception("Cannot find matching text in verse. Note not added to Paratext.");
                }
                List<CommentParagraph> commentParagraphs = new List<CommentParagraph>();
                foreach (var paragraph in request.Data.NoteParagraphs)
                {
                    commentParagraphs.Add(new CommentParagraph(new FormattedString(paragraph)));
                }

                var noteAdded = project.AddNote(writeLock, anchor, commentParagraphs, assignedUser: new UserInfo(request.Data.UserName));
                return Task.FromResult(new RequestResult<ExternalNote>(noteAdded.GetExternalNote(project, _logger)));
            }
        }
        private void WriteLockReleaseRequested(IWriteLock obj)
        {
            //throw new NotImplementedException();
            _logger.LogError("WriteLockReleaseRequested Hit", obj);
        }
    }
}
