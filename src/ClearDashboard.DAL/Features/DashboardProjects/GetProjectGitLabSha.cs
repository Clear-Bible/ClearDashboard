﻿using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record GetProjectGitLabShaQuery(string DatabasePath) : IRequest<RequestResult<string>>;

    public class GetProjectGitLabShaQueryHandler : SqliteDatabaseRequestHandler<GetProjectGitLabShaQuery, RequestResult<string>, string>
    {
        public GetProjectGitLabShaQueryHandler(ILogger<GetProjectGitLabShaQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; }

        public override Task<RequestResult<string>> Handle(GetProjectGitLabShaQuery request, CancellationToken cancellationToken)
        {
            FileInfo fi = new FileInfo(request.DatabasePath);

            ResourceDirectory = fi.DirectoryName;
            ResourceName = fi.Name;


            var queryResult = ValidateResourcePath(string.Empty);
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"SELECT LastMergedCommitSha FROM PROJECT LIMIT 1");
                }
                catch
                {
                    queryResult.Success = false;
                }
            }
            return Task.FromResult(queryResult);
        }

        protected override string ProcessData()
        {
            string sha = "";
            while (DataReader != null && DataReader.Read())
            {
                if (!DataReader.IsDBNull(0))
                {
                    sha = DataReader.GetString(0);
                }
            }
            return sha;
        }
    }

}