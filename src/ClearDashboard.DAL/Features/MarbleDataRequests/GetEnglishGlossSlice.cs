using ClearDashboard.DAL.CQRS.Features;
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
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer.Features.MarbleDataRequests
{
    public record GetEnglishGlossSliceQuery(string Word) : IRequest<RequestResult<List<CoupleOfStrings>>>;

    public class GetEnglishGlossSliceQueryHandler : SqliteDatabaseRequestHandler<GetEnglishGlossSliceQuery,
        RequestResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>
    {
        public GetEnglishGlossSliceQueryHandler(ILogger<GetEnglishGlossSliceQueryHandler> logger) : base(logger)
        {
            //no-op
        }


        protected override string ResourceName { get; set; } = "SemanticDomainsLookup.sqlite";

        public override Task<RequestResult<List<CoupleOfStrings>>> Handle(GetEnglishGlossSliceQuery request,
            CancellationToken cancellationToken)
        {
            ResourceDirectory = Path.Combine(Environment.CurrentDirectory, @"Resources\marble-concordances");

            var queryResult = ValidateResourcePath(new List<CoupleOfStrings>());
            if (queryResult.Success)
            {
                try
                {
                    var lower = ExecuteSqliteCommandAndProcessData(
                        $"SELECT DISTINCT ORIGINAL, GLOSS FROM WORDLOOKUP WHERE GLOSS LIKE '%{request.Word.ToLowerInvariant()}%' ");

                    queryResult.Data = lower;
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

        protected override List<CoupleOfStrings> ProcessData()
        {
            List<CoupleOfStrings> results = new();

            while (DataReader != null && DataReader.Read())
            {
                results.Add(new CoupleOfStrings
                {
                    stringA = DataReader.GetString(0),
                    stringB = DataReader.GetString(1)
                });
            }

            return results;
        }
    }
}
