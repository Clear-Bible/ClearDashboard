using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetParallelCorpusByParallelCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetParallelCorpusByParallelCorpusIdQuery,
        RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
            TokenizedTextCorpusId targetTokenizedCorpusId,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedTextCorpusId sourceTokenizedCorpusId,
        TokenizedTextCorpusId targetTokenizedCorpusId,
        IEnumerable<VerseMapping> verseMappings,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelCorpusByParallelCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelCorpusByParallelCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId, 
            TokenizedTextCorpusId targetTokenizedCorpusId, 
            IEnumerable<VerseMapping> verseMappings, 
            ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelCorpusByParallelCorpusIdQuery request, CancellationToken cancellationToken)

        {
            await Task.CompletedTask;

            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an VerseMapping list.
            //2. associated source and target TokenizedCorpusId

            var parallelCorpus =
                ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext)
                    .Include(pc => pc.VerseMappings)
                        .ThenInclude(vm => vm.Verses)
                            .ThenInclude(v => v.TokenVerseAssociations.Where(tva => tva.Deleted == null))
                                .ThenInclude(tva => tva.TokenComponent)
                    .Include(pc => pc.TokenComposites.Where(tc => tc.Deleted == null))
                        .ThenInclude(tc => tc.Tokens)
                            .ThenInclude(t => t.VerseRow)
                    .FirstOrDefault(pc => pc.Id == request.ParallelCorpusId.Id);

            var invalidArgMsg = "";
            if (parallelCorpus == null)
            {
                invalidArgMsg = $"ParallelCorpus not found for ParallelCorpusId '{request.ParallelCorpusId.Id}'";
            }
            else if (parallelCorpus!.SourceTokenizedCorpus == null || parallelCorpus.TargetTokenizedCorpus == null)
            {
                // Not sure this condition is possible, since we
                // are using .Include() for both properties:
                invalidArgMsg = $"ParallelCorpus '{request.ParallelCorpusId.Id}' has null source or target tokenized corpus";
            }

            if (!string.IsNullOrEmpty(invalidArgMsg))
            {
                return new RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
                    TokenizedTextCorpusId targetTokenizedCorpusId,
                    IEnumerable<VerseMapping> verseMappings,
                    ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: invalidArgMsg
                );
            }

            var bookNumbersToAbbreviations =
                FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum), x => x.silCannonBookAbbrev);

            var verseMappings = BuildVerseMappings(parallelCorpus!, bookNumbersToAbbreviations, cancellationToken);

            return new RequestResult<(TokenizedTextCorpusId sourceTokenizedCorpusId,
                    TokenizedTextCorpusId targetTokenizedCorpusId,
                    IEnumerable<VerseMapping> verseMappings,
                    ParallelCorpusId parallelCorpusId)>
                ((
                    ModelHelper.BuildTokenizedTextCorpusId(parallelCorpus!.SourceTokenizedCorpus!),
                    ModelHelper.BuildTokenizedTextCorpusId(parallelCorpus!.TargetTokenizedCorpus!),
                    verseMappings,
                    ModelHelper.BuildParallelCorpusId(parallelCorpus)
                ));
        }

        private static IEnumerable<VerseMapping> BuildVerseMappings(Models.ParallelCorpus parallelCorpus, Dictionary<int, string> bookNumbersToAbbreviations, CancellationToken cancellationToken)
        {
            var sourceCorpusId = parallelCorpus!.SourceTokenizedCorpus!.CorpusId;
            var targetCorpusId = parallelCorpus!.TargetTokenizedCorpus!.CorpusId;

            var verseMappings = parallelCorpus.VerseMappings
                .Where(vm => vm.Verses != null)
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
                });

            return verseMappings;
        }

        private static string VerseBCV(Models.Verse v)
        {
            return $"{v.BookNumber:000}{v.ChapterNumber:000}{v.VerseNumber:000}";
        }
    }
}
