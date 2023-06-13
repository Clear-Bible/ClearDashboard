using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ModelVerificationType = ClearDashboard.DataAccessLayer.Models.AlignmentVerification;
using ModelOriginatedType = ClearDashboard.DataAccessLayer.Models.AlignmentOriginatedFrom;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DataAccessLayer.Models;
using System.Security.Policy;
using System.Linq;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class PutAlignmentSetAlignmentsCommandHandler : ProjectDbContextCommandHandler<PutAlignmentSetAlignmentsCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public PutAlignmentSetAlignmentsCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutAlignmentSetAlignmentsCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(PutAlignmentSetAlignmentsCommand request,
            CancellationToken cancellationToken)
        {
            
            var alignmentSet = ProjectDbContext!.AlignmentSets
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.TargetTokenizedCorpus)
                .FirstOrDefault(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid AlignmentSetId '{request.AlignmentSetId.Id}' found in request"
                );
            }

            // If an alignment doesn’t have an id, INSERT, else UPDATE.UI should not UPDATE type auto.
            // UI can UPDATE other types.API limits there being more than one type other than type #1
            // alignment between the same two alignment pairs.

            // Check at all incoming alignments' source tokens are part of the correct parallel/tokenized corpus:
            var requestSourceTokenIds = request.Alignments.Select(e => e.AlignedTokenPair.SourceToken.TokenId.Id);

            var databaseSourceTokensById = ProjectDbContext!.TokenComponents
                .Include(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.Deleted == null)
                .Where(e => e.TokenizedCorpusId == alignmentSet.ParallelCorpus!.SourceTokenizedCorpusId)
                .Where(e => requestSourceTokenIds.Contains(e.Id))
                .ToDictionary(e => e.Id, e => e);

            var requestSourceTokenIdsNotFoundInDatabase = requestSourceTokenIds.Except(databaseSourceTokensById.Keys);
            if (requestSourceTokenIdsNotFoundInDatabase.Any())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Request alignment token pair source token ids [{string.Join(", ", requestSourceTokenIdsNotFoundInDatabase.Select(o => o.ToString()))}] not found in alignment set parallel corpus"
                );
            }

            // Check at all incoming alignments' target tokens are part of the correct parallel/tokenized corpus:
            var requestTargetTokenIds = request.Alignments.Select(e => e.AlignedTokenPair.TargetToken.TokenId.Id);

            var databaseTargetTokensById = ProjectDbContext!.TokenComponents
                .Include(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.Deleted == null)
                .Where(e => e.TokenizedCorpusId == alignmentSet.ParallelCorpus!.TargetTokenizedCorpusId)
                .Where(e => requestTargetTokenIds.Contains(e.Id))
                .ToDictionary(e => e.Id, e => e);

            var requestTargetTokenIdsNotFoundInDatabase = requestTargetTokenIds.Except(databaseTargetTokensById.Keys);
            if (requestTargetTokenIdsNotFoundInDatabase.Any())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Request alignment token pair target token ids [{string.Join(", ", requestTargetTokenIdsNotFoundInDatabase.Select(o => o.ToString()))}] not found in alignment set parallel corpus"
                );
            }

            // No updating of type 'auto' (i.e. FromAlignmentModel):
            if (request.Alignments
                .Where(e => e.AlignmentId != null)
                .Where(e => e.OriginatedFrom == AlignmentOriginatedFrom.FromAlignmentModel.ToString())
                .Any())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Alignment(s) in request have AlignmentIds and an OriginatedFrom == 'FromAlignmentModel'.  Updating FromAlignmentModel alignments is not allowed."
                );
            }

            var requestAlignmentsToInsert = new Dictionary<Guid, Alignment.Translation.Alignment>();

            var requestAlignmentsToUpdate = request.Alignments
                .Where(e => e.AlignmentId != null)
                .ToDictionary(e => e.AlignmentId!.Id, e => e);

            var requestAlignmentsWithoutIds = request.Alignments
                .Where(e => e.AlignmentId == null);

            var databaseAlignmentsById = ProjectDbContext!.Alignments
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(e => requestAlignmentsToUpdate.Keys.Contains(e.Id))
                .ToDictionary(e => e.Id, e => e);

            var alignmentIdsNotFound = requestAlignmentsToUpdate.Keys.Except(databaseAlignmentsById.Keys);
            if (alignmentIdsNotFound.Any()) 
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"AlignmentId(s) in request not found in database [{string.Join(", ", alignmentIdsNotFound.Select(o => o.ToString()))}]."
                );
            }

            // Look for source and target token matches in the given alignment set
            // that haven't been soft deleted and are not FromAlignmentModel alignments.
            // Any found can be updated instead of creating new alignments.

            var candidatesForUpdate = ProjectDbContext!.Alignments
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(e => requestAlignmentsWithoutIds.Select(a => a.AlignedTokenPair.SourceToken.TokenId.Id).Contains(e.SourceTokenComponentId))
                .Where(e => requestAlignmentsWithoutIds.Select(a => a.AlignedTokenPair.TargetToken.TokenId.Id).Contains(e.TargetTokenComponentId))
                .Where(e => e.Deleted == null)
                .Where(e => e.AlignmentOriginatedFrom != ModelOriginatedType.FromAlignmentModel)
                .ToList();

            var candidateTokenMatchesBySource = candidatesForUpdate.GroupBy(e => e.SourceTokenComponentId).ToDictionary(g => g.Key, g => g.Select(e => e));
            var candidateTokenMatchesByTarget = candidatesForUpdate.GroupBy(e => e.TargetTokenComponentId).ToDictionary(g => g.Key, g => g.Select(e => e));
            var identifiableEntityComparer = new IdentifiableEntityComparer();

            foreach (var requestAlignmentWithoutId in requestAlignmentsWithoutIds)
            {
                if (candidateTokenMatchesBySource.TryGetValue(requestAlignmentWithoutId.AlignedTokenPair.SourceToken.TokenId.Id, out var sourceMatches) &&
                    candidateTokenMatchesByTarget.TryGetValue(requestAlignmentWithoutId.AlignedTokenPair.TargetToken.TokenId.Id, out var targetMatches))
                {
                    var combinedMatch = sourceMatches.Intersect(targetMatches, identifiableEntityComparer).FirstOrDefault() as Models.Alignment;
                    if (combinedMatch is not null)
                    {
                        databaseAlignmentsById[combinedMatch.Id] = combinedMatch;
                        requestAlignmentsToUpdate[combinedMatch.Id] = requestAlignmentWithoutId;

                        continue;
                    }
                }

                requestAlignmentsToInsert.Add(Guid.NewGuid(), requestAlignmentWithoutId);
            }

            var sourceTokenIdsForDenormalization = new List<Guid>();
            foreach (var requestAlignmentToUpdate in requestAlignmentsToUpdate)
            {
                // We already checked for 'alignmentIdsNotFound' above, so they should all be in
                // this dictionary:
                var databaseAlignment = databaseAlignmentsById[requestAlignmentToUpdate.Key];

                if (!Enum.TryParse(requestAlignmentToUpdate.Value.OriginatedFrom, out ModelOriginatedType originatedType))
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"Invalid alignment originated from type '{requestAlignmentToUpdate.Value.OriginatedFrom}' found in request"
                    );
                }

                if (!Enum.TryParse(requestAlignmentToUpdate.Value.Verification, out ModelVerificationType verificationType))
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"Invalid alignment verification type '{requestAlignmentToUpdate.Value.Verification}' found in request alignment"
                    );
                }

                sourceTokenIdsForDenormalization.Add(databaseAlignment.SourceTokenComponentId);
                sourceTokenIdsForDenormalization.Add(requestAlignmentToUpdate.Value.AlignedTokenPair.SourceToken.TokenId.Id);

                databaseAlignment.AlignmentSet = alignmentSet;
                databaseAlignment.AlignmentOriginatedFrom = originatedType;
                databaseAlignment.AlignmentVerification = verificationType;
                databaseAlignment.SourceTokenComponent = databaseSourceTokensById[requestAlignmentToUpdate.Value.AlignedTokenPair.SourceToken.TokenId.Id];
                databaseAlignment.TargetTokenComponent = databaseTargetTokensById[requestAlignmentToUpdate.Value.AlignedTokenPair.TargetToken.TokenId.Id];
                databaseAlignment.Score = requestAlignmentToUpdate.Value.AlignedTokenPair.Score;

                requestAlignmentToUpdate.Value.AlignmentId = ModelHelper.BuildAlignmentId(databaseAlignment);
            }

            foreach (var requestAlignmentToInsert in requestAlignmentsToInsert)
            {
                if (!Enum.TryParse(requestAlignmentToInsert.Value.OriginatedFrom, out ModelOriginatedType originatedType))
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"Invalid alignment originated from type '{requestAlignmentToInsert.Value.OriginatedFrom}' found in request"
                    );
                }

                if (!Enum.TryParse(requestAlignmentToInsert.Value.Verification, out ModelVerificationType verificationType))
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"Invalid alignment verification type '{requestAlignmentToInsert.Value.Verification}' found in request alignment"
                    );
                }

                sourceTokenIdsForDenormalization.Add(requestAlignmentToInsert.Value.AlignedTokenPair.SourceToken.TokenId.Id);

                var alignmentToInsert = new Models.Alignment
                {
                    Id = requestAlignmentToInsert.Key,
                    AlignmentSet = alignmentSet,
                    AlignmentOriginatedFrom = originatedType,
                    AlignmentVerification = verificationType,
                    SourceTokenComponent = databaseSourceTokensById[requestAlignmentToInsert.Value.AlignedTokenPair.SourceToken.TokenId.Id],
                    TargetTokenComponent = databaseTargetTokensById[requestAlignmentToInsert.Value.AlignedTokenPair.TargetToken.TokenId.Id],
                    Score = requestAlignmentToInsert.Value.AlignedTokenPair.Score
                };

                ProjectDbContext!.Alignments.Add(alignmentToInsert);

                requestAlignmentToInsert.Value.AlignmentId = ModelHelper.BuildAlignmentId(alignmentToInsert);
            }

            using (var transaction = ProjectDbContext.Database.BeginTransaction())
            {
                alignmentSet.AddDomainEvent(new AlignmentSetSourceTokenIdsUpdatingEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization, ProjectDbContext));
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                transaction.Commit();
            }

            await _mediator.Publish(new AlignmentSetSourceTokenIdsUpdatedEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization), cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}