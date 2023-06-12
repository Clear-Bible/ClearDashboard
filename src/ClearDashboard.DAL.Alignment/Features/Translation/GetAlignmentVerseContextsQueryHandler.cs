using System.Collections.Generic;
using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.EventsAndDelegates;
using SIL.Machine.FiniteState;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentVerseContextsQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentVerseContextsQuery,
        RequestResult<IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )>>,
        IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )>>
    {

        public GetAlignmentVerseContextsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentVerseContextsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )>>> GetDataAsync(GetAlignmentVerseContextsQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .Include(e => e.ParallelCorpus)
                    .ThenInclude(e => e!.SourceTokenizedCorpus)
                .Include(e => e.ParallelCorpus)
                    .ThenInclude(e => e!.TargetTokenizedCorpus)
                .Where(e => e.Id == request.AlignmentSetId.Id)
                .FirstOrDefaultAsync();

            if (alignmentSet == null)
            {
                //sw.Stop();
               // Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [error]");

                return new RequestResult<IEnumerable<(
                    Alignment.Translation.Alignment alignment,
                    IEnumerable<Token> sourceVerseTokens,
                    uint sourceVerseTokensIndex,
                    IEnumerable<Token> targetVerseTokens,
                    uint targetVerseTokensIndex
                )>>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

            var sourceCorpusId = alignmentSet.ParallelCorpus!.SourceTokenizedCorpus!.CorpusId;
            var targetCorpusId = alignmentSet.ParallelCorpus!.TargetTokenizedCorpus!.CorpusId;

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - AlignmentSet database query", sw.Elapsed);
            sw.Restart();
#endif

            // Don't 'Include' anything below TokenVerseAssociations here because it
            // slows down this query considerably, even if there are none there:
            // Super weird:  when running this query using "" and "" and adding a
            // .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id) clause,
            // the query time went from 0.8s to 2.1s.  Why?!  To address, this .Where
            // clause was added to the Select statement below that converts from
            // database alignments to API alignments.
            var databaseAlignmentsQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Include(e => e.TargetTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.Deleted == null);
            
            if (request.StringsAreTraining)
            {
                databaseAlignmentsQueryable = databaseAlignmentsQueryable
                    .Where(e => e.SourceTokenComponent!.TrainingText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.TrainingText == request.TargetString);
            }
            else
            {
                databaseAlignmentsQueryable = databaseAlignmentsQueryable
                    .Where(e => e.SourceTokenComponent!.SurfaceText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.SurfaceText == request.TargetString);
            }

            var databaseAlignments = databaseAlignmentsQueryable
                .AsNoTrackingWithIdentityResolution()
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Alignments+Tokens database query [count: {1}]", sw.Elapsed, databaseAlignments.Count);
            sw.Restart();
#endif

            // For some reason, when lots of matches, having this AlignmentSetId 'where'
            // with the other pre-ToList() 'where' slows the overall query down considerably
            var filteredDatabaseAlignments = databaseAlignments
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id) 
                .WhereAlignmentTypesFilter(request.AlignmentTypesToInclude);

            var alignments = filteredDatabaseAlignments
                .Select(e => new Alignment.Translation.Alignment(
                    ModelHelper.BuildAlignmentId(
                        e,
                        alignmentSet.ParallelCorpus.SourceTokenizedCorpus,
                        alignmentSet.ParallelCorpus.TargetTokenizedCorpus,
                        e.SourceTokenComponent!),
                    new AlignedTokenPairs(
                        ModelHelper.BuildToken(e.SourceTokenComponent!),
                        ModelHelper.BuildToken(e.TargetTokenComponent!),
                        e.Score),
                    e.AlignmentVerification.ToString(),
                    e.AlignmentOriginatedFrom.ToString()))
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Convert to API alignments", sw.Elapsed);
            sw.Restart();
#endif

            /// Weird possible scenario - is this valid?
            /// VerseMapping
            ///     Verse 001002004 (source) - TokenVerseAssociation - [some token - "TokenB"]
            ///     Verse 001002004 (target)
            ///
            /// Such that there is a source corpus token "TokenA" that would normally be
            /// in the source verse (by having a matching BBBCCCVVV value), but isn't because of the
            /// TokenVerseAssociation.  So what do I return if one of the Alignments' SourceTokens
            /// points to "TokenA"? There won't be a VerseContext since "TokenA" effectively isn't
            /// in the verse mappings...

            var alignmentSourceTokens = alignments.Select(e => e.AlignedTokenPair.SourceToken).Distinct().ToList();
            var alignmentTargetTokens = alignments.Select(e => e.AlignedTokenPair.TargetToken).Distinct().ToList();

            var sourceTokenVerseContexts = TokenVerseContextFinder.GetTokenVerseContexts(
                alignmentSet.ParallelCorpus,
                alignmentSourceTokens,
                true,
                ProjectDbContext!,
                Logger);

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Source token verse contexts [count: {1}]", sw.Elapsed, sourceTokenVerseContexts.Count);
            sw.Restart();
#endif

            var targetTokenVerseContexts = TokenVerseContextFinder.GetTokenVerseContexts(
                alignmentSet.ParallelCorpus,
                alignmentTargetTokens,
                false,
                ProjectDbContext!,
                Logger);

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Target token verse contexts [count: {1}]", sw.Elapsed, targetTokenVerseContexts.Count);
            sw.Restart();
#endif

            List<(
                Alignment.Translation.Alignment Alignment,
                IEnumerable<Token> sourceVerseTokens,
                uint sourceVerseTokensIndex,
                IEnumerable<Token> targetVerseTokens,
                uint targetVerseTokensIndex)> alignmentVerseContexts = new();

            foreach (var alignment in alignments)
            {
                if (sourceTokenVerseContexts.TryGetValue(alignment.AlignedTokenPair.SourceToken.TokenId, out var sourceVerseContext) &&
                    targetTokenVerseContexts.TryGetValue(alignment.AlignedTokenPair.TargetToken.TokenId, out var targetVerseContext))
                {
                    alignmentVerseContexts.Add((
                        alignment,
                        sourceVerseContext.VerseTokens,
                        sourceVerseContext.VerseTokensIndex,
                        targetVerseContext.VerseTokens,
                        targetVerseContext.VerseTokensIndex));
                }
                else
                {
                    if (!sourceTokenVerseContexts.ContainsKey(alignment.AlignedTokenPair.SourceToken.TokenId))
                    {
                        Logger.LogError($"No verse context found for alignment source token {alignment.AlignedTokenPair.SourceToken.TokenId}");
                    }
                    if (!targetTokenVerseContexts.ContainsKey(alignment.AlignedTokenPair.TargetToken.TokenId))
                    {
                        Logger.LogError($"No verse context found for alignment target token {alignment.AlignedTokenPair.TargetToken.TokenId}");
                    }
                }
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif
            return new RequestResult<IEnumerable<(
                    Alignment.Translation.Alignment alignment,
                    IEnumerable<Token> sourceVerseTokens,
                    uint sourceVerseTokensIndex,
                    IEnumerable<Token> targetVerseTokens,
                    uint targetVerseTokensIndex
                )>>(alignmentVerseContexts);
        }
    }
}
