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
            async Task<RequestResult
                <IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>>
            GetDataAsync(GetTokensByTokenizedCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tokenizedCorpus = await ProjectDbContext.TokenizedCorpora.FindAsync(request.TokenizedCorpusId.Id);

                var groupedTokens = tokenizedCorpus.Tokens.GroupBy(
                    t => new { t.ChapterNumber, t.VerseNumber },
                    t => t
                );

                return new RequestResult<
                    IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>
                >
                (
                    groupedTokens.Select(gt =>
                        (
                            gt.Key.ChapterNumber.ToString(),
                            gt.Key.VerseNumber.ToString(),
                            gt.ToList().Select(
                                t => new ClearBible.Engine.Corpora.Token(
                                    new TokenId(
                                        t.BookNumber,
                                        t.ChapterNumber,
                                        t.VerseNumber,
                                        t.WordNumber,
                                        t.SubwordNumber),
                                    t.Text)),
                            false)
                    )
                );
            }
            catch (Exception ex)
            {
                return new RequestResult<
                        IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>
                    >
                    (result: null, success: false, message: ex.Message);
            }
        }
    }
}