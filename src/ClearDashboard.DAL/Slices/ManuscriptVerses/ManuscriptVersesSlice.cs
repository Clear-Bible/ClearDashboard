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

    public class GetManuscriptVerseByIdHandler : ResourceDatabaseRequestHandler<GetManuscriptVerseByIdQuery, QueryResult<List<CoupleOfStrings>>, List<CoupleOfStrings>>
    {
        public GetManuscriptVerseByIdHandler(ILogger<GetManuscriptVerseByIdHandler> logger) : base(logger)
        {
            //no-op
        }

        protected override string DatabaseName => "manuscriptverses.sqlite";
        
        public override Task<QueryResult<List<CoupleOfStrings>>> Handle(GetManuscriptVerseByIdQuery request, CancellationToken cancellationToken)
        {
            var queryResult = ValidateDatabasePath(new List<CoupleOfStrings>());
            if (queryResult.Success)
            {
                try
                {
                    queryResult.Data = ExecuteCommand($"SELECT verseID, verseText FROM verses WHERE verseID LIKE '{request.VerseId[..5]}%' ORDER BY verseID");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An unexpected error occurred while querying the '{DatabaseName}' database for verses with verseId : '{request.VerseId}'");
                    queryResult.Success = false;
                    queryResult.Message = ex.Message;
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
