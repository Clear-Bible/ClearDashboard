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

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class PutAlignmentSetAlignmentCommandHandler : ProjectDbContextCommandHandler<PutAlignmentSetAlignmentCommand,
        RequestResult<Alignment.Translation.AlignmentId>, Alignment.Translation.AlignmentId>
    {
        private readonly IMediator _mediator;

        public PutAlignmentSetAlignmentCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutAlignmentSetAlignmentCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Alignment.Translation.AlignmentId>> SaveDataAsync(PutAlignmentSetAlignmentCommand request,
            CancellationToken cancellationToken)
        {
            var alignmentSet = ProjectDbContext!.AlignmentSets
                .Include(ts => ts.ParallelCorpus)
                .FirstOrDefault(ts => ts.Id == request.AlignmentSetId.Id);

            if (alignmentSet == null)
            {
                return new RequestResult<Alignment.Translation.AlignmentId>
                (
                    success: false,
                    message: $"Invalid AlignmentSetId '{request.AlignmentSetId.Id}' found in request"
                );
            }

            if (!Enum.TryParse(request.Alignment.Verification, out ModelVerificationType verificationType))
            {
                return new RequestResult<Alignment.Translation.AlignmentId>
                (
                    success: false,
                    message: $"Invalid alignment verification type '{request.Alignment.Verification}' found in request"
                );
            }

            if (!Enum.TryParse(request.Alignment.OriginatedFrom, out ModelOriginatedType originatedType))
            {
                return new RequestResult<Alignment.Translation.AlignmentId>
                (
                    success: false,
                    message: $"Invalid alignment originated from type '{request.Alignment.OriginatedFrom}' found in request"
                );
            }

            if (!ProjectDbContext!.TokenComponents
                .Where(tc => tc.Deleted == null)
                .Where(tc => tc.TokenizedCorpusId == alignmentSet!.ParallelCorpus!.SourceTokenizedCorpusId)
                .Any(tc => tc.Id == request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id))
            {
                return new RequestResult<Alignment.Translation.AlignmentId>
                (
                    success: false,
                    message: $"Request alignment token pair source token id '{request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id}' not found in alignment set parallel corpus"
                );
            }

            if (!ProjectDbContext!.TokenComponents
                .Where(tc => tc.Deleted == null)
                .Where(tc => tc.TokenizedCorpusId == alignmentSet!.ParallelCorpus!.TargetTokenizedCorpusId)
                .Any(tc => tc.Id == request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id))
            {
                return new RequestResult<Alignment.Translation.AlignmentId>
                (
                    success: false,
                    message: $"Request alignment token pair target token id '{request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id}' not found in alignment set parallel corpus"
                );
            }

            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
            var sourceTokenIdsForDenormalization = new List<Guid>();
            
            List<Models.Alignment> existingAlignmentMatches = new List<Models.Alignment>();
            Models.Alignment? alignmentToPut = null;

            if (request.Alignment.AlignmentId is not null)
            {
                // Find existing "Assigned" alignment by Id (if it exists and originated from "Assigned"):
                var alignmentById = ProjectDbContext!.Alignments
                    .Where(e => e.Id == request.Alignment.AlignmentId.Id)
                    .Where(e => e.Deleted == null)
                    .Where(e => e.AlignmentOriginatedFrom == ModelOriginatedType.Assigned)
                    .SingleOrDefault();

                if (alignmentById is not null)
                {
                    existingAlignmentMatches.Add(alignmentById);
                }
            }
            
            // Add in any "Assigned" alignments that match by both source and target token ids:
            existingAlignmentMatches.AddRange(ProjectDbContext!.Alignments
                .Where(e => e.AlignmentSetId == alignmentSet.Id)
                .Where(e => e.Deleted == null)
                .Where(e => e.AlignmentOriginatedFrom == ModelOriginatedType.Assigned)
                .Where(e =>
                    e.SourceTokenComponent!.Id == request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id &&
                    e.TargetTokenComponent!.Id == request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id)
                .ToList());

            if (existingAlignmentMatches.Any())
            {
                // Update ("put") the first one:
                alignmentToPut = existingAlignmentMatches.First();
                existingAlignmentMatches.Where(e => e.Id != alignmentToPut.Id).ToList().ForEach(e => 
                {
                    e.Deleted = currentDateTime;
                    sourceTokenIdsForDenormalization.Add(e.SourceTokenComponentId);
                });
            }

            if (alignmentToPut is null)
            {
                alignmentToPut = new Models.Alignment { Id = Guid.NewGuid() };
                ProjectDbContext!.Alignments.Add(alignmentToPut);
            }
            else
            {
                sourceTokenIdsForDenormalization.Add(alignmentToPut.SourceTokenComponentId);
            }

            sourceTokenIdsForDenormalization.Add(request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id);

            alignmentToPut.SourceTokenComponentId = request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id;
            alignmentToPut.TargetTokenComponentId = request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id;
            alignmentToPut.Score = request.Alignment.AlignedTokenPair.Score;
            alignmentToPut.AlignmentVerification = verificationType;
            alignmentToPut.AlignmentOriginatedFrom = originatedType;
            alignmentToPut.AlignmentSetId = alignmentSet.Id;

            using (var transaction = ProjectDbContext.Database.BeginTransaction())
            {
                alignmentSet.AddDomainEvent(new AlignmentSetSourceTokenIdsUpdatingEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization, ProjectDbContext));
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                transaction.Commit();
            }

            //await _mediator.Publish(new AlignmentSetSourceTokenIdsUpdatedEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization), cancellationToken);

            // Querying the database so we can return a fully formed AlignmentId:
            var alignmentId = ModelHelper.AddIdIncludesAlignmentsQuery(ProjectDbContext!)
                .Where(al => al.Id == alignmentToPut.Id)
                .Select(a => ModelHelper.BuildAlignmentId(a))
                .First();

            return new RequestResult<Alignment.Translation.AlignmentId>(alignmentId);
        }
    }
}