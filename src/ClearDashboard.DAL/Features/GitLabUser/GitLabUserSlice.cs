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
        public record PostGitLabUserQuery(string ConnectionString, string CommandText) : IRequest<RequestResult<bool>>;

        public class PostGitLabUserHandler : MySqlDatabaseRequestHandler<PostGitLabUserQuery, RequestResult<bool>, bool>
        {
            private readonly ILogger<PostGitLabUserHandler> _logger;
            private string _connectionString;
            private string _commandText;
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
                _commandText = request.CommandText;

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
                    queryResult.Data = await ExecuteMySqlCommandAndProcessData(_connectionString, _commandText);
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the '{ResourceName}' database'",
                        ex);
                }

                queryResult.Data= false;

                return queryResult;
            }

            protected override bool ProcessData()
            {
                
                return false;
            }

        }
        
    }
}
