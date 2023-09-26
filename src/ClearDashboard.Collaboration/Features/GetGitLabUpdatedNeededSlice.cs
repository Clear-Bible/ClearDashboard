using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static ClearDashboard.DataAccessLayer.Features.MarbleDataRequests.LoadSemanticDictionaryLookupSlice;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClearDashboard.Collaboration.Features
{

    public record GetGitLabUpdatedNeededQuery(DashboardProject DashboardProject) : IRequest<RequestResult<bool>>;


    public class GetGitLabUpdatedNeededQueryHandler : CollabRequestHandler<GetGitLabUpdatedNeededQuery, RequestResult<bool>, bool>
    {
        private readonly CollaborationManager _collaborationManager;
        private DashboardProject _dashboardProject;
        private bool _returnResult;


        public GetGitLabUpdatedNeededQueryHandler(ILogger<GetGitLabUpdatedNeededQueryHandler> logger, CollaborationManager collaborationManager) : base(logger)
        {
            _collaborationManager = collaborationManager;
            //no-op
        }

        protected override string ResourceName { get; set; }
        public override Task<RequestResult<bool>> Handle(GetGitLabUpdatedNeededQuery request, CancellationToken cancellationToken)
        {
            _dashboardProject = request.DashboardProject;

            string remoteSha =_collaborationManager.GetRemoteSha(_dashboardProject.FullFilePath, _dashboardProject.Id);
            if (remoteSha != string.Empty)
            {
                // check to see if the local GitLabSha is different than the remote one
                if (remoteSha != _dashboardProject.GitLabSha)
                {
                    _returnResult = true;
                }
            }

            var result = new RequestResult<bool>(success: true, message: "no op")
            {
                Data = _returnResult
            };

            return Task.FromResult(result);
        }

        protected override bool ProcessData()
        {
            return _returnResult;
        }
    };

}
