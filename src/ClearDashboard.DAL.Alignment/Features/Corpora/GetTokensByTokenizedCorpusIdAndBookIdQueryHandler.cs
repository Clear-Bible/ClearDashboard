using Microsoft.EntityFrameworkCore;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, IEnumerable<Token> tokens, bool isSentenceStart)>>
    {
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
                var tokenizedCorpus = ProjectDbContext.TokenizedCorpora
                    .Include(tc => tc.Tokens)
                    .FirstOrDefault(tc => tc.Id == request.TokenizedCorpusId.Id);

                if (tokenizedCorpus == null)
                {
                    throw new Exception($"Tokenized Corpus {request.TokenizedCorpusId.Id} does not exist.");
                }

                var intifiedBookId = Int32.Parse(request.BookId);

                var groupedTokens = tokenizedCorpus.Tokens
                    .Where(t => t.BookNumber == intifiedBookId)
                    .OrderBy(t => t.BookNumber)
                    .ThenBy(t => t.ChapterNumber)
                    .ThenBy(t => t.VerseNumber)
                    .ThenBy(t => t.WordNumber)
                    .ThenBy(t => t.SubwordNumber)
                    .GroupBy(
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
                    (result: null, success: false, message: ex.ToString());
            }
        }
    }
}