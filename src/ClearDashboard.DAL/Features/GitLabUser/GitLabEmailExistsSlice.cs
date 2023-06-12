using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.GitLabUser
{
    public record GitLabEmailExistsQuery(string ConnectionString, string remoteEmail) : IRequest<RequestResult<bool>>;

    public class GitLabEmailExistsHandler : MySqlDatabaseRequestHandler<GitLabEmailExistsQuery, RequestResult<bool>, bool>
    {
        private readonly ILogger<GitLabEmailExistsHandler> _logger;
        private string _connectionString;
        private string _remoteEmail;

        public GitLabEmailExistsHandler(ILogger<GitLabEmailExistsHandler> logger) :
            base(logger)
        {
            _logger = logger;
            //no-op
        }


        protected override string ResourceName { get; set; } = "";


        public override async Task<RequestResult<bool>> Handle(GitLabEmailExistsQuery request, CancellationToken cancellationToken)
        {
            _connectionString = request.ConnectionString;
            _remoteEmail = request.remoteEmail;

            RequestResult<bool> queryResult = new();

            try
            {
                string sql =
                    "SELECT UserId,RemoteUserName,RemoteEmail,RemotePersonalAccessToken,RemotePersonalPassword,GroupName,NamespaceId"
                    + " FROM dashboard.gitlabusers;";
                queryResult.Data = await ExecuteMySqlCommandAndProcessData(_connectionString, sql);

                if (queryResult.Data == true)
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

        protected override bool ProcessData()
        {
            // look for user if they exist or not
            var user = CollaborationConfigurations.FirstOrDefault(c => c.RemoteEmail == _remoteEmail);

            if (user is null)
            {
                return false;
            }

            return true;
        }

    }
}
