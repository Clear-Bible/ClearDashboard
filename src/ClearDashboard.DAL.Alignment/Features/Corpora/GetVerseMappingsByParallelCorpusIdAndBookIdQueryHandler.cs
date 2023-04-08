using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetVerseMappingsByParallelCorpusIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetVerseMappingsByParallelCorpusIdAndBookIdQuery,
        RequestResult<IEnumerable<VerseMapping>>,
        IEnumerable<VerseMapping>>
    {

        public GetVerseMappingsByParallelCorpusIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetVerseMappingsByParallelCorpusIdAndBookIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<VerseMapping>>> GetDataAsync(GetVerseMappingsByParallelCorpusIdAndBookIdQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            await Task.CompletedTask;

            IQueryable<Models.ParallelCorpus> parallelCorpusQueryable = ProjectDbContext!.ParallelCorpa
                .Include(e => e.SourceTokenizedCorpus)
                .Include(e => e.TargetTokenizedCorpus)
                .Include(e => e.TokenComposites.Where(tc => tc.Deleted == null))
                    .ThenInclude(tc => tc.Tokens)
                            .ThenInclude(t => t.VerseRow);

            if (request.BookId is null)
            {
                parallelCorpusQueryable = parallelCorpusQueryable
                    .Include(e => e.VerseMappings)
                        .ThenInclude(e => e.Verses)
                            .ThenInclude(verse => verse.TokenVerseAssociations)
                                .ThenInclude(tva => tva.TokenComponent);
            }

            var parallelCorpusEntity = parallelCorpusQueryable
                    .FirstOrDefault(e => e.Id == request.ParallelCorpusId.Id);

#if DEBUG
            sw.Stop();
#endif

            var invalidArgMsg = "";
            if (parallelCorpusEntity == null)
            {
                invalidArgMsg = $"ParallelCorpus entity not found for ParallelCorpusId '{request.ParallelCorpusId.Id}'";
            }
            else if (parallelCorpusEntity!.SourceTokenizedCorpus == null || parallelCorpusEntity.TargetTokenizedCorpus == null)
            {
                // Not sure this condition is possible, since we
                // are using .Include() for both properties:
                invalidArgMsg = $"ParallelCorpus entity '{request.ParallelCorpusId.Id}' has null source or target tokenized corpus";
            }

            if (!string.IsNullOrEmpty(invalidArgMsg))
            {
                return new RequestResult<IEnumerable<VerseMapping>>
                (
                    success: false,
                    message: invalidArgMsg
                );
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Get parallel corpus '{parallelCorpusEntity!.DisplayName}'");
            sw.Restart();
#endif

            IEnumerable<Models.VerseMapping>? verseMappingEntities = null;

            if (request.BookId is null)
            {
                verseMappingEntities = parallelCorpusEntity.VerseMappings;
            }
            else
            {
                var sourceBookNumber = ModelHelper.GetBookNumberForSILAbbreviation(request.BookId);

                var parallelCorpusGuid = request.ParallelCorpusId.Id;
                var sourceTokenizedCorpusGuid = request.ParallelCorpusId.SourceTokenizedCorpusId!.Id;

                verseMappingEntities = ProjectDbContext!.VerseMappings
                    .Include(verseMapping => verseMapping.Verses)
                        .ThenInclude(verse => verse.TokenVerseAssociations)
                            .ThenInclude(tva => tva.TokenComponent)
                    .Where(verseMapping => verseMapping.ParallelCorpusId == parallelCorpusGuid)
                    .Where(verseMapping =>
                        verseMapping.Verses
                            .Where(verse => !verse.TokenVerseAssociations.Any())
                            .Select(verse => verse.BookNumber)
                            .Any(bookNumber => bookNumber.Equals(sourceBookNumber))
                        ||
                        verseMapping.Verses
                            .Where(verse => verse.TokenVerseAssociations.Any())
                            .SelectMany(verse => verse.TokenVerseAssociations
                                .Select(tva => new { tva.TokenComponent!.TokenizedCorpusId, ((Models.Token)tva.TokenComponent!).BookNumber }))
                            .Any(tcb =>
                                tcb.TokenizedCorpusId == sourceTokenizedCorpusGuid &&
                                tcb.BookNumber.Equals(sourceBookNumber)));

            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Get verse mapping entities '{parallelCorpusEntity!.DisplayName}' / '{request.BookId}' [verse mapping count: {verseMappingEntities.Count()}]");
            sw.Restart();
#endif

            var bookNumbersToAbbreviations =
                FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum), x => x.silCannonBookAbbrev);

            var verseMappings = BuildVerseMappings(
                parallelCorpusEntity,
                verseMappingEntities,
                bookNumbersToAbbreviations, 
                cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [verse mapping count: {verseMappings.Count}]");
#endif

            return new RequestResult<IEnumerable<VerseMapping>>(verseMappings);
        }

        private static List<VerseMapping> BuildVerseMappings(Models.ParallelCorpus parallelCorpus, IEnumerable<Models.VerseMapping> verseMappingEntities, Dictionary<int, string> bookNumbersToAbbreviations, CancellationToken cancellationToken)
        {
            var sourceCorpusId = parallelCorpus!.SourceTokenizedCorpus!.CorpusId;
            var targetCorpusId = parallelCorpus!.TargetTokenizedCorpus!.CorpusId;

            var verseMappings = verseMappingEntities
                .Select(vm =>
                {
                    var sourceVerseMappingTokenComposites = new List<Models.TokenComposite>();
                    var targetVerseMappingTokenComposites = new List<Models.TokenComposite>();

                    var sourceVerseMappingBCVs = new List<string>();
                    var targetVerseMappingBCVs = new List<string>();

                    var sourceVerses = vm.Verses
                        .Where(v => v.CorpusId == sourceCorpusId)
                        .Where(v => v.BookNumber != null && v.ChapterNumber != null && v.VerseNumber != null)
                        .Where(v => bookNumbersToAbbreviations.ContainsKey((int)v.BookNumber!))
                        .Select(v =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentBCV = VerseBCV(v);
                            sourceVerseMappingBCVs.Add(currentBCV);

                            var sourceTokenIds = v.TokenVerseAssociations
                                .Where(tva => tva.TokenComponent != null)
                                .OrderBy(tva => tva.Position)
                                .Select(tva => ModelHelper.BuildTokenId(tva.TokenComponent!))
                                .ToList();

                            // Capture TokenComposites containing any Tokens that either match the current
                            // Verses's TokenVerseAssociation Tokens, or are in the current Verse's BCV:
                            sourceVerseMappingTokenComposites.AddRange(parallelCorpus.TokenComposites
                                .Where(tc => tc.TokenizedCorpusId == parallelCorpus.SourceTokenizedCorpusId)
                                .Where(tc =>
                                    tc.Tokens.Any(t => t.VerseRow!.BookChapterVerse == currentBCV) ||
                                    tc.Tokens.Any(t => sourceTokenIds.Select(tid => tid.Id).ToList().Contains(t.Id))
                                ));

                            return new Verse(
                                bookNumbersToAbbreviations[(int)v.BookNumber!],
                                (int)v.ChapterNumber!,
                                (int)v.VerseNumber!,
                                sourceTokenIds);
                        }).OrderBy(v => v.BookNum).ThenBy(v => v.ChapterNum).ThenBy(v => v.VerseNum).ToList();

                    var targetVerses = vm.Verses
                        .Where(v => v.CorpusId == targetCorpusId)
                        .Where(v => v.BookNumber != null && v.ChapterNumber != null && v.VerseNumber != null)
                        .Where(v => bookNumbersToAbbreviations.ContainsKey((int)v.BookNumber!))
                        .Select(v =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentBCV = VerseBCV(v);
                            targetVerseMappingBCVs.Add(currentBCV);

                            var targetTokenIds = v.TokenVerseAssociations
                                .Where(tva => tva.TokenComponent != null)
                                .OrderBy(tva => tva.Position)
                                .Select(tva => ModelHelper.BuildTokenId(tva.TokenComponent!))
                                .ToList();

                            // Capture TokenComposites containing any Tokens that either match the current
                            // Verses's TokenVerseAssociation Tokens, or are in the current Verse's BCV:
                            targetVerseMappingTokenComposites.AddRange(parallelCorpus.TokenComposites
                                .Where(tc => tc.TokenizedCorpusId == parallelCorpus.TargetTokenizedCorpusId)
                                .Where(tc =>
                                    tc.Tokens.Any(t => t.VerseRow!.BookChapterVerse == currentBCV) ||
                                    tc.Tokens.Any(t => targetTokenIds.Select(tid => tid.Id).ToList().Contains(t.Id))
                                ));

                            return new Verse(
                                bookNumbersToAbbreviations[(int)v.BookNumber!],
                                (int)v.ChapterNumber!,
                                (int)v.VerseNumber!,
                                targetTokenIds);
                        }).OrderBy(v => v.BookNum).ThenBy(v => v.ChapterNum).ThenBy(v => v.VerseNum).ToList();

                    var sourceVerseMappingTokenGuidsByTVA = sourceVerses.SelectMany(sv => sv.TokenIds.Select(tid => tid.Id));

                    var sourceVerseMappingComposites = sourceVerseMappingTokenComposites
                        .Select(tc => ModelHelper.BuildCompositeToken(
                            tc,
                            tc.Tokens.Where(t =>
                                sourceVerseMappingTokenGuidsByTVA.Contains(t.Id) ||
                                sourceVerseMappingBCVs.Contains(t.VerseRow!.BookChapterVerse!)),
                            tc.Tokens.Where(t =>
                                !sourceVerseMappingTokenGuidsByTVA.Contains(t.Id) &&
                                !sourceVerseMappingBCVs.Contains(t.VerseRow!.BookChapterVerse!))
                        )).ToList();

                    var targetVerseMappingTokenGuidsByTVA = targetVerses.SelectMany(sv => sv.TokenIds.Select(tid => tid.Id));

                    var targetVerseMappingComposites = targetVerseMappingTokenComposites
                        .Select(tc => ModelHelper.BuildCompositeToken(
                            tc,
                            tc.Tokens.Where(t =>
                                targetVerseMappingTokenGuidsByTVA.Contains(t.Id) ||
                                targetVerseMappingBCVs.Contains(t.VerseRow!.BookChapterVerse!)),
                            tc.Tokens.Where(t =>
                                !targetVerseMappingTokenGuidsByTVA.Contains(t.Id) &&
                                !targetVerseMappingBCVs.Contains(t.VerseRow!.BookChapterVerse!))
                        )).ToList();

                    return new VerseMapping(sourceVerses, targetVerses, sourceVerseMappingComposites, targetVerseMappingComposites);
                })
                .ToList();

            return verseMappings;
        }

        private static string VerseBCV(Models.Verse v)
        {
            return $"{v.BookNumber:000}{v.ChapterNumber:000}{v.VerseNumber:000}";
        }
    }
}
