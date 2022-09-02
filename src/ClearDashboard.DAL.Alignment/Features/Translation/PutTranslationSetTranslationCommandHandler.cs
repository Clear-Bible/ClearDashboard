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
                var translations = ProjectDbContext!.Translations
                    .Where(tr => tr.TranslationSetId == request.TranslationSetId.Id)
                    .Where(tr => tr.SourceToken!.TrainingText == request.Translation.SourceToken.TrainingText);
                var tokens = ProjectDbContext!.Tokens
                    .Where(t => t.TokenizationId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                    .Where(t => t.TrainingText == request.Translation.SourceToken.TrainingText);

                var exactMatchFound = false;
                var tokenGuidsUpdated = new List<Guid>();
                foreach (var tr in translations)
                {
                    var exactMatch = ModelHelper.IsTokenIdMatch(rTokenId, tr.SourceToken!);

                    // Don't propagate to a non-exact match that has already been assigned:
                    if (exactMatch || tr.TranslationState != Models.TranslationState.Assigned)
                    {
                        tr.TargetText = request.Translation.TargetTranslationText;
                        tr.TranslationState = exactMatch
                            ? Models.TranslationState.Assigned
                            : Models.TranslationState.FromOther;
                    }

                    tokenGuidsUpdated.Add(tr.SourceToken!.Id);

                    if (exactMatch) exactMatchFound = true;
                }

                foreach (var t in tokens)
                {
                    if (!tokenGuidsUpdated.Contains(t.Id))
                    {
                        var exactMatch = ModelHelper.IsTokenIdMatch(rTokenId, t);
                        translationSet.Translations.Add(new Models.Translation
                        {
                            SourceToken = t,
                            TargetText = request.Translation.TargetTranslationText,
                            TranslationState = exactMatch
                                ? Models.TranslationState.Assigned
                                : Models.TranslationState.FromOther
                        });

                        if (exactMatch) exactMatchFound = true;
                    }
                }

                if (!exactMatchFound)
                {
                    return new RequestResult<object>
                    (
                        success: false,
                        message: $"Unable to find TokenId {request.Translation.SourceToken.TokenId} in source tokenized corpus id {translationSet.ParallelCorpus!.SourceTokenizedCorpusId}"
                    );
                }

            }
            else
            {
                var translation = ProjectDbContext!.Translations
                    .Where(tr => tr.TranslationSetId == translationSet.Id)
                    .Where(tr => tr.SourceToken!.BookNumber == rTokenId.BookNumber)
                    .Where(tr => tr.SourceToken!.ChapterNumber == rTokenId.ChapterNumber)
                    .Where(tr => tr.SourceToken!.VerseNumber == rTokenId.VerseNumber)
                    .Where(tr => tr.SourceToken!.WordNumber == rTokenId.WordNumber)
                    .Where(tr => tr.SourceToken!.SubwordNumber == rTokenId.SubWordNumber)
                    .FirstOrDefault();

                if (translation != null)
                {
                    translation.TargetText = request.Translation.TargetTranslationText;
                    translation.TranslationState = Models.TranslationState.Assigned;
                }
                else
                {
                    var token = ProjectDbContext!.Tokens
                        .Where(t => t.TokenizationId == translationSet.ParallelCorpus!.SourceTokenizedCorpusId)
                        .Where(t => t.BookNumber == rTokenId.BookNumber)
                        .Where(t => t.ChapterNumber == rTokenId.ChapterNumber)
                        .Where(t => t.VerseNumber == rTokenId.VerseNumber)
                        .Where(t => t.WordNumber == rTokenId.WordNumber)
                        .Where(t => t.SubwordNumber == rTokenId.SubWordNumber)
                        //.Where(t => t.Tokenization!.SourceParallelCorpora
                        //    .Any(spc => spc.TranslationSets
                        //        .Any(ts => ts.Id == request.TranslationSetId.Id)))
                        .FirstOrDefault();

                    if (token != null)
                    {
                        translationSet.Translations.Add(new Models.Translation
                        {
                            SourceToken = token,
                            TargetText = request.Translation.TargetTranslationText,
                            TranslationState = Models.TranslationState.Assigned
                        });
                    }
                    else
                    {
                        return new RequestResult<object>
                        (
                            success: false,
                            message: $"Unable to find TokenId {request.Translation.SourceToken.TokenId} in source tokenized corpus id {translationSet.ParallelCorpus!.SourceTokenizedCorpusId}"
                        );
                    }
                }
            }

            // need an await to get the compiler to be 'quiet'
//            await Task.CompletedTask;

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<object>(null);
        }
    }
}