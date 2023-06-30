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
using ClearBible.Engine.Corpora;
using Microsoft.AspNet.SignalR.Client.Http;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class PutTranslationSetTranslationCommandHandler : ProjectDbContextCommandHandler<PutTranslationSetTranslationCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public PutTranslationSetTranslationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutTranslationSetTranslationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(PutTranslationSetTranslationCommand request,
            CancellationToken cancellationToken)
        {
            var rTokenId = request.Translation.SourceToken.TokenId;
            var translationSet = ProjectDbContext!.TranslationSets
                .Include(ts => ts.ParallelCorpus)
                .FirstOrDefault(ts => ts.Id == request.TranslationSetId.Id);

            if (translationSet == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                );
            }

            if (request.TranslationActionType == "PutPropagate")
            {
                PutPropagate(translationSet, request.Translation, cancellationToken);
            }
            else
            {
                PutNoPropagate(translationSet, request.Translation, cancellationToken);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<object>(null);
        }

        private void PutPropagate(Models.TranslationSet translationSet, Alignment.Translation.Translation requestTranslation, CancellationToken cancellationToken)
        {
            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

            var rTokenId = requestTranslation.SourceToken.TokenId;

            var translations = ProjectDbContext!.Translations
                .Include(tr => tr.SourceTokenComponent)
                .Where(tr => tr.Deleted == null)
                .Where(tr => tr.TranslationSetId == translationSet.Id)
                .Where(tr => tr.SourceTokenComponent!.TrainingText == requestTranslation.SourceToken.TrainingText);
            var tokenComponents = ProjectDbContext!.TokenComponents
                .Where(t => t.Deleted == null)
                .Where(t => t.TokenizedCorpusId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                .Where(t => t.TrainingText == requestTranslation.SourceToken.TrainingText);

            var exactMatchFound = false;
            var tokenGuidsUpdated = new List<Guid>();
            foreach (var tr in translations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var exactMatch = rTokenId.Id == tr.SourceTokenComponent!.Id;

                // Don't propagate to a non-exact match that has already been assigned:
                if (exactMatch || tr.TranslationState != Models.TranslationOriginatedFrom.Assigned)
                {
                    tr.TargetText = requestTranslation.TargetTranslationText;
                    tr.TranslationState = exactMatch
                        ? Models.TranslationOriginatedFrom.Assigned
                        : Models.TranslationOriginatedFrom.FromOther;
                    tr.Modified = currentDateTime;
                    tr.LexiconTranslationId = requestTranslation.LexiconTranslationId?.Id;
                }

                tokenGuidsUpdated.Add(tr.SourceTokenComponent!.Id);

                if (exactMatch) exactMatchFound = true;
            }

            foreach (var t in tokenComponents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!tokenGuidsUpdated.Contains(t.Id))
                {
                    var exactMatch = rTokenId.Id == t.Id;
                    translationSet.Translations.Add(new Models.Translation
                    {
                        SourceTokenComponent = t,
                        TargetText = requestTranslation.TargetTranslationText,
                        TranslationState = exactMatch
                            ? Models.TranslationOriginatedFrom.Assigned
                            : Models.TranslationOriginatedFrom.FromOther,
                        LexiconTranslationId = requestTranslation.LexiconTranslationId?.Id
                    });

                    if (exactMatch) exactMatchFound = true;
                }
            }

            if (!exactMatchFound)
            {
                throw new ArgumentException($"Unable to find TokenId {rTokenId.Id} in source tokenized corpus id {translationSet.ParallelCorpus!.SourceTokenizedCorpusId} (PutPropagate)");
            }
        }

        private void PutNoPropagate(Models.TranslationSet translationSet, Alignment.Translation.Translation requestTranslation, CancellationToken cancellationToken)
        {
            var rTokenId = requestTranslation.SourceToken.TokenId;

            var translation = ProjectDbContext!.Translations
                .Where(tr => tr.Deleted == null)
                .Where(tr => tr.TranslationSetId == translationSet.Id)
                .Where(tr => tr.SourceTokenComponentId == rTokenId.Id)
                .FirstOrDefault();

            if (translation != null)
            {
                translation.TargetText = requestTranslation.TargetTranslationText;
                translation.TranslationState = Models.TranslationOriginatedFrom.Assigned;
                translation.Modified = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
                translation.LexiconTranslationId = requestTranslation.LexiconTranslationId?.Id;
            }
            else
            {
                var tokenComponent = ProjectDbContext!.TokenComponents
                    .Where(t => t.Deleted == null)
                    .Where(t => t.TokenizedCorpusId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                    .Where(t => t.Id == rTokenId.Id)
                    //.Where(t => t.TokenizedCorpus!.SourceParallelCorpora
                    //    .Any(spc => spc.TranslationSets
                    //        .Any(ts => ts.Id == request.TranslationSetId.Id)))
                    .FirstOrDefault();

                if (tokenComponent != null)
                {
                    translationSet.Translations.Add(new Models.Translation
                    {
                        SourceTokenComponent = tokenComponent,
                        TargetText = requestTranslation.TargetTranslationText,
                        TranslationState = Models.TranslationOriginatedFrom.Assigned,
                        LexiconTranslationId = requestTranslation.LexiconTranslationId?.Id
                    });
                }
                else
                {
                    throw new ArgumentException($"Unable to find TokenId {rTokenId.Id} in source tokenized corpus id {translationSet.ParallelCorpus!.SourceTokenizedCorpusId} (PutNoPropagate)");
                }
            }
        }
    }
}