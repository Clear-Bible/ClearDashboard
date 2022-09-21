using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
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

                var bookNumberForAbbreviation = GetBookNumberForSILAbbreviation(request.BookId);

                // We do a ToList() here to avoid 'cannot create expression tree'
                // errors in the VerseTokens GroupBy below
                var tokens = ProjectDbContext.Tokens
                    .Where(token => token.TokenizationId == request.TokenizedCorpusId.Id)
                    .Where(token => token.BookNumber == bookNumberForAbbreviation).ToList();

                if (!tokens.Any() && ProjectDbContext!.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TokenizedCorpusId.Id) == null)
                {
                    throw new Exception($"Tokenized Corpus {request.TokenizedCorpusId.Id} does not exist.");
                }

                // -------------------------------------------------------
                // These queries are only needed if it is possible for a
                // composite token to span across more than one bible book
                var tokenCompositeIds = tokens
                    .Where(token => token.TokenCompositeId != null)
                    .Select(token => token.TokenCompositeId)
                    .Distinct();

                // Will mostly likely return no tokens
                var nonBookTokensFromComposites = ProjectDbContext.Tokens
                    .Where(token => token.TokenCompositeId != null)
                    .Where(token => token.BookNumber != bookNumberForAbbreviation)
                    .Where(token => tokenCompositeIds.Contains(token.TokenCompositeId)).ToList();

                var allTokens = tokens
                    .Union(nonBookTokensFromComposites)
                    .OrderBy(token => token.EngineTokenId);
                // -------------------------------------------------------

                var verseTokens = allTokens
                    .GroupBy(t => new { t.ChapterNumber, t.VerseNumber })
                    .Select(g => new VerseTokens(
                        g.Key.ChapterNumber.ToString(),
                        g.Key.VerseNumber.ToString(),
                        g
                            .GroupBy(tc => tc.TokenCompositeId)
                            .SelectMany(gc => gc.Key != null
                                ? new[] {
                                    ModelHelper.BuildCompositeToken((Guid)gc.Key, gc.Select(t => t))
                                  }
                                : gc.Select(t => ModelHelper.BuildToken(t))
                                ),
                            false)
                        );

                // need an await to get the compiler to be 'quiet'
                await Task.CompletedTask;

                return new RequestResult<IEnumerable<VerseTokens>>
                (
                    verseTokens
                );
            }
            catch (Exception ex)
            {
                return new RequestResult<IEnumerable<VerseTokens>>
                    (result: null, success: false, message: ex.ToString());
            }

           
        }

        private int GetBookNumberForSILAbbreviation(string silBookAbbreviation)
        {
            var bookMappingDatum = FileGetBookIds.BookIds
                .FirstOrDefault(bookDatum => bookDatum.silCannonBookAbbrev == silBookAbbreviation);

            if (bookMappingDatum == null)
            {
                throw new Exception(
                    $"Unable to map book abbreviation: {silBookAbbreviation} to book number."
                );
            }

            if (Int32.TryParse(bookMappingDatum.silCannonBookNum, out int intifiedBookNumber))
            {
                return intifiedBookNumber;
            }
            else
            {
                throw new Exception(
                    $"Unable to parse book number {bookMappingDatum.silCannonBookNum} for SIL Book abbreviation {silBookAbbreviation}"
                );
            }
        }
    }
}