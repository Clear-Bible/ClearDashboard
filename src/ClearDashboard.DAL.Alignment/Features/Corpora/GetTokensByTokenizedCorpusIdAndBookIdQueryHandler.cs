using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
    {
        // **********************************************
        // TODO:  Remove the following two lines
        //public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory,
        //    "..", "..", "..", "Corpora", "data");
        //public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");
        // **********************************************

        public GetTokensByTokenizedCorpusIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetTokensByTokenizedCorpusIdAndBookIdQueryHandler> logger) : base(
            projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override
            Task<RequestResult
                <IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>>
            GetDataAsync(GetTokensByTokenizedCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
            //Then iterate tokens and package them by verse then return enumerable.

            var returnData = new List<(string chapter, string verse, IEnumerable<Token>, bool)>();

            var tokenizedCorpus = ProjectDbContext.TokenizedCorpora.Find(request.TokenizedCorpusId.Id);

            var groupedTokens = tokenizedCorpus.Tokens.GroupBy(
                t => new { t.ChapterNumber, t.VerseNumber },
                t => t
            );

            returnData.AddRange(groupedTokens.Select(gt =>
                (
                    gt.Key.ChapterNumber.ToString(),
                    gt.Key.VerseNumber.ToString(),
                    gt.ToList().Select(
                        t => new ClearBible.Engine.Corpora.Token(
                            new TokenId("MAT", t.BookNumber ?? 1, t.ChapterNumber ?? 1, t.VerseNumber ?? 1,
                                t.SubwordNumber ?? 1),
                            t.Text)),
                    false)
            ));


            return Task.FromResult(
                new RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool
                    isSentenceStart)>>
                (result: returnData,
                    success: true,
                    message: "Nice work."));
        }
    }
}