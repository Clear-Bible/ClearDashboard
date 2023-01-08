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
                .Include(vr => vr.TokenComponents
                    .Where(tc => tc.Deleted == null)
                    .Where(tc => tc.GetType() == typeof(Models.Token))
                    .AsQueryable())
                    .ThenInclude(tc => ((Models.Token)tc).TokenCompositeTokenAssociations
                        .Where(ta => ta.Deleted == null)
                        .AsQueryable())
                        .ThenInclude(ta => ta.TokenComposite)
                .Where(vr => vr.TokenizedCorpusId == request.TokenizedCorpusId.Id)
                .Where(vr => vr.BookChapterVerse!.Substring(0, 3) == bookNumberAsPaddedString)
                .OrderBy(vr => vr.BookChapterVerse)
                .ToList();

            if (!verseRows.Any() && ProjectDbContext!.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TokenizedCorpusId.Id) == null)
            {
                throw new Exception($"Tokenized Corpus {request.TokenizedCorpusId.Id} does not exist.");
            }

            var tokenCompositeGuids = ProjectDbContext.TokenCompositeTokenAssociations
                .Include(ta => ta.Token)
                .Include(ta => ta.TokenComposite)
                .Where(ta => ta.Deleted == null)
                .Where(ta => ta.TokenComposite!.TokenizedCorpusId == request.TokenizedCorpusId.Id)
                .Where(ta => ta.TokenComposite!.ParallelCorpusId == null)
                .Where(ta => ta.Token!.Deleted == null)
                .Where(ta => ta.Token!.BookNumber == bookNumberForAbbreviation)
                .Select(ta => ta.TokenCompositeId)
                .Distinct();

            var tokenCompositesByGuid = ProjectDbContext.TokenComposites
                .Include(tc => tc.Tokens)
                .Where(tc => tokenCompositeGuids.Contains(tc.Id))
                .ToDictionary(tc => tc.Id, tc => tc);

            var verseTokens = verseRows
                .Select(vr => {
                    cancellationToken.ThrowIfCancellationRequested();

                    var chapter = int.Parse(vr.BookChapterVerse!.Substring(3, 3)).ToString();
                    var verse = int.Parse(vr.BookChapterVerse!.Substring(6, 3)).ToString();

                    var tokens = vr.Tokens
                        .Where(t => !t.TokenCompositeTokenAssociations
                            .Where(ta => ta.TokenComposite!.ParallelCorpusId == null)
                            .Any())
                        .Select(t => ModelHelper.BuildToken(t))
                        .OrderBy(t => t.TokenId)
                        .ToList();

                    var composites = vr.Tokens
                        .SelectMany(t => t.TokenCompositeTokenAssociations
                            .Select(ta => ta.TokenCompositeId))
                        .Distinct()
                        .SelectMany(id =>
                        {
                            if (tokenCompositesByGuid.TryGetValue(id, out var tokenComposite))
                            {
                                return new[]
                                {
                                    ModelHelper.BuildCompositeToken(
                                        tokenComposite,
                                        tokenComposite.Tokens.Where(t => t.VerseRowId == vr.Id),
                                        tokenComposite.Tokens.Where(t => t.VerseRowId != vr.Id))
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