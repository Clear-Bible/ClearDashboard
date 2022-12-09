using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public record GetConsonantsSliceQuery(string Word) : IRequest<RequestResult<List<string>>>;

    public class GetConsonantsSliceQueryHandler : SqliteDatabaseRequestHandler<GetConsonantsSliceQuery,
        RequestResult<List<string>>, List<string>>
    {
        public GetConsonantsSliceQueryHandler(ILogger<GetConsonantsSliceQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "SemanticDomainsLookup.sqlite";

        public override Task<RequestResult<List<string>>> Handle(GetConsonantsSliceQuery request,
            CancellationToken cancellationToken)
        {
            ResourceDirectory = Path.Combine(Environment.CurrentDirectory, @"Resources\marble-concordances");

            var queryResult = ValidateResourcePath(new List<string>());
            if (queryResult.Success)
            {
                try
                {
                    var lower = ExecuteSqliteCommandAndProcessData(
                        $"SELECT DISTINCT ORIGINAL FROM WORDLOOKUP WHERE CONSONANTS LIKE '%{request.Word.ToLowerInvariant()}%' ");

                    var regular = queryResult.Data = ExecuteSqliteCommandAndProcessData(
                        $"SELECT DISTINCT ORIGINAL FROM WORDLOOKUP WHERE CONSONANTS LIKE '%{request.Word}%' ");

                    queryResult.Data = lower.Union(regular).ToList();
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult,
                        $"An unexpected error occurred while querying the '{ResourceName}' database for the database version'",
                        ex);
                }
            }

            return Task.FromResult(queryResult);
        }

        protected override List<string> ProcessData()
        {
            List<string> results = new List<string>();

            while (DataReader != null && DataReader.Read())
            {
                results.Add(DataReader.GetString(0));
            }

            return results;
        }
    }
}
