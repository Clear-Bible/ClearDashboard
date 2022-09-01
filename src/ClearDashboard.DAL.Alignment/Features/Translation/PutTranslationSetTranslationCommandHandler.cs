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

            if (request.TranslationActionType == "PutPropagate")
            {
                var ts = ProjectDbContext!.TranslationSets
                    .Include(ts => ts.Translations
                        .Where(tr => tr.SourceToken!.TrainingText == request.Translation.SourceToken.TrainingText))
                    .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                    .ThenInclude(s => s!.Tokens.Where(t => t.TrainingText == request.Translation.SourceToken.TrainingText))
                    .FirstOrDefault(c => c.Id == request.TranslationSetId.Id);

                if (ts == null)
                {
                    return new RequestResult<object>
                    (
                        success: false,
                        message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                    );
                }

                var exactMatchFound = false;
                var tokenGuidsUpdated = new List<Guid>();
                foreach (var tr in ts!.Translations)
                {
                    var exactMatch = IsTokenIdMatch(rTokenId, tr.SourceToken!);

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

                foreach (var t in ts!.ParallelCorpus!.SourceTokenizedCorpus!.Tokens)
                {
                    if (!tokenGuidsUpdated.Contains(t.Id))
                    {
                        var exactMatch = IsTokenIdMatch(rTokenId, t);
                        ts!.Translations.Add(new Models.Translation
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
                        message: $"Unable to find TokenId {request.Translation.SourceToken.TokenId} in source tokenized corpus id {ts.ParallelCorpus.SourceTokenizedCorpusId}"
                    );
                }

            }
            else
            {
                var ts = ProjectDbContext!.TranslationSets
                    .Include(ts => ts.Translations
                        .Where(t => t.SourceToken!.BookNumber == rTokenId.BookNumber)
                        .Where(t => t.SourceToken!.ChapterNumber == rTokenId.ChapterNumber)
                        .Where(t => t.SourceToken!.VerseNumber == rTokenId.VerseNumber)
                        .Where(t => t.SourceToken!.WordNumber == rTokenId.WordNumber)
                        .Where(t => t.SourceToken!.SubwordNumber == rTokenId.SubWordNumber))
                    .Include(ts => ts.ParallelCorpus)
                    .ThenInclude(pc => pc!.SourceTokenizedCorpus)
                    .ThenInclude(s => s!.Tokens
                        .Where(t => t.BookNumber == rTokenId.BookNumber)
                        .Where(t => t.ChapterNumber == rTokenId.ChapterNumber)
                        .Where(t => t.VerseNumber == rTokenId.VerseNumber)
                        .Where(t => t.WordNumber == rTokenId.WordNumber)
                        .Where(t => t.SubwordNumber == rTokenId.SubWordNumber))
                    .FirstOrDefault(c => c.Id == request.TranslationSetId.Id);

                if (ts == null)
                {
                    return new RequestResult<object>
                    (
                        success: false,
                        message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                    );
                }

                if (ts.Translations.Any())
                {
                    var translation = ts.Translations.First();
                    translation.TargetText = request.Translation.TargetTranslationText;
                    translation.TranslationState = Models.TranslationState.Assigned;
                }
                else
                {
                    var token = ts.ParallelCorpus!.SourceTokenizedCorpus!.Tokens.FirstOrDefault();
                    if (token != null)
                    {
                        ts.Translations.Add(new Models.Translation
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
                            message: $"Unable to find TokenId {request.Translation.SourceToken.TokenId} in source tokenized corpus id {ts.ParallelCorpus.SourceTokenizedCorpusId}"
                        );
                    }
                }
            }

            // need an await to get the compiler to be 'quiet'
//            await Task.CompletedTask;

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<object>(null);
        }

        private static bool IsTokenIdMatch(TokenId tokenId, Models.Token dbToken)
        {
            return (dbToken.BookNumber == tokenId.BookNumber &&
                    dbToken.ChapterNumber == tokenId.ChapterNumber &&
                    dbToken.VerseNumber == tokenId.VerseNumber &&
                    dbToken.WordNumber == tokenId.WordNumber &&
                    dbToken.SubwordNumber == tokenId.SubWordNumber);
        }
    }
}