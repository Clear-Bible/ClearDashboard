using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.DashboardProjects
{
    public record GetProjectBBBCCCVVVQuery(string DatabasePath, Guid CorpusId) : IRequest<RequestResult<List<string>>>;

    public class GetProjectBBBCCCVVVQueryHandler : SqliteDatabaseRequestHandler<GetProjectBBBCCCVVVQuery, RequestResult<List<string>>, List<string>>
    {
        public GetProjectBBBCCCVVVQueryHandler(ILogger<GetProjectBBBCCCVVVQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; }

        public override Task<RequestResult<List<string>>> Handle(GetProjectBBBCCCVVVQuery request, CancellationToken cancellationToken)
        {
            FileInfo fi = new FileInfo(request.DatabasePath);

            ResourceDirectory = fi.DirectoryName;
            ResourceName = fi.Name;


            var queryResult = ValidateResourcePath(new List<string>());
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"SELECT DISTINCT BBBCCCVVV FROM VERSE WHERE CORPUSID='{request.CorpusId.ToString().ToUpper()}' ORDER BY BBBCCCVVV");
                }
                catch
                {
                    queryResult.Success = false;
                }
            }
            return Task.FromResult(queryResult);
        }

        protected override List<string> ProcessData()
        {
            List<string> strings = new List<string>();
            while (DataReader != null && DataReader.Read())
            {
                if (!DataReader.IsDBNull(0))
                {
                    strings.Add(DataReader.GetString(0));
                }
            }
            return strings;
        }
    }

}
