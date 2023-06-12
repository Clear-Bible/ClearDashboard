using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.GitLabUser
{

    public record GetUserFromEmailQuery
        (string ConnectionString, string remoteEmail) : IRequest<RequestResult<CollaborationConfiguration>>;

    public class GetUserFromEmailHandler : MySqlDatabaseRequestHandler<GetUserFromEmailQuery,
        RequestResult<CollaborationConfiguration>, CollaborationConfiguration>
    {
        private readonly ILogger<GetUserFromEmailHandler> _logger;
        private string _connectionString;
        private string _remoteEmail;

        public GetUserFromEmailHandler(ILogger<GetUserFromEmailHandler> logger) :
            base(logger)
        {
            _logger = logger;
            //no-op
        }


        protected override string ResourceName { get; set; } = "";


        public override async Task<RequestResult<CollaborationConfiguration>> Handle(GetUserFromEmailQuery request,
            CancellationToken cancellationToken)
        {
            _connectionString = request.ConnectionString;
            _remoteEmail = request.remoteEmail;

            RequestResult<CollaborationConfiguration> queryResult = new();

            try
            {
                string sql =
                    "SELECT UserId,RemoteUserName,RemoteEmail,RemotePersonalAccessToken,RemotePersonalPassword,GroupName,NamespaceId"
                    + " FROM dashboard.gitlabusers;";
                queryResult.Data = await ExecuteMySqlCommandAndProcessData(_connectionString, sql);

                if (queryResult.Data is not null)
                {
                    queryResult.Success = true;
                }
                else
                {
                    queryResult.Success = false;
                }
            }
            catch (Exception ex)
            {
                LogAndSetUnsuccessfulResult(ref queryResult,
                    $"An unexpected error occurred while querying the '{ResourceName}' database'",
                    ex);
            }

            return queryResult;
        }

        protected override CollaborationConfiguration ProcessData()
        {
            var user = CollaborationConfigurations.FirstOrDefault(c => c.RemoteEmail == _remoteEmail);
            return user;
        }
    }
}