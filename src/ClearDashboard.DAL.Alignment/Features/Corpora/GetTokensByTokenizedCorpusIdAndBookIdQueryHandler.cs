using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

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
            var bookNumberForAbbreviation = ModelHelper.GetBookNumberForSILAbbreviation(request.BookId);

            // We do a ToList() here to avoid 'cannot create expression tree'
            // errors in the VerseTokens GroupBy below
            var bookNumberAsPaddedString = $"{bookNumberForAbbreviation:000}";
            var verseRows = ProjectDbContext.VerseRows
                .Include(vr => vr.TokenComponents.Where(tc => tc.Deleted == null))
                .Where(vr => vr.TokenizedCorpusId == request.TokenizedCorpusId.Id)
                .Where(vr => vr.BookChapterVerse!.Substring(0, 3) == bookNumberAsPaddedString)
                .OrderBy(vr => vr.BookChapterVerse)
                .ToList();

            if (!verseRows.Any() && ProjectDbContext!.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TokenizedCorpusId.Id) == null)
            {
                throw new Exception($"Tokenized Corpus {request.TokenizedCorpusId.Id} does not exist.");
            }

            var verseTokens = verseRows
                .Select(vr => {
                    cancellationToken.ThrowIfCancellationRequested();

                    var chapter = int.Parse(vr.BookChapterVerse!.Substring(3, 3)).ToString();
                    var verse = int.Parse(vr.BookChapterVerse!.Substring(6, 3)).ToString();

                    var tokenCompositeMap = vr.TokenComponents
                        .Where(tc => tc is Models.TokenComposite)
                        .Where(tc => (tc as Models.TokenComposite)!.ParallelCorpusId == null)
                        .ToDictionary(
                            tc => (tc as Models.TokenComposite)!.Id,
                            tc => (tc as Models.TokenComposite)!);

                    var tokens = vr.TokenComponents
                        .Where(tc => tc is Models.Token)
                        .GroupBy(tc => (tc as Models.Token)!.TokenCompositeId)
                        .SelectMany(g =>
                        {
                            if (g.Key != null)
                            {
                                // Only TokenComposites with a null ParallelCorpusId 
                                // are in the dictionary, therefore this should filter
                                // out non-null ones:
                                if (tokenCompositeMap.TryGetValue((Guid)g.Key!, out var tokenComposite))
                                {
                                    return new[]
                                    {
                                        ModelHelper.BuildCompositeToken(tokenComposite, g.Select(t => (t as Models.Token)!))
                                    };
                                }
                                else
                                {
                                    return Enumerable.Empty<Token>();
                                }
                            }
                            else
                            {
                                return g.Select(t => ModelHelper.BuildToken(t));
                            }
                        })
                        .ToList();
                                
                    return new VerseTokens(
                        chapter,
                        verse,
                        tokens,
                        vr.IsSentenceStart);
                }).ToList();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<VerseTokens>>
            (
                verseTokens
            );
        }
    }
}