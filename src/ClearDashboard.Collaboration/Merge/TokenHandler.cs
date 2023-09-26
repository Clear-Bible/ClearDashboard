using System;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using SIL.Machine.Utils;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Merge;

public class TokenHandler : DefaultMergeHandler<IModelSnapshot<Models.Token>>
{
    public TokenHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.Token),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "Token")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.VerseRowId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId))
                {
                    var verseRowId = projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)verseRowLocation!)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    logger.LogDebug($"Converted Token TokenizedCorpusId ('{tokenizedCorpusId}') / VerseRowLocation ('{verseRowLocation}') to VerseRowId ('{verseRowId}')");
                    return (verseRowId != Guid.Empty) ? verseRowId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Token snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowLocation conversion.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Token>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Token>");
                }

                return ResolveTokenId((IModelSnapshot<Models.Token>)modelSnapshot, projectDbContext, logger);
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), TokenBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.Token.VerseRowId) });
    }

    protected static Guid ResolveTokenId(IModelSnapshot<Models.Token> modelSnapshot, ProjectDbContext projectDbContext, ILogger logger)
    {
        if (modelSnapshot.PropertyValues.TryGetValue("Ref", out var refId) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.EngineTokenId), out var engineTokenId))
        {
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.OriginTokenLocation), out var originTokenLocation);
            var hasIndex = int.TryParse(((string)refId!).Split("_").LastOrDefault(), out int index);

            Guid tokenId;
            if (originTokenLocation is not null && hasIndex)
            {
                tokenId = projectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                    .Where(e => e.OriginTokenLocation == (string)originTokenLocation!)
                    .OrderBy(e => e.OriginTokenLocation)
                    .Skip(index)
                    .Take(1)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                if (tokenId == default)
                {
                    tokenId = Guid.NewGuid();
                    logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}).  Using: '{tokenId}'");
                }
                else
                {
                    logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}) to Token Id ('{tokenId}')");
                }
            }
            else
            {
                tokenId = projectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                    .Where(e => e.EngineTokenId == (string)engineTokenId!)
                    .Select(e => e.Id)
                    .FirstOrDefault();

                if (tokenId == default)
                {
                    tokenId = Guid.NewGuid();
                    logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}').  Using: '{tokenId}'");
                }
                else
                {
                    logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}') to Token Id ('{tokenId}')");
                }
            }

            return tokenId;
        }
        else
        {
            throw new PropertyResolutionException($"Token snapshot does not have all:  Ref+TokenizedCorpusId+EngineTokenId, which are required for Id resolution.");
        }
    }

    protected override async Task HandleDeleteAsync(IModelSnapshot<Models.Token> itemToDelete, CancellationToken cancellationToken)
    {
        // If deleting a Token that is associated with a composite...
        // my best guess is we should delete the composite since it
        // is now invalid

        var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
        List<GeneralModel<Models.AlignmentSetDenormalizationTask>> alignmentSetDenormalizationTasks = new();

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
    $"Delete any TokenComposites associated with the token being deleted",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var tokenId = ResolveTokenId(itemToDelete, projectDbContext, logger);

                var dependentTokenCompositeGuids = projectDbContext.TokenCompositeTokenAssociations
                    .Where(e => e.TokenId == tokenId)
                    .Select(e => e.TokenCompositeId)
                    .Distinct()
                    .ToList();

                var tokenCompositesToDelete = projectDbContext.TokenComposites
                    .Include(tc => tc.Translations.Where(t => t.Deleted == null))
                    .Include(tc => tc.SourceAlignments.Where(a => a.Deleted == null))
                    .Include(tc => tc.TargetAlignments.Where(a => a.Deleted == null))
                    .Where(e => dependentTokenCompositeGuids.Contains(e.Id))
                    .ToList();

                foreach (var tokenComposite in tokenCompositesToDelete)
                {
                    projectDbContext.TokenComposites.Remove(tokenComposite);
                    foreach (var e in tokenComposite.Translations) { e.Deleted = currentDateTime; }
                    foreach (var e in tokenComposite.SourceAlignments) 
                    { 
                        e.Deleted = currentDateTime;

                        var t = new GeneralModel<Models.AlignmentSetDenormalizationTask>(nameof(Models.AlignmentSetDenormalizationTask.Id), Guid.NewGuid());
                        t.Add(nameof(Models.AlignmentSetDenormalizationTask.AlignmentSetId), e.AlignmentSetId);
                        t.Add(nameof(Models.AlignmentSetDenormalizationTask.SourceText), tokenComposite.TrainingText!);
                        alignmentSetDenormalizationTasks.Add(t);
                    }
                    foreach (var e in tokenComposite.TargetAlignments) 
                    { 
                        e.Deleted = currentDateTime;

                        var t = new GeneralModel<Models.AlignmentSetDenormalizationTask>(nameof(Models.AlignmentSetDenormalizationTask.Id), Guid.NewGuid());
                        t.Add(nameof(Models.AlignmentSetDenormalizationTask.AlignmentSetId), e.AlignmentSetId);
                        t.Add(nameof(Models.AlignmentSetDenormalizationTask.SourceText), tokenComposite.TrainingText!);
                        alignmentSetDenormalizationTasks.Add(t);
                    }
                }

                await Task.CompletedTask;
            },
            cancellationToken
        );

        if (alignmentSetDenormalizationTasks.Any())
        {
            _mergeContext.MergeBehavior.StartInsertModelCommand(alignmentSetDenormalizationTasks.First());
            foreach (var child in alignmentSetDenormalizationTasks)
            {
                _ = await _mergeContext.MergeBehavior.RunInsertModelCommand(child, cancellationToken);
            }
            _mergeContext.MergeBehavior.CompleteInsertModelCommand(typeof(Models.AlignmentSetDenormalizationTask));

            _mergeContext.FireAlignmentDenormalizationEvent = true;
        }

        await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }
}
