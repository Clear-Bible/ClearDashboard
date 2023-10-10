using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, RequestResult<IReadOnlyList<ExternalNote>>>
    {
        private readonly IPluginHost _host;
        private readonly ILogger<GetNotesQueryHandler> _logger;

        public GetNotesQueryHandler(IPluginHost host, ILogger<GetNotesQueryHandler> logger)
        {
            _host = host;
            _logger = logger;
        }
        public Task<RequestResult<IReadOnlyList<ExternalNote>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
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
            catch (InvalidOperationException) //empty, which means the external project doesn't exist
            {
                return Task.FromResult(new RequestResult<IReadOnlyList<ExternalNote>>(new List<ExternalNote>()));
            }
            var paratextChapterNotes = project.GetNotes(request.Data.BookNumber, request.Data.ChapterNumber, request.Data.IncludeResolved);

            return Task.FromResult(new RequestResult<IReadOnlyList<ExternalNote>>(paratextChapterNotes
                .Select(ptn => ptn.GetExternalNote(project))
                .ToList()));
        }
    }
}
