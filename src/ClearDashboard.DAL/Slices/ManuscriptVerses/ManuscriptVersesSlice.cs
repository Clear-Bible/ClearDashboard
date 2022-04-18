using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Slices.ManuscriptVerses
{
    public record GetManuscriptVerseByIdQuery(string VerseId) : IRequest<QueryResult<List<CoupleOfStrings>>>;

    public class GetManuscriptVerseByIdHandler : SqliteDatabaseRequestHandler<GetManuscriptVerseByIdQuery, QueryResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>
    {
        public GetManuscriptVerseByIdHandler(ILogger<GetManuscriptVerseByIdHandler> logger) : base(logger)
        {
            //no-op
        }

        protected override string ResourceName => "manuscriptverses.sqlite";
        
        public override Task<QueryResult<List<CoupleOfStrings>>> Handle(GetManuscriptVerseByIdQuery request, CancellationToken cancellationToken)
        {
            var queryResult = ValidateResourcePath(new List<CoupleOfStrings>());
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = ExecuteSqliteCommandAndProcessData($"SELECT verseID, verseText FROM verses WHERE verseID LIKE '{request.VerseId[..5]}%' ORDER BY verseID");
                }
                catch (Exception ex)
                {
                    LogAndSetUnsuccessfulResult(ref queryResult, $"An unexpected error occurred while querying the '{ResourceName}' database for verses with verseId : '{request.VerseId}'", ex);
                }
            }
            return Task.FromResult(queryResult);
        }

        protected override List<CoupleOfStrings> ProcessData()
        {
            var list = new List<CoupleOfStrings>();
            while (DataReader != null && DataReader.Read())
            {
                list.Add(new CoupleOfStrings
                {
                    stringA = DataReader.GetString(0),
                    stringB = DataReader.GetString(1)
                });
            }
            return list;
        }
    }
}
