using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DAL.CQRS.Features.Features;

namespace ClearDashboard.DataAccessLayer.Features.GitLabUser
{
    public class GitLabUserSlice
    {
        public record PostGitLabUserQuery(string ConnectionString,
            int userId,
            string remoteUserName,
            string remoteEmail,
            string remotePersonalAccessToken,
            string remotePersonalPassword,
            string group,
            int namespaceId) : IRequest<RequestResult<bool>>;

        public class PostGitLabUserHandler : MySqlDatabaseRequestHandler<PostGitLabUserQuery, RequestResult<bool>, bool>
        {
            private readonly ILogger<PostGitLabUserHandler> _logger;
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


                _logger = logger;
                //no-op
            }


            protected override string ResourceName { get; set; } = "";


            public override async Task<RequestResult<bool>> Handle(
                PostGitLabUserQuery request, CancellationToken cancellationToken)
            {
                _connectionString = request.ConnectionString;
                _userId = request.userId;
                _remoteUserName = request.remoteUserName;
                _remoteEmail = request.remoteEmail;
                _remotePersonalAccessToken = request.remotePersonalAccessToken;
                _remotePersonalPassword = request.remotePersonalPassword;
                _group = request.group;
                _namespaceId = request.namespaceId;

                //ResourceName = Path.Combine(Environment.CurrentDirectory, @"Resources\SDBG\lemmaLU.csv");

                RequestResult<bool> queryResult = new();
                //queryResult = ValidateResourcePath(new List<SemanticGlossesLookup>());
                //if (queryResult.Success == false)
                //{
                //    LogAndSetUnsuccessfulResult(ref queryResult,
                //        $"An unexpected error occurred while querying the MARBLE CSV Lookup databases : '{ResourceName}'");
                //    return Task.FromResult(queryResult);
                //}

                try
                {
                    queryResult.Data = await ExecuteMySqlCommand(_connectionString, _userId, _remoteUserName,
                        _remoteEmail, _remotePersonalAccessToken, _remotePersonalPassword, _group, _namespaceId);
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
                return true;
            }

        }
        
    }
}
