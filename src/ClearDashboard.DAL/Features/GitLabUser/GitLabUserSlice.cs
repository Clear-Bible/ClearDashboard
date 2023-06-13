using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.GitLabUser
{
    public class GitLabUserSlice
    {
        public record PostGitLabUserQuery(string ConnectionString,
            int UserId,
            string RemoteUserName,
            string RemoteEmail,
            string RemotePersonalAccessToken,
            string RemotePersonalPassword,
            string Group,
            int NamespaceId) : IRequest<RequestResult<bool>>;

        public class PostGitLabUserHandler : MySqlDatabaseRequestHandler<PostGitLabUserQuery, RequestResult<bool>, bool>
        {
            private string _connectionString;
            private int _userId;
            private string _remoteUserName;
            private string _remoteEmail;
            private string _remotePersonalAccessToken;
            private string _remotePersonalPassword;
            private string _group;
            private int _namespaceId;

            public PostGitLabUserHandler(ILogger<PostGitLabUserHandler> logger) :
                base(logger)
            {
                //no-op
            }


            protected override string ResourceName { get; set; } = "";


            public override async Task<RequestResult<bool>> Handle(
                PostGitLabUserQuery request, CancellationToken cancellationToken)
            {
                _connectionString = request.ConnectionString;
                _userId = request.UserId;
                _remoteUserName = request.RemoteUserName;
                _remoteEmail = request.RemoteEmail;
                _remotePersonalAccessToken = Encryption.Encrypt(request.RemotePersonalAccessToken);
                _remotePersonalPassword = Encryption.Encrypt(request.RemotePersonalPassword);
                _group = request.Group;
                _namespaceId = request.NamespaceId;

                RequestResult<bool> queryResult = new();

                try
                {
                    queryResult.Data = await ExecuteMySqlCommand(_connectionString, _userId, _remoteUserName,
                        _remoteEmail, _remotePersonalAccessToken, _remotePersonalPassword, _group, _namespaceId);

                    if (queryResult.Data)
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
                if (ReturnValue == 0)
                {
                    return false;
                }

                return true;
            }

        }
        
    }
}
