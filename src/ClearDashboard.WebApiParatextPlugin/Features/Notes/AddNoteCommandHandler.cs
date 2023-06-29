using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Forms;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using ClearDashboard.WebApiParatextPlugin.Helpers;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class AddNoteCommandHandler : IRequestHandler<AddNoteCommand, RequestResult<IProjectNote>>
    {
        private readonly IPluginHost _host;
        private readonly IProject _project;
        private readonly MainWindow _mainWindow;
        private readonly IPluginChildWindow _parent;
        private readonly ILogger<AddNoteCommandHandler> _logger;

        public AddNoteCommandHandler(IPluginHost host, IProject project, MainWindow mainWindow, IPluginChildWindow parent, ILogger<AddNoteCommandHandler> logger)
        {
            _host = host;
            _project = project;
            _mainWindow = mainWindow;
            _parent = parent;
            _logger = logger;
        }
        public Task<RequestResult<IProjectNote>> Handle(AddNoteCommand request, CancellationToken cancellationToken)
        {
            if (request.Data.ParatextProjectId.Equals(string.Empty))
            {
                throw new Exception($"paratextprojectid is not set");
            }
            IProject project;
            try
            {
                project = _host.GetAllProjects(true)
                    .Where(p => p.ID.Equals(request.Data.ParatextProjectId))
                    .First();
            }
            catch (Exception)
            {
                throw new Exception($"paratextprojectid {request.Data.ParatextProjectId} not found");
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

                var noteAdded = project.AddNote(writeLock, anchor, commentParagraphs, assignedUser: new UserInfo(request.Data.ParatextUser));
                //cannot return IProjectNote return value from AddNote because it has a circular reference and the json serializer used with signalr cannot serialize the object.
                return Task.FromResult(new RequestResult<IProjectNote>(null));
            }
        }

        private class UserInfo : IUserInfo
        {
            public UserInfo(string name)
            {
                Name = name;
            }

            public string Name { get;}
        }
        private void WriteLockReleaseRequested(IWriteLock obj)
        {
            //throw new NotImplementedException();
            _logger.LogError("WriteLockReleaseRequested Hit", obj);
        }
    }
}
