
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.Notes
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, RequestResult<IReadOnlyList<IProjectNote>>>
    {
        private readonly IPluginHost _host;
        private readonly IProject _project;
        private readonly ILogger<GetNotesQueryHandler> _logger;

        public GetNotesQueryHandler(IPluginHost host, IProject project, ILogger<GetNotesQueryHandler> logger)
        {
            _host = host;
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<IReadOnlyList<IProjectNote>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
