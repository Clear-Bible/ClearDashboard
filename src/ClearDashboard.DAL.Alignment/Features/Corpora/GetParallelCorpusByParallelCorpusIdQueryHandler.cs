using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an VerseMapping list.
            //2. associated source and target TokenizedCorpusId

            var parallelCorpus =
                ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext)
                    .Include(pc => pc.VerseMappings)
                        .ThenInclude(vm => vm.Verses)
                            .ThenInclude(v => v.TokenVerseAssociations)
                                .ThenInclude(tva => tva.TokenComponent)
                    .Include(pc => pc.TokenComposites)
                        .ThenInclude(tc => tc.VerseRow)
                    .Include(pc => pc.TokenComposites)
                        .ThenInclude(tc => tc.Tokens)
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
                    var sourceVerses = vm.Verses
                        .Where(v => v.CorpusId == sourceCorpusId)
                        .Where(v => v.BookNumber != null && v.ChapterNumber != null && v.VerseNumber != null)
                        .Where(v => bookNumbersToAbbreviations.ContainsKey((int)v.BookNumber!))
                        .Select(v =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentBCV = VerseBCV(v);

                            var sourceTokens = v.TokenVerseAssociations
                                .Where(tva => tva.TokenComponent != null)
                                .OrderBy(tva => tva.Position)
                                .Select(tva => ModelHelper.BuildTokenId(tva.TokenComponent!))
                            .Union(parallelCorpus.TokenComposites
                                    .Where(tc => tc.TokenizationId == parallelCorpus.SourceTokenizedCorpusId)
                                    .Where(tc => tc.VerseRow!.BookChapterVerse == currentBCV)
                                    .Select(tc => ModelHelper.BuildTokenId(tc)));

                            return new Verse(
                                bookNumbersToAbbreviations[(int)v.BookNumber!],
                                (int)v.ChapterNumber!,
                                (int)v.VerseNumber!,
                                sourceTokens);
                        });

                    var targetVerses = vm.Verses
                        .Where(v => v.CorpusId == targetCorpusId)
                        .Where(v => v.BookNumber != null && v.ChapterNumber != null && v.VerseNumber != null)
                        .Where(v => bookNumbersToAbbreviations.ContainsKey((int)v.BookNumber!))
                        .Select(v =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var currentBCV = VerseBCV(v);

                            var targetTokens = v.TokenVerseAssociations
                                .Where(tva => tva.TokenComponent != null)
                                .OrderBy(tva => tva.Position)
                                .Select(tva => ModelHelper.BuildTokenId(tva.TokenComponent!))
                            .Union(parallelCorpus.TokenComposites
                                    .Where(tc => tc.TokenizationId == parallelCorpus.TargetTokenizedCorpusId)
                                    .Where(tc => tc.VerseRow!.BookChapterVerse == currentBCV)
                                    .Select(tc => ModelHelper.BuildTokenId(tc)));

                            return new Verse(
                                bookNumbersToAbbreviations[(int)v.BookNumber!],
                                (int)v.ChapterNumber!,
                                (int)v.VerseNumber!,
                                targetTokens);
                        });

                    return new VerseMapping(sourceVerses, targetVerses);
                });

            return verseMappings;
        }

        private static string VerseBCV(Models.Verse v)
        {
            return $"{v.BookNumber:000}{v.ChapterNumber:000}{v.VerseNumber:000}";
        }
    }
}
