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

                var bookNumberForAbbreviation = GetBookNumberForSILAbbreviation(request.BookId);
                var tokens = ProjectDbContext.Tokens.Where(token => token.TokenizationId == request.TokenizedCorpusId.Id
                                                                    && token.BookNumber == bookNumberForAbbreviation);


                var groupedTokens = tokens
                    .OrderBy(t => t.BookNumber)
                    .ThenBy(t => t.ChapterNumber)
                    .ThenBy(t => t.VerseNumber)
                    .ThenBy(t => t.WordNumber)
                    .ThenBy(t => t.SubwordNumber)
                    .GroupBy(
                        t => new { t.ChapterNumber, t.VerseNumber },
                        t => t
                    );

                // need an await to get the compiler to be 'quiet'
                await Task.CompletedTask;

                return new RequestResult<IEnumerable<VerseTokens>>
                (
                    groupedTokens.Select(gt =>
                        (
                            new VerseTokens(gt.Key.ChapterNumber.ToString(),
                                gt.Key.VerseNumber.ToString(),
                                gt.Select(
                                    t => new Token(
                                        new TokenId(
                                            t.BookNumber,
                                            t.ChapterNumber,
                                            t.VerseNumber,
                                            t.WordNumber,
                                            t.SubwordNumber),
                                        t.SurfaceText ?? string.Empty,
                                        t.TrainingText ?? string.Empty)),
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

            if (Int32.TryParse(bookMappingDatum.clearTreeBookNum, out int intifiedBookNumber))
            {
                return intifiedBookNumber;
            }
            else
            {
                throw new Exception(
                    $"Unable to parse book number {bookMappingDatum.clearTreeBookNum} for SIL Book abbreviation {silBookAbbreviation}"
                );
            }
        }
    }
}