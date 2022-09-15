using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Bcv
{
    //public record GetBcvDictionariesQuery() : 
    //    IRequest<RequestResult<Dictionary<string, string>>>;

    public record GetBcvDictionariesQuery(LocalView<Corpus> localCorpora) : ProjectRequestQuery<Dictionary<string, string>>;

    public class GetBcvDictionariesQueryHandler : ProjectDbContextQueryHandler<
        GetBcvDictionariesQuery,
        RequestResult<Dictionary<string, string>>,
        Dictionary<string, string>>
    {
        private readonly IMediator _mediator;

        public GetBcvDictionariesQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetBcvDictionariesQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Dictionary<string, string>>> GetDataAsync(GetBcvDictionariesQuery request, CancellationToken cancellationToken)
        {
            var verses =
                ProjectDbContext.Corpa
                    .WhereOrList(request.localCorpora)
                    //.Where(c => c.Id == Guid.Parse("DBD155D1-2B4F-4E32-961C-32902BA07B82"))
                    .Join(ProjectDbContext.TokenizedCorpora, c => c.Id, tc => tc.CorpusId, (c, tc) => new { c, tc })
                    .Join(ProjectDbContext.Tokens, tc => tc.tc.Id, t => t.TokenizationId, (tc, t) => new { tc, t })
                    //.Where(x => x.tc.c.ParatextGuid=="ad617f73fbb4c531c76c1c281cac42c9d7241793") 
                    .Select(x => new { x.t.BookNumber, x.t.ChapterNumber, x.t.VerseNumber })
                    .Distinct()
                    .OrderBy(x => x.BookNumber)
                    .ThenBy(x => x.ChapterNumber)
                    .ThenBy(x => x.VerseNumber);
            //var versesList = verses.ToList();

            var bbbcccvvv = "000000000";
            Dictionary<string, string> dict = new();
            foreach (var verse in verses)
            {
                bbbcccvvv = verse.BookNumber.ToString().PadLeft(3, '0') +
                            verse.ChapterNumber.ToString().PadLeft(3, '0') +
                            verse.VerseNumber.ToString().PadLeft(3, '0');
                dict.Add(bbbcccvvv, bbbcccvvv);
            }
            return new RequestResult<Dictionary<string, string>>(dict);

            //return new RequestResult<Dictionary<string, string>>(verses);

        }
    }

    static class WhereOrListExtension
    {
        public static DbSet<Corpus> WhereOrList(this DbSet<Corpus> dbCorpora, LocalView<Corpus> localCorpora)
        {
            //for each item we need to know if item.id == items on the list and if so to include them
            var disposalList = new List<Corpus>();
            foreach (var dbCorpus in dbCorpora)
            {
                foreach (var localCorpus in localCorpora)
                {
                    if (dbCorpus.Id != localCorpus.Id && !disposalList.Contains(dbCorpus))
                    {
                        disposalList.Add(dbCorpus);
                    }
                }
            }

            foreach (var item in disposalList)
            {
                dbCorpora.Remove(item);
            }
            return dbCorpora;
        }
    }

    //public class GetBcvDictionariesQueryHandler : //ProjectDbContextQueryHandler<GetBcvDictionariesQuery, RequestResult<Dictionary<string, string>>, Dictionary<string, string>>
    //SqliteDatabaseRequestHandler<GetBcvDictionariesQuery, RequestResult<Dictionary<string, string>>, Dictionary<string, string>>
    //{
    //    public GetBcvDictionariesQueryHandler(ILogger<GetBcvDictionariesQueryHandler> logger) : base(logger)
    //    {
    //        //no-op
    //    }

    //    protected override string ResourceName { get; set; } = "manuscriptverses.sqlite";

    //    public override Task<RequestResult<Dictionary<string, string>>> Handle(GetBcvDictionariesQuery request, CancellationToken cancellationToken)
    //    {
    //        //var queryResult = ValidateResourcePath(new List<CoupleOfStrings>());
    //        if (queryResult.Success)
    //        {
    //            try
    //            {
    //                queryResult.Data = ExecuteSqliteCommandAndProcessData($"SELECT verseID, verseText FROM verses WHERE verseID LIKE '{request.VerseId[..6]}%' ORDER BY verseID");
    //            }
    //            catch (Exception ex)
    //            {
    //                LogAndSetUnsuccessfulResult(ref queryResult, $"An unexpected error occurred while querying the '{ResourceName}' database for verses with verseId : '{request.VerseId}'", ex);
    //            }
    //        }
    //        return Task.FromResult(queryResult);
    //    }

    //    protected override Dictionary<string, string> ProcessData()
    //    {
    //        var list = new List<CoupleOfStrings>();
    //        while (DataReader != null && DataReader.Read())
    //        {
    //            list.Add(new CoupleOfStrings
    //            {
    //                stringA = DataReader.GetString(0),
    //                stringB = DataReader.GetString(1)
    //            });
    //        }
    //        return list;
    //    }
    //}
}
