using ClearDashboard.DAL.CQRS;

using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SIL.Scripture;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class ResolveExternalNoteCommandHandler : IRequestHandler<ResolveExternalNoteCommand, RequestResult<string>>
    {
        private readonly IPluginHost _host;
        private readonly MainWindow _mainWindow;
        private readonly ILogger<ResolveExternalNoteCommandHandler> _logger;

        public ResolveExternalNoteCommandHandler(IPluginHost host, MainWindow mainWindow, ILogger<ResolveExternalNoteCommandHandler> logger)
        {
            _host = host;
            _mainWindow = mainWindow;
            _logger = logger;
        }
        public Task<RequestResult<string>> Handle(ResolveExternalNoteCommand request, CancellationToken cancellationToken)
        {
            if (request.Data.ExternalProjectId.Equals(string.Empty))
            {
                throw new Exception($"externalprojectid is not set");
            }
            if (request.Data.ExternalNoteId.Equals(string.Empty))
            {
                throw new Exception($"externalnoteid is not set");
            }
            if (request.Data.VerseRefString.Equals(string.Empty))
            {
                throw new Exception($"verserefstring is not set");
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

            var verse = new VerseRef(request.Data.VerseRefString);
            var paratextChapterNotes = project.GetNotes(verse.BookNum, verse.ChapterNum, true);

            var paratextNotesWithExternalNoteId = paratextChapterNotes
                .Where(n => n.GetIdAndLabels().id.Equals(request.Data.ExternalNoteId));

            if (paratextNotesWithExternalNoteId.Count() != 1)
            {
                throw new Exception(
                    @$"Looking for ExternalNoteId resulted in {paratextNotesWithExternalNoteId.Count()} notes. 
                        Can only process if associated with exacly one note.");
            }

            var paratextNoteWithExternalId = paratextNotesWithExternalNoteId.First();

            using var writeLock = project.RequestWriteLock(_mainWindow, WriteLockReleaseRequested, WriteLockScope.ProjectNotes);
            if (writeLock == null)
            {
                throw new Exception("Couldn't obtain write lock required to resolve note.");
            }
            else
            {

                paratextNoteWithExternalId.Resolve(writeLock);
                return Task.FromResult(new RequestResult<string>(""));
            }
        }
        private void WriteLockReleaseRequested(IWriteLock obj)
        {
            //throw new NotImplementedException();
            _logger.LogError("WriteLockReleaseRequested Hit", obj);
        }
    }
}
