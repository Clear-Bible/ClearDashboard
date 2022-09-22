using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ModelVerificationType = ClearDashboard.DataAccessLayer.Models.AlignmentVerification;
using ModelOriginatedType = ClearDashboard.DataAccessLayer.Models.AlignmentOriginatedFrom;


namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class CreateAlignmentSetCommandHandler : ProjectDbContextCommandHandler<CreateAlignmentSetCommand,
        RequestResult<AlignmentSet>, AlignmentSet>
    {
        private readonly IMediator _mediator;

        public CreateAlignmentSetCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateAlignmentSetCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<AlignmentSet>> SaveDataAsync(CreateAlignmentSetCommand request,
            CancellationToken cancellationToken)
        {
            var sourceTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.SourceToken.TokenId.Id);
            var targetTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.TargetToken.TokenId.Id);

            var parallelCorpus = ProjectDbContext!.ParallelCorpa
                .Include(pc => pc.SourceTokenizedCorpus)
                    .ThenInclude(stc => stc!.TokenComponents.Where(tc => sourceTokenIds.Contains(tc.Id)))
                .Include(pc => pc.TargetTokenizedCorpus)
                    .ThenInclude(ttc => ttc!.TokenComponents.Where(tc => targetTokenIds.Contains(tc.Id)))
                .FirstOrDefault(c => c.Id == request.ParallelCorpusId.Id);
            if (parallelCorpus == null)
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

            var notFoundSourceTokens = sourceTokenIds
                .Except(parallelCorpus!.SourceTokenizedCorpus!.TokenComponents
                    .Select(tc => tc.Id));
            if (notFoundSourceTokens.Any())
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: $"Requested alignment token pair source Id(s) not found in parallel corpus: '{string.Join(",", notFoundSourceTokens)}'"
                );
            }

            var notFoundTargetTokens = targetTokenIds
                .Except(parallelCorpus!.TargetTokenizedCorpus!.TokenComponents
                    .Select(tc => tc.Id));
            if (notFoundTargetTokens.Any())
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: $"Requested alignment token pair target Id(s) not found in parallel corpus: '{string.Join(",", notFoundTargetTokens)}'"
                );
            }

            var verificationTypes = new Dictionary<string, ModelVerificationType>();
            var originatedTypes = new Dictionary<string, ModelOriginatedType>();
            foreach (var al in request.Alignments)
            {
                if (Enum.TryParse(al.Verification, out ModelVerificationType verificationType))
                {
                    verificationTypes[al.Verification] = verificationType;
                }
                else
                {
                    return new RequestResult<AlignmentSet>
                    (
                        success: false,
                        message: $"Invalid alignment verification type '{al.Verification}' found in request"
                    );
                }

                if (Enum.TryParse(al.OriginatedFrom, out ModelOriginatedType originatedType))
                {
                    originatedTypes[al.OriginatedFrom] = originatedType;
                }
                else
                {
                    return new RequestResult<AlignmentSet>
                    (
                        success: false,
                        message: $"Invalid alignment originated from type '{al.OriginatedFrom}' found in request"
                    );
                }
            }

            try
            {
                var alignmentSetModel = new Models.AlignmentSet
                {
                    ParallelCorpusId = request.ParallelCorpusId.Id,
                    DisplayName = request.DisplayName,
                    SmtModel = request.SmtModel,
                    IsSyntaxTreeAlignerRefined = request.IsSyntaxTreeAlignerRefined,
                    Metadata = request.Metadata,
                    //DerivedFrom = ,
                    //EngineWordAlignment = ,
                    Alignments = request.Alignments
                        .Select(al => new Models.Alignment
                        {
                            SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                            TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                            Score = al.AlignedTokenPair.Score,
                            AlignmentVerification = verificationTypes[al.Verification],
                            AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                        }).ToList()
                };

                ProjectDbContext.AlignmentSets.Add(alignmentSetModel);
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                return new RequestResult<AlignmentSet>(new AlignmentSet(
                    ModelHelper.BuildAlignmentSetId(alignmentSetModel),
                    ModelHelper.BuildParallelCorpusId(alignmentSetModel.ParallelCorpus!),
                    _mediator));

            }
            catch (Exception e)
            {
                return new RequestResult<Alignment.Translation.AlignmentSet>
                (
                    success: false,
                    message: e.Message
                );
            }
        }
    }
}