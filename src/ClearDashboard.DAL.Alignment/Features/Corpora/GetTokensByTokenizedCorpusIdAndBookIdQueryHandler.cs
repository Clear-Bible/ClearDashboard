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
using System.Diagnostics;

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
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var bookNumberForAbbreviation = ModelHelper.GetBookNumberForSILAbbreviation(request.BookId);
            var bookNumberAsPaddedString = $"{bookNumberForAbbreviation:000}";

            var tokensByVerseRowId = ProjectDbContext.VerseRows
                .Include(e => e.TokenComponents.Where(t => t.Deleted == null))
                .Where(e => e.TokenizedCorpusId == request.TokenizedCorpusId.Id)
                .Where(e => e.BookChapterVerse!.Substring(0, 3) == bookNumberAsPaddedString)
                .OrderBy(e => e.BookChapterVerse)
                .ToDictionary(e => e.Id, e => e);

#if DEBUG
            sw.Stop();
#endif

            if (!tokensByVerseRowId.Any() && ProjectDbContext!.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TokenizedCorpusId.Id) == null)
            {
                throw new Exception($"Tokenized Corpus {request.TokenizedCorpusId.Id} does not exist.");
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Get tokens by VerseRow [count: {tokensByVerseRowId.Count}] for tokenized corpus '{request.TokenizedCorpusId.Id}' and book id '{request.BookId}'");
            sw.Restart();
#endif

            var tokenCompositeAssociations = ProjectDbContext.TokenCompositeTokenAssociations
                .Include(ta => ta.Token)
                .Include(ta => ta.TokenComposite)
                .Where(ta => ta.Deleted == null)
                .Where(ta => ta.TokenComposite!.TokenizedCorpusId == request.TokenizedCorpusId.Id)
                .Where(ta => ta.TokenComposite!.ParallelCorpusId == null)
                .Where(ta => ta.Token!.Deleted == null)
                .Where(ta => ta.Token!.BookNumber == bookNumberForAbbreviation)
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Get TokenComposite / Token associations");
            sw.Restart();
#endif
            var tokenCompositeTokensByGuid = tokenCompositeAssociations
                .GroupBy(ta => ta.TokenComposite)
                .ToDictionary(
                    g => g.Key!.Id, 
                    g => new { TokenComposite = g.Key!, Tokens = g.Select(ta => ta.Token!)});

            var tokenCompositeGuidByTokenGuid = tokenCompositeAssociations
                .ToDictionary(ta => ta.TokenId, ta => ta.TokenCompositeId);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Get TokenComposites by Guid [count: {tokenCompositeTokensByGuid.Count}]");
            sw.Restart();
#endif

            var verseTokens = tokensByVerseRowId
                .Select(kvp => {
                    cancellationToken.ThrowIfCancellationRequested();

                    var chapter = int.Parse(kvp.Value.BookChapterVerse!.Substring(3, 3)).ToString();
                    var verse = int.Parse(kvp.Value.BookChapterVerse!.Substring(6, 3)).ToString();

                    var tokens = kvp.Value.Tokens
                        .Where(tc => !tokenCompositeGuidByTokenGuid.ContainsKey(tc.Id))
                        .Select(t => ModelHelper.BuildToken(t))
                        .OrderBy(t => t.TokenId)
                        .ToList();

                    var composites = kvp.Value.Tokens
                        .Where(t => tokenCompositeGuidByTokenGuid.ContainsKey(t.Id))
                        .Select(t => tokenCompositeGuidByTokenGuid[t.Id])
                        .Distinct()
                        .SelectMany(tcid =>
                        {
                            if (tokenCompositeTokensByGuid.TryGetValue(tcid, out var tokenCompositeTokens))
                            {
                                return new[]
                                {
                                    ModelHelper.BuildCompositeToken(
                                        tokenCompositeTokens.TokenComposite,
                                        tokenCompositeTokens.Tokens.Where(t => t.VerseRowId == kvp.Key),
                                        tokenCompositeTokens.Tokens.Where(t => t.VerseRowId != kvp.Key))
                                };
                            }
                            else
                            {
                                return Enumerable.Empty<CompositeToken>();
                            }
                        })
                        .ToList();

                    tokens.AddRange(composites);
                                
                    return new VerseTokens(
                        chapter,
                        verse,
                        tokens,
                        kvp.Value.IsSentenceStart,
                        kvp.Value.IsInRange,
                        kvp.Value.IsRangeStart,
                        kvp.Value.IsEmpty,
                        kvp.Value.OriginalText ?? string.Empty
                        );
                }).ToList();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [verse token count: {verseTokens.Count}] for tokenized corpus '{request.TokenizedCorpusId.Id}' and book id '{request.BookId}'");
#endif

            return new RequestResult<IEnumerable<VerseTokens>>
            (
                verseTokens
            );
        }
    }
}