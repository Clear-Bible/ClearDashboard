using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentsByAlignmentSetIdAndTokenIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentsByAlignmentSetIdAndTokenIdsQuery,
        RequestResult<IEnumerable<Alignment.Translation.Alignment>>,
        IEnumerable<Alignment.Translation.Alignment>>
    {

        public GetAlignmentsByAlignmentSetIdAndTokenIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentsByAlignmentSetIdAndTokenIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Alignment.Translation.Alignment>>> GetDataAsync(GetAlignmentsByAlignmentSetIdAndTokenIdsQuery request, CancellationToken cancellationToken)
        {
            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .Include(ts => ts.ParallelCorpus)
                .FirstOrDefaultAsync(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<IEnumerable<Alignment.Translation.Alignment>>
                (
                    success: false,
                    message: $"Invalid AlignmentSetId '{request.AlignmentSetId.Id}' found in request"
                );
            }

            var sourceTokenIds = request.EngineParallelTextRows.SelectMany(e => e.SourceTokens!.Select(st => st.TokenId.Id)).ToList();
            var targetTokenIds = request.EngineParallelTextRows.SelectMany(e => e.TargetTokens!.Select(st => st.TokenId.Id)).ToList();

            var databaseAlignments = ModelHelper.AddIdIncludesAlignmentsQuery(ProjectDbContext!)
                .Where(al => al.Deleted == null)
                .Where(al => al.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(al => sourceTokenIds.Contains(al.SourceTokenComponentId) || targetTokenIds.Contains(al.TargetTokenComponentId))
                .ToList()
                .FilterByAlignmentMode(request.AlignmentOriginationFilterMode);

            var alignments = databaseAlignments
                .Select(a => new Alignment.Translation.Alignment(
                    ModelHelper.BuildAlignmentId(a),
                    new AlignedTokenPairs(
                        ModelHelper.BuildToken(a.SourceTokenComponent!),
                        ModelHelper.BuildToken(a.TargetTokenComponent!),
                        a.Score),
                    a.AlignmentVerification.ToString(),
                    a.AlignmentOriginatedFrom.ToString()))
                .ToList();

            return new RequestResult<IEnumerable<Alignment.Translation.Alignment>>(alignments.OrderBy(a => a.AlignedTokenPair.SourceToken.TokenId.ToString()).ToList());
        }
     }
}
