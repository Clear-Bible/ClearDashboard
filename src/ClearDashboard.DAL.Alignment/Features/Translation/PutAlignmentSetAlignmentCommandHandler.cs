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
using Microsoft.EntityFrameworkCore.Storage;
using ClearDashboard.DAL.Alignment.Translation;

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

            var alignmentsToRemove = ProjectDbContext!.Alignments
                .Where(tr => tr.AlignmentSetId == alignmentSet.Id)
                .Where(tr => tr.SourceTokenComponent!.Id == request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id ||
                             tr.TargetTokenComponent!.Id == request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id);

            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

            foreach (var e in alignmentsToRemove)
            {
                e.Deleted = currentDateTime;
            }

            var alignment = new Models.Alignment
            {
                SourceTokenComponentId = request.Alignment.AlignedTokenPair.SourceToken.TokenId.Id,
                TargetTokenComponentId = request.Alignment.AlignedTokenPair.TargetToken.TokenId.Id,
                Score = request.Alignment.AlignedTokenPair.Score,
                AlignmentVerification = verificationType,
                AlignmentOriginatedFrom = originatedType
            };


            alignmentSet.Alignments.Add(alignment);

            using (var transaction =  ProjectDbContext.Database.BeginTransaction())
            {
                alignmentSet.AddDomainEvent(new AlignmentAddingRemovingEvent(alignmentsToRemove, alignment, ProjectDbContext));
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                transaction.Commit();
            }

            await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsToRemove, alignment), cancellationToken);

            // Querying the database so we can return a fully formed AlignmentId:
            var alignmentId = ModelHelper.AddIdIncludesAlignmentsQuery(ProjectDbContext!)
                .Where(al => al.Id == alignment.Id)
                .Select(a => ModelHelper.BuildAlignmentId(a))
                .First();

            return new RequestResult<Alignment.Translation.AlignmentId>(alignmentId);
        }
    }
}