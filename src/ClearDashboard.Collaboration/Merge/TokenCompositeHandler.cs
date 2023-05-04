using System;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DAL.CQRS;
using MediatR;
using ClearDashboard.DAL.Alignment.Corpora;
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
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.Token),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "Token")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.VerseRowId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
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

        mergeContext.MergeBehavior.AddPropertyNameMapping(  
            (typeof(Models.TokenComposite), TokenBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.TokenComposite.VerseRowId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), TokenBuilder.TOKEN_LOCATIONS),
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

                await projectDbContext.TokenComposites
                    .Where(tc => tc.TokenCompositeTokenAssociations
                        .Where(tca => tca.Token!.VerseRowId == verseRowId).Any())
                    .ExecuteDeleteAsync(cancellationToken);

            };
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteTokenComponentsByVerseRowIdQueryAsync(Guid verseRowId)
    {
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                await projectDbContext.TokenComponents
                    .Where(tc => tc.VerseRowId == verseRowId)
                    .ExecuteDeleteAsync(cancellationToken);

            };
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

                var tokenCompositeId = (Guid)itemToCreate.PropertyValues["Id"]!;
                var tokenizedCorpusId = (Guid)itemToCreate.PropertyValues["TokenizedCorpusId"]!;
                var parallelCorpusId = (Guid?)itemToCreate.PropertyValues["ParallelCorpusId"];
                var tokenLocations = (IEnumerable<string>)itemToCreate.PropertyValues["TokenLocations"]!;

                var tokenLocationMap = projectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                    .Where(e => tokenLocations.Contains(e.EngineTokenId))
                    .ToDictionary(e => e.EngineTokenId!, e => e);

                var leftover = tokenLocationMap.Keys.Except(tokenLocations);
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain tokens: {string.Join(", ", leftover)}");
                }

                if (tokenLocationMap.Values
                    .SelectMany(t => t.TokenCompositeTokenAssociations
                        .Select(ta => new { ta.TokenCompositeId, ta.TokenComposite!.ParallelCorpusId }))
                    .Any(tap => tap.TokenCompositeId != tokenCompositeId && tap.ParallelCorpusId == parallelCorpusId))
                {
                    throw new PropertyResolutionException($"Merge conflict:  CompositeToken '{tokenCompositeId}' snapshot contains tokens already part of a composite having a ParallelCorpusId (null or non-null) that matches '{parallelCorpusId}'.");
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

                var tokenCompositeId = (Guid)snapshot.PropertyValues["Id"]!;
                var tokenizedCorpusId = (Guid)snapshot.PropertyValues["TokenizedCorpusId"]!;
                var parallelCorpusId = (Guid?)snapshot.PropertyValues["ParallelCorpusId"];

                var tokenLocationMap = projectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                    .Where(e => tokenLocationsOnlyIn1.Union(tokenLocationsOnlyIn2).Contains(e.EngineTokenId))
                    .ToDictionary(e => e.EngineTokenId!, e => e);

                var leftover = tokenLocationMap.Keys.Except(tokenLocationsOnlyIn1.Union(tokenLocationsOnlyIn2));
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain tokens: {string.Join(", ", leftover)}");
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

