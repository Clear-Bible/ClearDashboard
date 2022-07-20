using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetTokensByTokenizedCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<GetTokensByTokenizedCorpusIdAndBookIdQuery, RequestResult<IEnumerable<VerseTokens>>, IEnumerable<VerseTokens>>
    {
        public GetTokensByTokenizedCorpusIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetTokensByTokenizedCorpusIdAndBookIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<VerseTokens>>> GetDataAsync(GetTokensByTokenizedCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var tokenizedCorpus = await ProjectDbContext.TokenizedCorpora.Include(tc => tc.Tokens).FirstOrDefaultAsync(tc => tc.Id == request.TokenizedCorpusId.Id, cancellationToken: cancellationToken);
                if (tokenizedCorpus != null)
                {
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

                    return new RequestResult<IEnumerable<VerseTokens>>
                    (
                        groupedTokens.Select(gt =>
                            new VerseTokens(
                                Chapter: gt.Key.ChapterNumber.ToString(),
                                Verse: gt.Key.VerseNumber.ToString(),
                                Tokens: gt.ToList().Select(
                                    t => new ClearBible.Engine.Corpora.Token(
                                        new TokenId(
                                            t.BookNumber,
                                            t.ChapterNumber,
                                            t.VerseNumber,
                                            t.WordNumber,
                                            t.SubwordNumber),
                                        t.Text)),
                                IsSentenceStart: false)
                        )
                    );
                }
                else
                {
                    return new RequestResult<IEnumerable<VerseTokens>>(result: null, success: false, message: $"Could not find tokenized corpus ID: {request.TokenizedCorpusId.Id}");
                }
            }
            catch (Exception ex)
            {
                return new RequestResult<IEnumerable<VerseTokens>>(result: null, success: false, message: ex.ToString());
            }
        }
    }
}