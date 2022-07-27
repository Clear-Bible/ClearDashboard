using Microsoft.EntityFrameworkCore;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetTokensByTokenizedCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<VerseTokens>>,
        IEnumerable<VerseTokens>>
    {
        public GetTokensByTokenizedCorpusIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetTokensByTokenizedCorpusIdAndBookIdQueryHandler> logger) : base(
            projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override
            async Task<RequestResult
                <IEnumerable<VerseTokens>>>
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

                var bookAbbreviationsToNumbers =
                    FileGetBookIds.BookIds.ToDictionary(x => x.silCannonBookAbbrev, x => int.Parse(x.silCannonBookNum), StringComparer.OrdinalIgnoreCase);
                if (!bookAbbreviationsToNumbers.TryGetValue(request.BookId, out int bookNumber))
                {
                    throw new Exception($"Invalid book '{request.BookId}' found in request");
                }

                var groupedTokens = tokenizedCorpus.Tokens
                    .Where(t => t.BookNumber == bookNumber)
                    .OrderBy(t => t.BookNumber)
                    .ThenBy(t => t.ChapterNumber)
                    .ThenBy(t => t.VerseNumber)
                    .ThenBy(t => t.WordNumber)
                    .ThenBy(t => t.SubwordNumber)
                    .GroupBy(
                        t => new { t.ChapterNumber, t.VerseNumber },
                        t => t
                    );

                return new RequestResult<IEnumerable<VerseTokens>>
                (
                    groupedTokens.Select(gt =>
                        ( 
                            new VerseTokens(gt.Key.ChapterNumber.ToString(),
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
                                 false
                                )
                           )
                    )
                );
            }
            catch (Exception ex)
            {
                return new RequestResult<IEnumerable<VerseTokens>>
                    (result: null, success: false, message: ex.ToString());
            }
        }
    }
}