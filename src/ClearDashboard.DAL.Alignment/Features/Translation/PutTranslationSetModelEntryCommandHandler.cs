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

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class PutTranslationSetModelEntryCommandHandler : ProjectDbContextCommandHandler<PutTranslationSetModelEntryCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public PutTranslationSetModelEntryCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutTranslationSetTranslationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(PutTranslationSetModelEntryCommand request,
            CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext!.TranslationSets
                .Include(ts => ts.TranslationModel)
                    .ThenInclude(tm => tm.TargetTextScores)
                .FirstOrDefault(ts => ts.Id == request.TranslationSetId.Id);

            if (translationSet == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                );
            }

            var modelEntry = translationSet.TranslationModel
                .Where(tm => tm.SourceText == request.sourceText)
                .FirstOrDefault();

            if (modelEntry == null)
            {
                modelEntry = new Models.TranslationModelEntry
                {
                    SourceText = request.sourceText,
                    TargetTextScores = new List<Models.TranslationModelTargetTextScore>()
                };

                translationSet.TranslationModel.Add(modelEntry);
            }
            else
            {
                ProjectDbContext!.RemoveRange(modelEntry.TargetTextScores);
                modelEntry.TargetTextScores.Clear();
            }

            modelEntry.TargetTextScores.AddRange(request.targetTranslationTextScores
                .Select(ttts => new Models.TranslationModelTargetTextScore
                {
                    Text = ttts.Key,
                    Score = ttts.Value
                }));

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<object>(null);
        }
    }
}