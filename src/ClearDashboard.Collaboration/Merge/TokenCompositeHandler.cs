using System;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.Collaboration.Exceptions;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class TokenCompositeHandler : DefaultMergeHandler<IModelSnapshot<Models.TokenComposite>>
{
    public TokenCompositeHandler(MergeContext mergeContext) : base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.TokenComposite),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "TokenComposite")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.VerseRowId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenCompositeBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.TokenizedCorpusId), out var tokenizedCorpusId))
                {
                    var verseRowId = projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)verseRowLocation!)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    logger.LogDebug($"Converted TokenComposite TokenizedCorpusId ('{tokenizedCorpusId}') / VerseRowLocation ('{verseRowLocation}') to VerseRowId ('{verseRowId}')");
                    return (verseRowId != Guid.Empty) ? verseRowId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"TokenComposite snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowLocation conversion.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.TokenComposite>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.TokenComposite>");
                }

                return ResolveTokenCompositeId((IModelSnapshot<Models.TokenComposite>)modelSnapshot, projectDbContext, logger);
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.TokenComposite), "Ref"),
            new[] { nameof(Models.TokenComposite.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), "Ref"),
            new[] { nameof(Models.TokenComposite.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(  
            (typeof(Models.TokenComposite), TokenCompositeBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.TokenComposite.VerseRowId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), TokenCompositeBuilder.TOKEN_LOCATIONS),
            Enumerable.Empty<string>());
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteCompositesByVerseRowIdQueryAsync(Guid verseRowId)
    {
        /*
         * Query to run before deleting a VerseRow (and using its real Id):
DELETE FROM TokenComponent WHERE Id IN
(
    SELECT DISTINCT tco.Id
    FROM TokenComponent tco
    JOIN TokenCompositeTokenAssociation tca on tca.TokenCompositeId = tco.Id
    JOIN TokenComponent tc on tca.TokenId = tc.Id
    JOIN VerseRow vr on tc.VerseRowId = vr.Id
    WHERE vr.Id = '[... some id ...]'
)
         */
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                projectDbContext.RemoveRange(projectDbContext.TokenComposites
                    .Where(tc => tc.TokenCompositeTokenAssociations
                        .Where(tca => tca.Token!.VerseRowId == verseRowId).Any()));

                await Task.CompletedTask;
            };
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteTokenComponentsByVerseRowIdQueryAsync(Guid verseRowId)
    {
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                projectDbContext.RemoveRange(projectDbContext.TokenComponents
                    .Where(tc => tc.VerseRowId == verseRowId));

                await Task.CompletedTask;

            };
    }

    protected static Guid ResolveTokenCompositeId(IModelSnapshot<Models.TokenComposite> modelSnapshot, ProjectDbContext projectDbContext, ILogger logger)
    {
        if (modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.TokenizedCorpusId), out var tokenizedCorpusId) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.EngineTokenId), out var engineTokenId))
        {
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.ParallelCorpusId), out var parallelCorpusId);

            var tokenCompositeId = projectDbContext.TokenComposites
                .Where(e => e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                .Where(e => e.ParallelCorpusId == (Guid?)parallelCorpusId)
                .Where(e => e.EngineTokenId == (string)engineTokenId!)
                .Select(e => e.Id)
                .FirstOrDefault();

            if (tokenCompositeId == default)
            {
                tokenCompositeId = Guid.NewGuid();
                logger.LogDebug($"No TokenComposite Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / ParallelCorpusId ('{parallelCorpusId}') / EngineTokenId ('{engineTokenId}').  Using: '{tokenCompositeId}'");
            }
            else
            {
                logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / ParallelCorpusId ('{parallelCorpusId}') / EngineTokenId ('{engineTokenId}') to TokenComposite Id ('{tokenCompositeId}')");
            }

            return tokenCompositeId;
        }
        else
        {
            throw new PropertyResolutionException($"TokenComposite snapshot does not have all:  TokenizedCorpusId+EngineTokenId, which are required for Id resolution.");
        }
    }

    // FIXME:  need to talk out this logic with someone.  I think what I need to do
    // is approach this at a low level - no business logic - and assume that the
    // snapshot carries across changes to affected Translations and Alignments.

    protected override async Task HandleDeleteAsync(IModelSnapshot<Models.TokenComposite> itemToDelete, CancellationToken cancellationToken)
    {
        await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }

    protected override async Task<Guid> HandleCreateAsync(IModelSnapshot<Models.TokenComposite> itemToCreate, CancellationToken cancellationToken)
    {
        var id = await base.HandleCreateAsync(itemToCreate, cancellationToken);

        // Not only do we need to create the TokenComposite record itself,
        // but we also need to find all TokenLocation tokens and set their
        // TokenCompositeId.

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"In HandleCreateAsync associating new TokenComposite with child Tokens",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var tokenCompositeId = ResolveTokenCompositeId(itemToCreate, projectDbContext, logger);

                var tokenizedCorpusId = (Guid)itemToCreate.PropertyValues["TokenizedCorpusId"]!;
                var parallelCorpusId = (Guid?)itemToCreate.PropertyValues["ParallelCorpusId"];
                var tokenLocations = (IEnumerable<string>)itemToCreate.PropertyValues["TokenLocations"]!;

                var tokenLocationMap2 = (await _mergeContext.MergeBehavior.GetEntityValuesAsync(
                    typeof(Models.Token),
                    new List<string> { "Id", "EngineTokenId" },
                    new Dictionary<string, object> { { "EngineTokenId", tokenLocations } },
                    cancellationToken))
                    .Select(e => (e["EngineTokenId"], Guid.Parse((string)e["Id"])))
                    .ToList();

                var tokensMatchingLocations = projectDbContext.Tokens
                    .Include(e => e.TokenCompositeTokenAssociations)
                    .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                    .Where(e => tokenLocations.Contains(e.EngineTokenId))
                    .Where(e => e.Deleted == null);

                var tokenLocationMap = tokensMatchingLocations
                    .ToDictionary(e => e.EngineTokenId!, e => e);

                var leftover = tokenLocations.Except(tokenLocationMap.Keys);
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", leftover)}");
                }

                var existingAssociatedTokenCompositeIds = tokenLocationMap.Values
                    .SelectMany(t => t.TokenCompositeTokenAssociations
                        .Select(ta => new { ta.TokenCompositeId, ta.TokenComposite!.ParallelCorpusId }))
                    .Where(tap => tap.ParallelCorpusId == parallelCorpusId)
                    .Select(tap => tap.TokenCompositeId)
                    .ToList();

                if (existingAssociatedTokenCompositeIds.Any())
                {
                    logger.LogInformation($"CompositeToken '{tokenCompositeId}' snapshot contains tokens already part of a composite having a ParallelCorpusId (null or non-null) that matches '{parallelCorpusId}'.  Deleting existing/conflicting composite");
                    projectDbContext.RemoveRange(projectDbContext.TokenComposites.Where(e => existingAssociatedTokenCompositeIds.Contains(e.Id)));
                }

                // Add the tokens to the composite:
                var tokenAssocationsToAdd = tokenLocationMap
                    .Select(l => new Models.TokenCompositeTokenAssociation
                    {
                        TokenId = l.Value.Id,
                        TokenCompositeId = tokenCompositeId
                    })
                    .ToList();

                projectDbContext.TokenCompositeTokenAssociations.AddRange(tokenAssocationsToAdd);
                await Task.CompletedTask;
            },
            cancellationToken);

        return id;
    }

    public override async Task<bool> HandleModifyPropertiesAsync(IModelDifference<IModelSnapshot<Models.TokenComposite>> modelDifference, IModelSnapshot<Models.TokenComposite> itemToModify, CancellationToken cancellationToken = default)
    {
        var modified = await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        // If the modifications included changes to which tokens were part of
        // the composite, we need to do two things:
        //  - remove the previous child tokens
        //  - add the new child tokens

        var tokenLocationDifference = modelDifference.PropertyDifferences
            .Where(pd => pd.PropertyName == "TokenLocations")
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ListDifference<string>)))
            .FirstOrDefault();
        if (tokenLocationDifference is null)
        {
            return modified;
        }

        var snapshot = (IModelSnapshot<Models.TokenComposite>)itemToModify;
        var valueListDifference = (ListDifference<string>)tokenLocationDifference.PropertyValueDifference;
        var tokenLocationsOnlyIn1 = valueListDifference.ListMembershipDifference.OnlyIn1;
        var tokenLocationsOnlyIn2 = valueListDifference.ListMembershipDifference.OnlyIn2;

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Applying TokenComposite 'TokenLocations' property ListMembershipDifference (OnlyIn1: {string.Join(", ", tokenLocationsOnlyIn1)}, OnlyIn2: {string.Join(", ", tokenLocationsOnlyIn2)})", 
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var tokenCompositeId = ResolveTokenCompositeId(snapshot, projectDbContext, logger);
                var tokenizedCorpusId = (Guid)snapshot.PropertyValues["TokenizedCorpusId"]!;
                var parallelCorpusId = (Guid?)snapshot.PropertyValues["ParallelCorpusId"];

                var tokenLocationsBoth = tokenLocationsOnlyIn1.Union(tokenLocationsOnlyIn2);

                var tokenLocationMap = projectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                    .Where(e => tokenLocationsBoth.Contains(e.EngineTokenId))
                    .Where(e => e.Deleted == null)
                    .ToDictionary(e => e.EngineTokenId!, e => e);

                var leftover = tokenLocationsBoth.Except(tokenLocationMap.Keys);
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", leftover)}");
                }

                // Remove the OnlyIn1 tokens from the composite:
                var tokenIdsOnlyIn1 = tokenLocationMap
                            .Where(l => tokenLocationsOnlyIn1.Contains(l.Key))
                            .Select(l => l.Value.Id)
                            .ToList();
                var tokenAssocationsToRemove = projectDbContext.TokenCompositeTokenAssociations
                        .Where(a => tokenIdsOnlyIn1.Contains(a.TokenId));

                projectDbContext.RemoveRange(tokenAssocationsToRemove);

                if (tokenLocationMap.Values
                    .SelectMany(t => t.TokenCompositeTokenAssociations
                        .Select(ta => new { ta.TokenCompositeId, ta.TokenComposite!.ParallelCorpusId }))
                    .Any(tap => tap.TokenCompositeId != tokenCompositeId && tap.ParallelCorpusId == parallelCorpusId))
                {
                    throw new PropertyResolutionException($"Merge conflict:  CompositeToken '{tokenCompositeId}' snapshot contains tokens already part of a composite having a ParallelCorpusId (null or non-null) that matches '{parallelCorpusId}'.");
                }

                // Add the OnlyIn2 tokens to the composite:
                var tokenAssocationsToAdd = tokenLocationMap
                    .Where(l => tokenLocationsOnlyIn2.Contains(l.Key)).ToList()
                    .Select(l => new Models.TokenCompositeTokenAssociation
                    {
                        TokenId = l.Value.Id,
                        TokenCompositeId = tokenCompositeId
                    })
                    .ToList();

                projectDbContext.TokenCompositeTokenAssociations.AddRange(tokenAssocationsToAdd);
                await Task.CompletedTask;
            },
            cancellationToken);

        return modified;
    }

    public Expression<Func<Models.TokenComposite, bool>> BuildTokenChildWhereExpression(IModelSnapshot<Models.TokenComposite> snapshot, Guid tokenizedCorpusId, IEnumerable<string> tokenLocations)
    {
        return e => e.GetType() == typeof(Models.Token) &&
                    e.TokenizedCorpusId == tokenizedCorpusId &&
                    tokenLocations.Contains(e.EngineTokenId);
    }
}

