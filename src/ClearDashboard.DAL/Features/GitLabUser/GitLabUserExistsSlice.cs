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
    public record GitLabUserExistsQuery(string ConnectionString,
     int userId,
     string remoteUserName,
     string remoteEmail) : IRequest<RequestResult<bool>>;

    public class GitLabUserExistsHandler : MySqlDatabaseRequestHandler<GitLabUserExistsQuery, RequestResult<bool>, bool>
    {
        private readonly ILogger<GitLabUserExistsHandler> _logger;
        private string _connectionString;
        private int _userId;
        private string _remoteUserName;
        private string _remoteEmail;

        public GitLabUserExistsHandler(ILogger<GitLabUserExistsHandler> logger) :
            base(logger)
        {
            _logger = logger;
            //no-op
        }


        protected override string ResourceName { get; set; } = "";


        public override async Task<RequestResult<bool>> Handle(
            GitLabUserExistsQuery request, CancellationToken cancellationToken)
        {
            _connectionString = request.ConnectionString;
            _userId = request.userId;
            _remoteUserName = request.remoteUserName;
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
            var user = CollaborationConfigurations.FirstOrDefault(c => c.UserId == _userId);

            if (user is null)
            {
                return false;
            }

            return true;
        }

    }

}

